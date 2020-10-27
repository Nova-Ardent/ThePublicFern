using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Asparagus_Fern.Tools;


namespace Asparagus_Fern.Features.RealmQuest.Managers
{
    public class QuestCreationManager : Manager
    {
        DiscordSocketClient client;

        public override string helpCommand => ";help quests";
        public override string questMasterHelpCommand => ";help create quests";

        static readonly Regex trimmer = new Regex(@"\s\s+", RegexOptions.Compiled);

        string createStoryBoard = ";create story board";
        string helpStoryBoard = ";help story board";
        string createQuestMaster = ";create quest master";
        string removeQuestMaster = ";remove quest master";
        string getQuestMasters = ";get quest masters";

        public QuestCreationManager(DiscordSocketClient client)
        {
            this.client = client;
        }

        public async Task Command(SocketMessage message)
        {
            if (message.Author.IsBot || !RealmQuest.questMasterData.IsQuestMaster(message.Author.Id)) return;
            string content = trimmer.Replace(message.Content.ToLower(), " ");

            if (RealmQuest.questMasterData.IsOwner(message.Author.Id) && content.StartsWith(createQuestMaster)) await CreateOrRemoveQuestMaster(message, content, true);
            else if (RealmQuest.questMasterData.IsOwner(message.Author.Id) && content.StartsWith(removeQuestMaster)) await CreateOrRemoveQuestMaster(message, content, false);
            else if (RealmQuest.questMasterData.IsQuestMaster(message.Author.Id) && content.StartsWith(getQuestMasters)) await GetQuestMasters(message, content);
        }

        async Task GetQuestMasters(SocketMessage message, string content)
        {
            List<EmbedFieldBuilder> embedFieldBuilders = new List<EmbedFieldBuilder>();

            foreach (var masters in RealmQuest.questMasterData.questMasters)
            {
                embedFieldBuilders.Add(new EmbedFieldBuilder()
                {
                    Name = $"{masters.name}#{masters.discrim}",
                    Value = $"Created {masters.createdQuests} quests\nQuests played {masters.timesPlayed} times",
                    IsInline = true
                });
            }

            var embed = new EmbedBuilder()
            {
                Author = RealmQuest.ead,
                Color = Color.Gold,
                Description = $"A list of all the current Quest Masters. Realm Quest owner: **{RealmQuest.questMasterData.ownerName}#{RealmQuest.questMasterData.ownerDescrim}**",
                Fields = embedFieldBuilders
            }.Build();
            await message.Channel.SendMessageAsync(embed: embed);
        }

        async Task CreateOrRemoveQuestMaster(SocketMessage message, string content, bool add)
        {
            if (message.MentionedUsers.Count != 0)
            {
                List<EmbedFieldBuilder> embedFieldBuilders = new List<EmbedFieldBuilder>();
                Action<ulong, string, string> onFound = (inc, uname, disc) =>
                {
                    embedFieldBuilders.Add(new EmbedFieldBuilder()
                    {
                        Name = $"{inc})",
                        Value = $"{uname}#{disc}",
                        IsInline = true
                    });
                };

                int i = 0;
                if (add)
                {
                    foreach(var user in message.MentionedUsers)
                    {
                        if (RealmQuest.questMasterData.IsQuestMaster(user.Id)) continue;

                        onFound(user.Id, user.Username, user.DiscriminatorValue.ToString("D4"));
                        RealmQuest.questMasterData.questMasters.Add(new QuestMasterData.User
                        {
                            id = user.Id,
                            name = user.Username,
                            discrim = user.DiscriminatorValue.ToString("D4")
                        });
                    }
                }
                else
                {
                    RealmQuest.questMasterData.questMasters = RealmQuest.questMasterData.questMasters.Where(x =>
                    {
                        if (message.MentionedUsers.Any(y => y.Id == x.id))
                        {
                            onFound(x.id, x.name, x.discrim);
                            return false;
                        }
                        return true;
                    }).ToList();
                }


                if (embedFieldBuilders.Count != 0)
                {
                    var embed = new EmbedBuilder()
                    {
                        Author = RealmQuest.ead,
                        Color = Color.Gold,
                        Description = $"The following users have been {(add ? "set as" : "removed from")} Quest Masters",
                        Fields = embedFieldBuilders
                    }.Build();
                    await message.Channel.SendMessageAsync(embed: embed);
                    SaveAndLoad.SaveFile(RealmQuest.questMasterData, Directory.GetCurrentDirectory(), QuestMasterData.savePath);
                }
                else
                {
                    var embed = new EmbedBuilder()
                    {
                        Author = RealmQuest.ead,
                        Color = Color.Gold,
                        Description = add ? "all selected users are already Quest Masters." : "none of the selected users were Quest Masters."
                    }.Build();
                    await message.Channel.SendMessageAsync(embed: embed);
                }
            }
            else
            {
                var embed = new EmbedBuilder()
                {
                    Author = RealmQuest.ead,
                    Color = Color.Gold,
                    Description = "include the users you would like to make as quest masters"
                }.Build();
                await message.Channel.SendMessageAsync(embed: embed);
            }
        }

        public override async Task Help(SocketMessage message)
        {
            if (message.Author.IsBot || !RealmQuest.questMasterData.IsQuestMaster(message.Author.Id)) return;

            var embed = new EmbedBuilder()
            {
                Author = RealmQuest.ead,
                Color = Color.Gold,
                Description = "The following commands are only accessible by quest masters, individuals that are given permission to create quests. "
                    + $"If you would like to request becoming a quest master, message {RealmQuest.questMasterData.ownerName}#{RealmQuest.questMasterData.ownerDescrim}\n\n"
                    + $"Start a story board\n{createStoryBoard}\n\n"
                    + $"Get help creating stories\n{helpStoryBoard}\n\n"
                    + $"Get a list of quest masters\n{getQuestMasters}\n\n"
                    + (RealmQuest.questMasterData.IsOwner(message.Author.Id) ? $"Add a new quest master\n{createQuestMaster}\n\nRemove a quest master\n{removeQuestMaster}" : "")
            }.Build();
            await message.Channel.SendMessageAsync(embed: embed);
            await base.Help(message); 
        }
    }
}
