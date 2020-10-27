using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using System.IO;
using Asparagus_Fern.Features.RealmQuest.Managers;
using Asparagus_Fern.Tools;

namespace Asparagus_Fern.Features.RealmQuest
{
    class RealmQuest
    {
        DiscordSocketClient client;
        ulong ownerID;

        public static QuestMasterData questMasterData = new QuestMasterData();

        QuestCreationManager questCreationManager;
        CharacterManager characterManager;

        public static EmbedAuthorBuilder ead = new EmbedAuthorBuilder() { Name = "Realm Quest" };

        public RealmQuest(DiscordSocketClient client)
        {
            this.client = client;
            this.client.MessageReceived += Message;
            this.client.Connected += OnConnected;

            questCreationManager = new QuestCreationManager(this.client);
            characterManager = new CharacterManager(this.client);

            this.client.MessageReceived += characterManager.Command;
            this.client.MessageReceived += questCreationManager.Command;
            this.client.ReactionAdded += characterManager.OnReaction;

            if (SaveAndLoad.FileExists(Directory.GetCurrentDirectory(), QuestMasterData.savePath))
            {
                SaveAndLoad.LoadFile(out questMasterData, Directory.GetCurrentDirectory(), QuestMasterData.savePath);
            }
            else
            {
                SaveAndLoad.SaveFile(questMasterData, Directory.GetCurrentDirectory(), QuestMasterData.savePath);
            }
        }

        public async Task OnConnected()
        {
            var applicationInfo = await this.client.GetApplicationInfoAsync();
            ownerID = applicationInfo.Owner.Id;
            questMasterData.owner = applicationInfo.Owner.Id;
            questMasterData.ownerName = applicationInfo.Owner.Username;
            questMasterData.ownerDescrim = applicationInfo.Owner.DiscriminatorValue.ToString("D4");

            Console.WriteLine($"Found owner {applicationInfo.Owner.Username}#{applicationInfo.Owner.DiscriminatorValue.ToString("D4")}");
            questMasterData.Save();
        }

        private async Task Message(SocketMessage message)
        {
            if (message.Author.IsBot) return;
            if (message.Content.StartsWith(";"))
            {
                if (message.Content.StartsWith(characterManager.helpCommand)) await characterManager.Help(message);
                else if (message.Content.StartsWith(questCreationManager.questMasterHelpCommand)) await questCreationManager.Help(message);
                else if (message.Content.StartsWith(";help")) await Help(message);
            }
        }

        private async Task Help(SocketMessage message)
        {
            bool QM = questMasterData.IsQuestMaster(message.Author.Id);
            bool hasCharacter = false;

            var embed = new EmbedBuilder()
            {
                Author = ead,
                Color = Color.Blue,
                Description =
                    "Welcome to game Realm Quest!\n\n" +
                    (QM ? "Oh hey! you're a quest master. You may already know this, but as a quest master there are a large variety of things you can do. Like Create items, quests, enemies, you name it! What would you like to do?" +
                    "\n\nOh, need some help figuring things out? Here..\n\n" +
                    $"Creating quests!\n{questCreationManager.questMasterHelpCommand}\n\n"
                    : "Oh hello! I didn't see you there, sorry. ") +
                    (hasCharacter ? 
                    "Welcome back! What would you like to do today?\n\n" +
                    $"character creation!\n{characterManager.helpCommand}"
                    : "You must be new here! lets get you started on making a character shall we?\n\n" + 
                    $"character creation!\n{characterManager.helpCommand}")
            }.Build();
            await message.Channel.SendMessageAsync(embed: embed);
        }
    }
}
