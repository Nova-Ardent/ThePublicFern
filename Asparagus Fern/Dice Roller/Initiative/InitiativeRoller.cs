using Asparagus_Fern.Common;
using Discord;
using Discord.WebSocket;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Asparagus_Fern.Dice_Roller.DiceProcessor;
using static Asparagus_Fern.Dice_Roller.Initiative.InitiativeRoller;
using Discord.Rest;
public partial class Responses
{
    public enum Initiative
    {
        HelpInitiative,
        NewI,
        NewInitiative,
        AddI,
        AddInitiative,
        NameI,
        NameInitiative,
        RollI,
        RollInitiative,
        ShowI,
        ShowInitiative,
        StartI,
        StartInitiative,
    }
}

namespace Asparagus_Fern.Dice_Roller.Initiative
{
    class InitiativeRoller : DiscordIO
    {
        public enum InitiativeState
        {
            NeedsName,
            NeedsRoll,
            Done,
        }

        public class InitiativeID
        {
            public string Character;
            public DiceProcessor.DiceProcessor Roll;
            public int Initiative = int.MinValue;
            public InitiativeState State = InitiativeState.NeedsName;
        }

        public class InitiativeIDs
        {
            public string User;
            public List<InitiativeID> ActiveIniativeIDs = new List<InitiativeID>();

            public InitiativeState CurrentState()
            {
                if (ActiveIniativeIDs.Count == 0)
                {
                    return InitiativeState.Done;
                }
                else
                {
                    return ActiveIniativeIDs.Last().State;
                }
            }
        }

        public class Initiative
        {
            public bool Started
            {
                get => Initiatives.Count > 0 && Initiatives.All(x => x.Value.CurrentState() == InitiativeState.Done) && OrderedInitiatives.Count > 0;
            }
            public ulong Chat;
            public Dictionary<ulong, InitiativeIDs> Initiatives = new Dictionary<ulong, InitiativeIDs>();
            public List<InitiativeID> OrderedInitiatives = new List<InitiativeID>();
            public int turn = 0;
            public ulong lastMessageID = 0;
        }

        Dictionary<ulong, Initiative> activeInitiatives = new Dictionary<ulong, Initiative>();

        public override async Task AsyncCommand(Enum command, SocketMessage message, string messageStripped, bool admin)
        {
            if (command is Responses.Initiative intitiaveCommand)
            {
                switch (intitiaveCommand)
                {
                    case Responses.Initiative.NewI:
                    case Responses.Initiative.NewInitiative:
                        await StartNewInitiative(message);
                        break;
                    case Responses.Initiative.AddI:
                    case Responses.Initiative.AddInitiative:
                        await AddInitiative(message);
                        break;
                    case Responses.Initiative.NameI:
                    case Responses.Initiative.NameInitiative:
                        await NameInitiative(message, messageStripped);
                        break;
                    case Responses.Initiative.RollI:
                    case Responses.Initiative.RollInitiative:
                        await RollInitiative(message, messageStripped);
                        break;
                    case Responses.Initiative.ShowI:
                    case Responses.Initiative.ShowInitiative:
                        await ShowInitiative(message);
                        break;
                    case Responses.Initiative.StartInitiative:
                    case Responses.Initiative.StartI:
                        await StartInitiative(message);
                        break;
                }
            }
        }

        public async Task StartNewInitiative(SocketMessage message)
        {
            activeInitiatives[message.Channel.Id] = new Initiative();
            activeInitiatives[message.Channel.Id].Chat = message.Channel.Id;
            await DisplayMessage(message, "Initiative", "A new initiative has started! use `add initiative` to start a new character");
        }

        public async Task AddInitiative(SocketMessage message)
        {
            if (!activeInitiatives.ContainsKey(message.Channel.Id))
            {
                await DisplayMessage(message, "Initiative", "No initiative has been started in this channel.");
                return;
            }

            if (activeInitiatives[message.Channel.Id].Initiatives.ContainsKey(message.Author.Id)
                && activeInitiatives[message.Channel.Id].Initiatives[message.Author.Id].CurrentState() != InitiativeState.Done)
            {
                if (activeInitiatives[message.Channel.Id].Initiatives[message.Author.Id].CurrentState() == InitiativeState.NeedsRoll)
                {
                    await DisplayMessage(message, "Initiative", "You already are working on a character, it currently needs a roll. use `roll initiative`");
                }
                else if (activeInitiatives[message.Channel.Id].Initiatives[message.Author.Id].CurrentState() == InitiativeState.NeedsName)
                {
                    await DisplayMessage(message, "Initiative", "You already are working on a character, it currently needs a name. use `name initiative`");
                }
                return;
            }
            else
            {
                if (!activeInitiatives[message.Channel.Id].Initiatives.ContainsKey(message.Author.Id))
                {
                    activeInitiatives[message.Channel.Id].Initiatives[message.Author.Id] = new InitiativeIDs();
                }

                activeInitiatives[message.Channel.Id].Initiatives[message.Author.Id].ActiveIniativeIDs.Add(new InitiativeID());
                activeInitiatives[message.Channel.Id].Initiatives[message.Author.Id].ActiveIniativeIDs.Last().State = InitiativeState.NeedsName;
                activeInitiatives[message.Channel.Id].Initiatives[message.Author.Id].User = message.Author.Username;
                await DisplayMessage(message, "Initiative", "New initiative character created! please provide a name for your character. use `name initiative`.");
            }
        }

        public async Task NameInitiative(SocketMessage message, string messageStripped)
        {
            if (!activeInitiatives.ContainsKey(message.Channel.Id))
            {
                await DisplayMessage(message, "Initiative", "No initiative has been started in this channel.");
                return;
            }

            if (activeInitiatives[message.Channel.Id].Initiatives.ContainsKey(message.Author.Id)
                && activeInitiatives[message.Channel.Id].Initiatives[message.Author.Id].CurrentState() == InitiativeState.NeedsName)
            {
                if (string.IsNullOrWhiteSpace(messageStripped))
                {
                    await DisplayMessage(message, "Initiative", "You must provide a name for your character.");
                    return;
                }

                activeInitiatives[message.Channel.Id].Initiatives[message.Author.Id].ActiveIniativeIDs.Last().Character = messageStripped;
                activeInitiatives[message.Channel.Id].Initiatives[message.Author.Id].ActiveIniativeIDs.Last().State = InitiativeState.NeedsRoll;
                await DisplayMessage(message, "Initiative", "Please set a roll for your character, use `roll initiative`.");
            }
            else if (activeInitiatives[message.Channel.Id].Initiatives.ContainsKey(message.Author.Id)
                && activeInitiatives[message.Channel.Id].Initiatives[message.Author.Id].CurrentState() == InitiativeState.NeedsRoll)
            {
                await DisplayMessage(message, "Initiative", "You are currently working on a character, it needs a roll. use `roll initiative`.");
            }
            else
            {
                await DisplayMessage(message, "Initiative", "You are not currently working on a character. use `add initiative` to start a new character.");
            }
        }

        public async Task RollInitiative(SocketMessage message, string messageStripped)
        {
            if (!activeInitiatives.ContainsKey(message.Channel.Id))
            {
                await DisplayMessage(message, "Initiative", "No initiative has been started in this channel.");
                return;
            }

            if (activeInitiatives[message.Channel.Id].Initiatives.ContainsKey(message.Author.Id)
                && activeInitiatives[message.Channel.Id].Initiatives[message.Author.Id].CurrentState() == InitiativeState.NeedsName)
            {
                await DisplayMessage(message, "Initiative", "You are currently working on a character, it needs a name. use `name initiative`.");
            }
            else if (activeInitiatives[message.Channel.Id].Initiatives.ContainsKey(message.Author.Id)
                && activeInitiatives[message.Channel.Id].Initiatives[message.Author.Id].CurrentState() == InitiativeState.NeedsRoll)
            {
                activeInitiatives[message.Channel.Id].Initiatives[message.Author.Id].ActiveIniativeIDs.Last().Roll = new DiceProcessor.DiceProcessor(messageStripped);
                if (activeInitiatives[message.Channel.Id].Initiatives[message.Author.Id].ActiveIniativeIDs.Last().Roll.IsValid())
                {
                    activeInitiatives[message.Channel.Id].Initiatives[message.Author.Id].ActiveIniativeIDs.Last().State = InitiativeState.Done;
                    await DisplayMessage(message, "Initiative", "New Character Added!");

                    if (activeInitiatives[message.Channel.Id].Started)
                    {
                        activeInitiatives[message.Channel.Id].Initiatives[message.Author.Id].ActiveIniativeIDs.Last().Initiative = activeInitiatives[message.Channel.Id].Initiatives[message.Author.Id].ActiveIniativeIDs.Last().Roll.Roll().Result;
                        activeInitiatives[message.Channel.Id].OrderedInitiatives.Add(activeInitiatives[message.Channel.Id].Initiatives[message.Author.Id].ActiveIniativeIDs.Last());
                        activeInitiatives[message.Channel.Id].OrderedInitiatives = activeInitiatives[message.Channel.Id].OrderedInitiatives.OrderByDescending(x => x.Initiative).ToList();
                        await ShowInitiative(message);
                    }
                }
                else
                {
                    await DisplayMessage(message, "Initiative", "That roll format is invalid.!");
                }
            }
            else
            {
                await DisplayMessage(message, "Initiative", "You are not currently working on a character. use `add initiative` to start a new character.");
            }
        }

        public async Task ShowInitiative(SocketMessage message)
        {
            await ShowInitiative(message.Channel);
        }

        public async Task ShowInitiative(ISocketMessageChannel Channel)
        {
            if (!activeInitiatives.ContainsKey(Channel.Id))
            {
                await DisplayMessage(Channel, "Initiative", "No initiative has been started in this channel.");
                return;
            }

            if (!activeInitiatives[Channel.Id].Started)
            {
                int i = 0;
                string initiatives = "";
                foreach (var initiative in activeInitiatives[Channel.Id].Initiatives)
                {
                    foreach (var initiativeID in initiative.Value.ActiveIniativeIDs)
                    {
                        i++;
                        if (initiativeID.State == InitiativeState.Done)
                            initiatives += $"**{i}**: {initiativeID.Character} - {initiativeID.Roll}\n";
                        else if (initiativeID.State == InitiativeState.NeedsRoll)
                            initiatives += $"**{i}**: {initiativeID.Character} - Needs Roll\n";
                        else
                            initiatives += $"**{i}** {initiative.Value.User}'s un-named character";
                    }
                }

                await DisplayMessage(Channel, "Initiative", initiatives);
            }
            else
            {
                int i = 0;
                string initiatives = "";
                foreach (var initiativeID in activeInitiatives[Channel.Id].OrderedInitiatives)
                {
                    string initiativeString = i == activeInitiatives[Channel.Id].turn ? "⚔️" : "";

                    i++;
                    if (initiativeID.State == InitiativeState.Done)
                        initiatives += $"**{i}**: {initiativeID.Character} - {initiativeID.Roll}:\t {initiativeID.Initiative} **{initiativeString}**\n";
                }

                RestUserMessage displayMessage = await DisplayMessage(Channel, "Initiative", initiatives);
                await displayMessage.AddReactionAsync(new Emoji("✅"));
                await displayMessage.AddReactionAsync(new Emoji("🟥"));
                activeInitiatives[Channel.Id].lastMessageID = displayMessage.Id;
            }
        }

        public async Task StartInitiative(SocketMessage message)
        {
            if (!activeInitiatives.ContainsKey(message.Channel.Id))
            {
                await DisplayMessage(message, "Initiative", "No initiative has been started in this channel.");
                return;
            }

            if (activeInitiatives[message.Channel.Id].Initiatives.Count == 0)
            {
                await DisplayMessage(message, "Initiative", "No characters have been added to the initiative.");
                return;
            }

            if (activeInitiatives[message.Channel.Id].Started)
            {
                await DisplayMessage(message, "Initiative", "Initiative Already Started.");
                await ShowInitiative(message);
                return;
            }

            bool CanCreateInititative = true;
            foreach (var initiative in activeInitiatives[message.Channel.Id].Initiatives)
            {
                foreach (var initiativeID in initiative.Value.ActiveIniativeIDs)
                {
                    if (initiativeID.State != InitiativeState.Done)
                    {
                        await DisplayMessage(message, "Initiative", $"All characters must have a name and roll before starting the initiative. {initiative.Value.User} please finish your character.");
                        CanCreateInititative = false;
                    }
                    else
                    {
                        initiativeID.Initiative = initiativeID.Roll.Roll().Result;
                        activeInitiatives[message.Channel.Id].OrderedInitiatives.Add(initiativeID);
                    }
                }
            }

            if (CanCreateInititative)
            {
                activeInitiatives[message.Channel.Id].OrderedInitiatives = activeInitiatives[message.Channel.Id].OrderedInitiatives.OrderByDescending(x => x.Initiative).ToList();
            }
            else
            {
                activeInitiatives[message.Channel.Id].OrderedInitiatives.Clear();
                return;
            }

            await ShowInitiative(message);
        }

        public override async Task OnReaction(Cacheable<IUserMessage, ulong> cachedMessage, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (!reaction.User.IsSpecified || reaction.User.Value.IsBot) 
                return;

            if (activeInitiatives.ContainsKey(channel.Id))
            {
                if (reaction.Emote.Name == "✅")
                {
                    activeInitiatives[channel.Id].turn = (activeInitiatives[channel.Id].turn + 1) % activeInitiatives[channel.Id].OrderedInitiatives.Count;
                    await ShowInitiative(channel);
                }
                else if (reaction.Emote.Name == "🟥")
                {
                    activeInitiatives[channel.Id].turn = activeInitiatives[channel.Id].turn - 1;
                    if (activeInitiatives[channel.Id].turn < 0)
                    {
                        activeInitiatives[channel.Id].turn = activeInitiatives[channel.Id].OrderedInitiatives.Count - 1;
                    }
                    await ShowInitiative(channel);
                }
            }
        }

        public override Enum HelpCommand()
        {
            return Responses.Initiative.HelpInitiative;
        }

        public override string FeatureName()
        {
            return "Dice Roller";
        }
    }
}
