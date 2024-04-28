using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Asparagus_Fern.Common;
using Asparagus_Fern.Dice_Roller.DiceProcessor;
using Discord;
using System.Reflection;

public partial class Responses
{
    public enum DiceRoller
    {
        RollHelp,
        Roll,
    }
}

namespace Asparagus_Fern.Dice_Roller
{
    class DiceRoller : DiscordIO
    {
        public override async Task AsyncCommand(Enum command, SocketMessage message, string messageStripped, bool admin)
        {
            if (command is Responses.DiceRoller diceRollerCommand)
            {
                switch (diceRollerCommand)
                {
                    case Responses.DiceRoller.Roll: await RollDice(message, messageStripped); break;
                }
            }   

            await base.AsyncCommand(command, message, messageStripped, admin);
        }

        public async Task RollDice(SocketMessage message, string messageStripped)
        {
            try
            {
                DiceProcessor.DiceProcessor dieProcessor = new DiceProcessor.DiceProcessor(messageStripped);
                string authorURL = message.Author.GetAvatarUrl();

                if (!dieProcessor.IsValid())
                {
                    await DisplayMessage(message, "Die Rolls:", $"The following roll: `{messageStripped}` was invalid.", authorURL);
                    return;
                }

                DieRoll dieRoll = dieProcessor.Roll();

                if (dieRoll.Rolls.Count > 5)
                {
                    await DisplayDie(message, dieRoll, 0, authorURL);
                    await message.Channel.SendMessageAsync("...");
                    await DisplayDie(message, dieRoll, dieRoll.Rolls.Count - 1, authorURL);
                    await DisplayDieResults(message, dieRoll, authorURL, messageStripped);
                }
                else
                {
                    for (int i = 0; i < dieRoll.Rolls.Count; i++)
                    {
                        await DisplayDie(message, dieRoll, i, authorURL);
                    }
                    await DisplayDieResults(message, dieRoll, authorURL, messageStripped);
                }

                await Task.CompletedTask;
            }
            catch (Exception e)
            {
                string authorURL = message.Author.GetAvatarUrl();
                await DisplayMessage(message, "Die Rolls:", $"The following roll: `{messageStripped}` was invalid or failed to send.", authorURL);
            }
        }

        public async Task DisplayDie(SocketMessage message, DieRoll roll, int index, string thumbNail)
        {
            await DisplayMessage(message, $"Die Roll: #{index}", $"#\n__{roll.Rolls[index]}__", thumbNail);
        }

        public async Task DisplayDieResults(SocketMessage message, DieRoll roll, string thumbNail, string rollData)
        {
            await DisplayMessage(message, $"Roll Result: {rollData}", $"#\n__{roll.Result}__", thumbNail);
        }

        public override string FeatureName()
        {
           return "Dice Roller";
        }

        public override Enum HelpCommand()
        {
            return Responses.DiceRoller.RollHelp;
        }

        public override Color FeatureColor()
        {
            return new Color(255, 150, 150);
        }
    }
}
