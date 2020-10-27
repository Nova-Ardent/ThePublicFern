using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using System.Linq;
using Asparagus_Fern.Features.RealmQuest.Managers.Data;
using System.Text.RegularExpressions;
using Asparagus_Fern.Tools;

namespace Asparagus_Fern.Features.RealmQuest.Managers
{
    public class CharacterManager : Manager
    {
        public class Message
        {
            public ulong UserID;
            public DateTime time;
        }

        public class DeleteMessage : Message { }
        public class ConfirmationMessage : Message { }
        public class StatsMessage : Message 
        {
            public ulong vit;
            public ulong str;
            public ulong dex;
            public ulong intel;
            public ulong luck;
        }

        public static string saveFiles = "realmCharacters";

        public override string helpCommand => ";help character";
        public override string questMasterHelpCommand => "";

        public static string createCharacterCommand = ";create character";
        public static string createCharacterNameCommand = createCharacterCommand + " name";
        public static string deleteCharacterCommand = ";delete character";

        public static string characterCommand = ";character";

        static readonly Regex trimmer = new Regex(@"\s\s+", RegexOptions.Compiled);
        static readonly Regex alphaSpace = new Regex("^[a-zA-Z\\s]+$");

        static readonly string checkMark = "<:check:768578804741832714>";
        static readonly string xMark = "<:x_:768579331071934484>";

        List<Message> cachedMessages = new List<Message>();

        public CharacterManager(DiscordSocketClient client)
        {
            SaveAndLoad.CheckPathAndCreate(saveFiles);
        }

        public async Task Command(SocketMessage message)
        {
            if (message.Author.IsBot) return;
            string content = trimmer.Replace(message.Content.ToLower(), " ");

            if (content.StartsWith(createCharacterCommand)) await CreateCharacter(message, content);
            else if (content.StartsWith(characterCommand)) await GetChacter(message);
            else if (content.StartsWith(deleteCharacterCommand)) await DeleteCharacter(message, content);
        }

        public async Task OnReaction(Cacheable<IUserMessage, ulong> cachedMessage, ISocketMessageChannel channel, SocketReaction reaction)
        {
            SocketUser user = (SocketUser)reaction.User;
            if (user.IsBot) return;

            CharacterData characterData = GetCharacter(reaction.UserId);
            Message mostRecentMessage;

            if (characterData.name != null && characterData.type == AdventurerType.None)
            {
                AdventurerAttribute attribute = null;
                AdventurerType type = AdventurerType.None;
                foreach (var adventurerType in Enum.GetValues(typeof(AdventurerType)).Cast<AdventurerType>())
                {
                    attribute = EnumExtensions.GetAttribute<AdventurerAttribute>(adventurerType);
                    if ($"<:{reaction.Emote.Name}:{((Emote)reaction.Emote).Id}>".Equals(attribute.emote))
                    {
                        type = adventurerType;
                        break;
                    }
                }

                if (type == AdventurerType.None) return;

                characterData.type = type;
                SaveAndLoad.SaveFile(characterData, saveFiles, $"{characterData.userID}.json");
                var embed = new EmbedBuilder()
                {
                    Author = RealmQuest.ead,
                    Color = Color.Blue,
                    Description = $"Oh nice {characterData.name}, you're a {attribute.name}. Are you ready to start rolling for your stats?"
                }.Build();
                var sendMessage = await reaction.Channel.SendMessageAsync(embed: embed);
                await sendMessage.AddReactionAsync(Emote.Parse(checkMark));
                await sendMessage.AddReactionAsync(Emote.Parse(xMark));

                Message confirmMessage = new ConfirmationMessage()
                {
                    UserID = characterData.userID,
                    time = DateTime.Now,
                };

                cachedMessages = cachedMessages.Where(x => x.UserID != characterData.userID).ToList();
                cachedMessages.Add(confirmMessage);
                SaveAndLoad.SaveFile(characterData, saveFiles, $"{characterData.userID}.json");
            }
            else if (characterData.name != null && characterData.type != AdventurerType.None)
            {
                mostRecentMessage = cachedMessages.FirstOrDefault(x => x.UserID == characterData.userID);
                if (mostRecentMessage != null && (
                    (mostRecentMessage is ConfirmationMessage && $"<:{reaction.Emote.Name}:{((Emote)reaction.Emote).Id}>".Equals(checkMark)) || 
                    (mostRecentMessage is StatsMessage && $"<:{reaction.Emote.Name}:{((Emote)reaction.Emote).Id}>".Equals(xMark))))
                {
                    AdventurerAttribute attribute = EnumExtensions.GetAttribute<AdventurerAttribute>(characterData.type);
                    ulong[] GetPointsPerStat = attribute.GetPointsPerStat();

                    List<EmbedFieldBuilder> embedFieldBuilders = new List<EmbedFieldBuilder>();
                    embedFieldBuilders.Add(new EmbedFieldBuilder() { Name = $"vitality",        Value = GetPointsPerStat[0], IsInline = true });
                    embedFieldBuilders.Add(new EmbedFieldBuilder() { Name = $"Strength",        Value = GetPointsPerStat[1], IsInline = true });
                    embedFieldBuilders.Add(new EmbedFieldBuilder() { Name = $"dexterity",       Value = GetPointsPerStat[2], IsInline = true });
                    embedFieldBuilders.Add(new EmbedFieldBuilder() { Name = $"intelligence",    Value = GetPointsPerStat[3], IsInline = true });
                    embedFieldBuilders.Add(new EmbedFieldBuilder() { Name = $"luck",            Value = GetPointsPerStat[4], IsInline = true });
                    embedFieldBuilders.Add(new EmbedFieldBuilder() { Name = $"{checkMark} keep stats", Value = $"{xMark} roll again", IsInline = true });

                    var embed = new EmbedBuilder()
                    {
                        Author = RealmQuest.ead,
                        Color = Color.Blue,
                        Description = $"How do you like these stats {characterData.name}? These seem pretty good.",
                        Fields = embedFieldBuilders
                    }.Build();

                    var sendMessage = await reaction.Channel.SendMessageAsync(embed: embed);
                    await sendMessage.AddReactionAsync(Emote.Parse(checkMark));
                    await sendMessage.AddReactionAsync(Emote.Parse(xMark));

                    Message statsMessage = new StatsMessage
                    {
                        vit = GetPointsPerStat[0],
                        str = GetPointsPerStat[1],
                        dex = GetPointsPerStat[2],
                        intel = GetPointsPerStat[3],
                        luck = GetPointsPerStat[4],
                        UserID = mostRecentMessage.UserID,
                        time = DateTime.Now,
                    };
                    cachedMessages = cachedMessages.Where(x => x.UserID != characterData.userID).ToList();
                    cachedMessages.Add(statsMessage);
                }
                else if (mostRecentMessage is StatsMessage && $"<:{reaction.Emote.Name}:{((Emote)reaction.Emote).Id}>".Equals(checkMark))
                {
                    StatsMessage stats = mostRecentMessage as StatsMessage;
                    characterData.vitality = stats.vit;
                    characterData.strength = stats.str;
                    characterData.dexterity = stats.dex;
                    characterData.intelligence = stats.intel;
                    characterData.luck = stats.luck;
                    characterData.level = 0;
                    characterData.experience = 0;

                    cachedMessages = cachedMessages.Where(x => x.UserID != characterData.userID).ToList();
                    await GetChacter(reaction.Message.IsSpecified ? reaction.Message.Value : null, characterData.userID, reaction.Channel, characterData);
                    SaveAndLoad.SaveFile(characterData, saveFiles, $"{characterData.userID}.json");
                }
            }

            mostRecentMessage = cachedMessages.FirstOrDefault(x => x.UserID == characterData.userID);
            if (mostRecentMessage is DeleteMessage && $"<:{reaction.Emote.Name}:{((Emote)reaction.Emote).Id}>".Equals(checkMark))
            {
                SaveAndLoad.DeleteFile(saveFiles, $"{characterData.userID}.json");

                var embed = new EmbedBuilder()
                {
                    Author = RealmQuest.ead,
                    Color = Color.Blue,
                    Description = $"Your character has been deleted",
                }.Build();
                await reaction.Channel.SendMessageAsync(embed: embed);
            }
            else if (mostRecentMessage is DeleteMessage && $"<:{reaction.Emote.Name}:{((Emote)reaction.Emote).Id}>".Equals(xMark))
            {
                cachedMessages = cachedMessages.Where(x => x.UserID != characterData.userID).ToList();

                var embed = new EmbedBuilder()
                {
                    Author = RealmQuest.ead,
                    Color = Color.Blue,
                    Description = $"The delete message has been deactivated",
                }.Build();
                await reaction.Channel.SendMessageAsync(embed: embed);
            }
        }

        public async Task CreateCharacter(SocketMessage message, string content)
        {
            CharacterData characterData = GetCharacter(message.Author.Id);
            if (CharacterCompleted(message.Author.Id, characterData))
            {
                return;
            }

            if (characterData.name == null)
            {
                if (content.StartsWith(createCharacterNameCommand))
                {
                    Embed embed;
                    string name = content.Remove(0, createCharacterNameCommand.Length);
                    if (alphaSpace.IsMatch(name))
                    {
                        name = name.Trim().ToTitleCase();
                    }
                    else
                    {
                        embed = new EmbedBuilder()
                        {
                            Author = RealmQuest.ead,
                            Color = Color.Blue,
                            Description = $"I'm sorry, your name can only be spaces, and alphabetical characters."
                        }.Build();
                        await message.Channel.SendMessageAsync(embed: embed);
                        return;
                    }

                    var response = await message.Channel.SendMessageAsync(embed: GetAdventurerTypeEmbed(name, false));
                    foreach (var adventurerType in Enum.GetValues(typeof(AdventurerType)).Cast<AdventurerType>())
                    {
                        AdventurerAttribute attribute = EnumExtensions.GetAttribute<AdventurerAttribute>(adventurerType);
                        if (!String.IsNullOrWhiteSpace(attribute.emote))
                        {
                            await response.AddReactionAsync(Emote.Parse(attribute.emote));
                        }
                    }
                    characterData.name = name;
                    SaveAndLoad.SaveFile(characterData, saveFiles, $"{message.Author.Id}.json");
                }
                else 
                {
                    var embed = new EmbedBuilder()
                    {
                        Author = RealmQuest.ead,
                        Color = Color.Blue,
                        Description = "How are you, can we get your name?\n\n"
                             + $"create your name!\n{createCharacterNameCommand} **name**"
                    }.Build();
                    await message.Channel.SendMessageAsync(embed: embed);
                }
            }
            else if (characterData.type == AdventurerType.None)
            {
                var response = await message.Channel.SendMessageAsync(embed: GetAdventurerTypeEmbed(characterData.name, true));
                foreach (var adventurerType in Enum.GetValues(typeof(AdventurerType)).Cast<AdventurerType>())
                {
                    AdventurerAttribute attribute = EnumExtensions.GetAttribute<AdventurerAttribute>(adventurerType);
                    if (!String.IsNullOrWhiteSpace(attribute.emote))
                    {
                        await response.AddReactionAsync(Emote.Parse(attribute.emote));
                    }
                }
            }
            else if (characterData.level == -1)
            {
                var embed = new EmbedBuilder()
                {
                    Author = RealmQuest.ead,
                    Color = Color.Blue,
                    Description = $"Welcome back {characterData.name}, are you ready now to roll for your stats?"
                }.Build();
                var sendMessage = await message.Channel.SendMessageAsync(embed: embed);
                await sendMessage.AddReactionAsync(Emote.Parse(checkMark));
                await sendMessage.AddReactionAsync(Emote.Parse(xMark));

                Message cachedMessage = new ConfirmationMessage()
                {
                    UserID = characterData.userID,
                    time = DateTime.Now,
                };

                cachedMessages = cachedMessages.Where(x => x.UserID != characterData.userID).ToList();
                cachedMessages.Add(cachedMessage);
            }
        }

        public async Task DeleteCharacter(SocketMessage message, string content)
        {
            CharacterData characterData = GetCharacter(message.Author.Id);
            var name = content.Substring(deleteCharacterCommand.Length).Trim().ToTitleCase();
            if (name.Equals(characterData.name))
            {
                var embed = new EmbedBuilder()
                {
                    Author = RealmQuest.ead,
                    Color = Color.Blue,
                    Description = $"Are you sure you want to delete your character? This action __Cannot__ be undone."
                }.Build();
                var sendMessage = await message.Channel.SendMessageAsync(embed: embed);
                await sendMessage.AddReactionAsync(Emote.Parse(checkMark));
                await sendMessage.AddReactionAsync(Emote.Parse(xMark));

                Message cachedMessage = new DeleteMessage()
                {
                    UserID = characterData.userID,
                    time = DateTime.Now,
                };

                cachedMessages = cachedMessages.Where(x => x.UserID != characterData.userID).ToList();
                cachedMessages.Add(cachedMessage);
            }
        }

        public Embed GetAdventurerTypeEmbed(string name, bool welcomeBack)
        {
            List<EmbedFieldBuilder> embedFieldBuilders = new List<EmbedFieldBuilder>();
            foreach (var adventurerType in Enum.GetValues(typeof(AdventurerType)).Cast<AdventurerType>())
            {
                AdventurerAttribute attribute = EnumExtensions.GetAttribute<AdventurerAttribute>(adventurerType);
                if (!String.IsNullOrWhiteSpace($"{attribute.emote} {attribute.name}") && !String.IsNullOrWhiteSpace(attribute.statDescription))
                {
                    embedFieldBuilders.Add(new EmbedFieldBuilder()
                    {
                        Name = $"{attribute.emote} {attribute.name}",
                        Value = attribute.statDescription,
                        IsInline = true
                    });
                }
            }

            var embed = new EmbedBuilder()
            {
                Author = RealmQuest.ead,
                Color = Color.Blue,
                Description = welcomeBack ? $"Hello again {name}, I don't think you ever told me what type of you are?" : $"Nice to meet you {name}, what kind of adventurer are you?",
                Fields = embedFieldBuilders
            }.Build();
            return embed;
        }

        public async Task GetChacter(SocketMessage message, ulong? authorID = null, ISocketMessageChannel channel = null, CharacterData characterData = null)
        {
            characterData = characterData ?? GetCharacter(authorID ?? message.Author.Id);
            bool isComplete = CharacterCompleted(authorID ?? message.Author.Id, characterData);

            if (isComplete)
            {
                AdventurerAttribute adventureAttribute = EnumExtensions.GetAttribute<AdventurerAttribute>(characterData.type);

                List<EmbedFieldBuilder> embedFieldBuilders = new List<EmbedFieldBuilder>();
                embedFieldBuilders.Add(new EmbedFieldBuilder() { Name = $"level: {characterData.level}",        Value = $"experience: {characterData.experience}",     IsInline = true });
                embedFieldBuilders.Add(new EmbedFieldBuilder() { Name = $"vitality",        Value = characterData.vitality,     IsInline = true });
                embedFieldBuilders.Add(new EmbedFieldBuilder() { Name = $"Strength",        Value = characterData.strength,     IsInline = true });
                embedFieldBuilders.Add(new EmbedFieldBuilder() { Name = $"dexterity",       Value = characterData.dexterity,    IsInline = true });
                embedFieldBuilders.Add(new EmbedFieldBuilder() { Name = $"intelligence",    Value = characterData.intelligence, IsInline = true });
                embedFieldBuilders.Add(new EmbedFieldBuilder() { Name = $"luck",            Value = characterData.luck,         IsInline = true });

                var embed = new EmbedBuilder()
                {
                    Author = RealmQuest.ead,
                    Title = $"Name: {characterData.name} {adventureAttribute.emote}",
                    Color = Color.Blue,
                    Fields = embedFieldBuilders
                }.Build();
                await (channel ?? message.Channel).SendMessageAsync(embed: embed);
            }
            else
            {
                var embed = new EmbedBuilder()
                {
                    Author = RealmQuest.ead,
                    Title = "Please Complete your character before checking the character page.",
                    Color = Color.Blue,
                    Description = $"You can use this command to continue working on your character\n{createCharacterCommand}"
                }.Build();
                await (channel ?? message.Channel).SendMessageAsync(embed: embed);
            }
        }

        public CharacterData GetCharacter(ulong userID)
        {
            CharacterData characterData = new CharacterData(userID);
            if (SaveAndLoad.FileExists(saveFiles, $"{userID}.json"))
            {
                SaveAndLoad.LoadFile(out characterData, saveFiles, $"{userID}.json");
            }

            return characterData;
        }

        public bool CharacterCompleted(ulong userID, CharacterData cache = null)
        {
            if (cache == null)
            {
                cache = GetCharacter(userID);
            }

            if (cache.name == null) return false;
            if (cache.type == AdventurerType.None) return false;
            if (cache.level == -1) return false;
            return true;
        }

        public override async Task Help(SocketMessage message)
        {
            bool userExists = CharacterCompleted(message.Author.Id);

            var embed = new EmbedBuilder()
            {
                Author = RealmQuest.ead,
                Color = Color.Blue,
                Description = (userExists
                ? "Oh Hello! what can I do for you today?\n\n"
                + $"Delete your current character\n{deleteCharacterCommand} **Character Name**\n\n"
                + $"Show your characters profile\n{characterCommand}\n\n"
                : "Oh! looks like you're new here, I'd like to get to know you!\n\n"
                + $"Create your character\n{createCharacterCommand}")
            }.Build();
            await message.Channel.SendMessageAsync(embed: embed);
            await base.Help(message);
        }
    }
}
