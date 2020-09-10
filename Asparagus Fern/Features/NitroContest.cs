using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Linq;

namespace Asparagus_Fern.Features
{
    class NitroContest
    {
        [System.Serializable]
        public class NitroContestServers
        {
            public List<NitroContestServer> servers { get; set; } = new List<NitroContestServer>();
        }

        [System.Serializable]
        public class NitroContestServer
        {
            public ulong ID { get; set; }
            public ulong startingPoints { get; set; }
            public ulong pointsPerHour { get; set; }
            public ulong pointsPerMessage { get; set; }
            public string reward { get; set; }
            public List<NitroContestPlayer> players { get; set; } = new List<NitroContestPlayer>();

            public NitroContestServer() { }
            public NitroContestServer(ulong ID)
            {
                this.ID = ID;
            }
        }

        [System.Serializable]
        public class NitroContestPlayer
        {
            public ulong ID { get; set; }
            public string name { get; set; }
            public string Discrim { get; set; }

            public ulong uuid { get; set; }

            public ulong stockedPoints { get; set; }
            public ulong actualPoints { get; set; }

            public NitroContestPlayer() { }
            public NitroContestPlayer(ulong ID, string name, string Discrim, ulong startingPoints)
            {
                this.ID = ID;
                this.name = name;
                this.Discrim = Discrim;

                actualPoints = 0;
                stockedPoints = startingPoints;
            }
        }

        private DiscordSocketClient client;
        public NitroContestServers nitroContestServers;

        private System.Threading.Timer timer;
        private const string nitroContestFile = "nitroContestServers.json";

        private ulong cacheMessageID;
        private bool messageCacheAdmin;

        static readonly Regex trimmer = new Regex(@"\s\s+", RegexOptions.Compiled);

        public NitroContest(DiscordSocketClient client)
        {
            this.client = client;
            client.MessageReceived += Message;

            if (SaveAndLoad.FileExists(Directory.GetCurrentDirectory(), nitroContestFile))
            {
                SaveAndLoad.LoadFile(out nitroContestServers, Directory.GetCurrentDirectory(), nitroContestFile);
            }
            else
            {
                nitroContestServers = new NitroContestServers();
                SaveAndLoad.SaveFile(nitroContestServers, Directory.GetCurrentDirectory(), nitroContestFile);
            }

            timer = new System.Threading.Timer(IntervalTask, null, 0, 1000 * 60 * 60);
        }

        private async Task Message(SocketMessage message)
        {
            if (message.Author.IsBot) return;
            string content = trimmer.Replace(message.Content.ToLower(), " ");

            var guildID = (message.Channel as SocketGuildChannel).Guild.Id;
            if (nitroContestServers.servers.Any(x => x.ID == guildID))
            {
                var server = nitroContestServers.servers.Find(x => x.ID == guildID);
                NitroContestPlayer player = null;
                if (server.players.Any(x => x.ID == message.Author.Id))
                {
                    player = server.players.First(x => x.ID == message.Author.Id);
                    player.stockedPoints += server.pointsPerMessage;
                }

                if (content.StartsWith("nitrocontest help"))
                {
                    await message.Channel.SendMessageAsync(embed: GetHelp(server));
                }

                if (player != null)
                {
                    if (content.StartsWith("nitrovontest vote"))
                    {
                        string[] voting = content.Split(' ');
                        if (voting.Length != 4)
                        {
                            await message.Channel.SendMessageAsync(embed: GetVoteHelp());
                        }
                        else if (voting.Length == 4)
                        {
                            ulong userID;
                            if (message.MentionedUsers.Count == 1)
                            {
                                userID = message.MentionedUsers.First().Id;
                            }
                            else
                            {
                                ulong.TryParse(voting[2], out userID);
                            }

                            var votedPlayer = server.players.Find(x => x.ID == userID);
                            if (votedPlayer != null)
                            {
                                if (votedPlayer.ID == player.ID)
                                {
                                    await message.Channel.SendMessageAsync("you cannot vote for yourself.");
                                }
                                else
                                {
                                    ulong voteAmount = 0;
                                    if (ulong.TryParse(voting[3], out voteAmount))
                                    {
                                        if (voteAmount <= player.stockedPoints)
                                        {
                                            player.actualPoints += voteAmount / 2;
                                            player.stockedPoints -= voteAmount;
                                            votedPlayer.actualPoints += voteAmount;

                                            await message.Channel.SendMessageAsync($"You gave {votedPlayer.name}#{votedPlayer.Discrim} {voteAmount}pts, and gained {voteAmount / 2}");
                                            SaveAndLoad.SaveFile(nitroContestServers, Directory.GetCurrentDirectory(), nitroContestFile);
                                        }
                                        else
                                        {
                                            await message.Channel.SendMessageAsync("You don't have enough stored points to vote that much");
                                        }
                                    }
                                    else
                                    {
                                        await message.Channel.SendMessageAsync("The voting amount could not be understood.");
                                    }
                                }
                            }
                            else
                            {
                                await message.Channel.SendMessageAsync("The player pinged could not be found.");
                            }
                        }
                    }
                    if (content.StartsWith("nitrocontest points"))
                    {
                        var embed = new EmbedBuilder()
                        {
                            Title = $"{player.name}#{player.Discrim}'s points",
                            Description = $"{player.actualPoints} points\n{player.stockedPoints} stored points",
                            Color = new Color(0, 102, 255)
                        }.Build();
                        await message.Channel.SendMessageAsync(embed: embed);
                    }
                }
                else
                {
                    if (content.StartsWith("nitrocontest join"))
                    {
                        if (!server.players.Any(x => x.uuid == message.Author.Id))
                        {
                            NitroContestPlayer p = new NitroContestPlayer(message.Author.Id, message.Author.Username, message.Author.Discriminator, server.startingPoints);
                            server.players.Add(p);

                            await message.Channel.SendMessageAsync($"Welcome aboard {p.name}#{p.Discrim}, you start with {p.stockedPoints} points!. use `NitroContest vote` to vote for other players.");
                            SaveAndLoad.SaveFile(nitroContestServers, Directory.GetCurrentDirectory(), nitroContestFile);
                        }
                    }
                }

                if (content.StartsWith("nitrocontest leaderboard") && server.players.Count() > 0)
                {
                    string[] split = content.Split(' ');
                    int skipAmount = 0;
                    if (content.StartsWith("nitrocontest leaderboard skip") && split.Length == 4 && int.TryParse(split[3], out skipAmount))
                    {
                        if (server.players.Count <= skipAmount)
                        {
                            skipAmount = 0;
                            await message.Channel.SendMessageAsync("that would skip everyone");
                            return; 
                        }
                    }

                    var leaders = server.players.OrderBy(x => x.actualPoints).Skip(skipAmount).Take(10);
                    var leader = leaders.First();
                    var leaderboardProfilePic = (message.Channel as SocketGuildChannel).Guild.GetUser(leader.ID).GetAvatarUrl();

                    EmbedAuthorBuilder author = new EmbedAuthorBuilder() { IconUrl = leaderboardProfilePic, Name = $"{leader.name}#{leader.Discrim}   {leader.actualPoints}pts" };
                    EmbedFooterBuilder footer = new EmbedFooterBuilder() { };
                    if (player != null)
                    {
                        footer.Text = $"You are currently sitting at {player.actualPoints} points, and have {player.stockedPoints} to use.";
                    }

                    int place = skipAmount;
                    var leaderFields = leaders.Select(x =>
                    {
                        place++;

                        string placeStr = "";
                        if (place == 1) placeStr = $"{place}st  :";
                        else if (place == 2) placeStr = $"{place}nd  :";
                        else if (place == 3) placeStr = $"{place}rd  :";
                        else if (place < 10) placeStr = $"{place}    :";
                        else placeStr = $"{place}th :";
                        return new EmbedFieldBuilder() { 
                            Name = $"**`place: {placeStr}`\t\t{x.name}#{x.Discrim}**",
                            Value = $"{x.actualPoints}pts"
                        };
                    }).ToList();

                    var embed = new EmbedBuilder()
                    {
                        Title = "Nitro Contest Leaderboard!",
                        Author = author,
                        Fields = leaderFields,
                        Footer = footer,
                        Color = new Color(0, 102, 255)
                    }.Build();

                    await message.Channel.SendMessageAsync(embed: embed);
                }
                else if (server.players.Count() == 0)
                {
                    await message.Channel.SendMessageAsync("no one is playing :(");
                }
            }

            if (MessageIsFromAdmin(message))
            {
                if (content.StartsWith("nitrocontest server"))
                {
                    if (nitroContestServers.servers.Any(x => x.ID == guildID))
                    {
                        await message.Channel.SendMessageAsync("WHAT ARE YOU DOING!??! This server is already in the contest, are you trying to get extra points?!? <:gun2:750453086853529600>");
                        return;
                    }
                    nitroContestServers.servers.Add(new NitroContestServer(guildID));
                    await message.Channel.SendMessageAsync("Server was set to a nitro contest server! <a:partyblob:751420504640061481>");
                    SaveAndLoad.SaveFile(nitroContestServers, Directory.GetCurrentDirectory(), nitroContestFile);
                }
                else if (content.StartsWith("nitrocontest remove server"))
                {
                    if (!nitroContestServers.servers.Any(x => x.ID == guildID))
                    {
                        await message.Channel.SendMessageAsync("What Server? I don't remember seeing any server.");
                        return;
                    }
                    nitroContestServers.servers = nitroContestServers.servers.Where(x => x.ID != guildID).ToList();
                    await message.Channel.SendMessageAsync("removing discord server from Nitro contests.");
                    SaveAndLoad.SaveFile(nitroContestServers, Directory.GetCurrentDirectory(), nitroContestFile);
                }
                else if (content.StartsWith("nitrocontest set pph") || content.StartsWith("nitrocontest set ppm"))
                {
                    bool isPph = content.StartsWith("nitrocontest set pph");
                    string[] pph = content.Split(' ');
                    if (pph.Length == 4)
                    {
                        ulong n;
                        bool isNumeric = ulong.TryParse(pph[3], out n);
                        if (isNumeric)
                        {
                            var server = nitroContestServers.servers.Find(x => x.ID == guildID);
                            if (isPph)
                            {
                                server.pointsPerHour = n;
                                await message.Channel.SendMessageAsync($"Users will now get {server.pointsPerHour} points per hour.");
                            }
                            else
                            {
                                server.pointsPerMessage = n;
                                await message.Channel.SendMessageAsync($"Users will now get {server.pointsPerMessage} points per message.");
                            }

                            SaveAndLoad.SaveFile(nitroContestServers, Directory.GetCurrentDirectory(), nitroContestFile);
                        }
                        else
                        {
                            await message.Channel.SendMessageAsync("make sure that number you included is a number.");
                        }
                    }
                    else
                    {
                        await message.Channel.SendMessageAsync("try including a number, and nothing else");
                    }
                }
                else if (content.StartsWith("nitrocontest set reward"))
                {
                    int trimLength = "nitrocontest set reward ".Length;
                    if (content.Length > trimLength)
                    {
                        var server = nitroContestServers.servers.Find(x => x.ID == guildID);
                        server.reward = content.Substring(trimLength);
                        await message.Channel.SendMessageAsync($"The reward for nitro contest is now {server.reward}");
                        SaveAndLoad.SaveFile(nitroContestServers, Directory.GetCurrentDirectory(), nitroContestFile);
                    }
                }
            }
        }

        void IntervalTask(Object obj)
        {
            foreach (var server in nitroContestServers.servers)
            {
                foreach (var player in server.players)
                {
                    player.stockedPoints += server.pointsPerHour;
                }
            }
            SaveAndLoad.SaveFile(nitroContestServers, Directory.GetCurrentDirectory(), nitroContestFile);
        }

        Embed GetHelp(NitroContestServer server)
        {
            return new EmbedBuilder() { Title = "Nitro contest!", Description = "**What is it?**\n"
                + $"Nitro Contest is a chance to win {server.reward}. The winner of the contest is whoever is at the top of the leaderboard at the end of the month.\n\n"
                + "**How do I play?**\n"
                + $"To join the contest you can start by using the following command\n\nNitroContest join\n\nAfter joined, every message will earn you {server.pointsPerMessage} stored pts, and once an hour you will get {server.pointsPerHour} stored pts. "
                + "With the stored points, you can vote for other users on the leaderboard. Voting for a user, will give you half the leaderboard points.\n\n"
                + "To learn how to vote for a user. Use the command\n\n\nNitroContest vote\n\n\n"
                + "**How do I tell how I am doing?**\n"
                + "You can tell how you are doing by loading up the leaderboard, or requesting your points. You can request a leaderboard by typing\n\n\n"
                + "NitroContest leaderboard\n\n\n"
                + "and you can check your individual points by typing\n\n\n"
                + "NitroContest points\n\n\n"
                + "**What are the rules?**\n"
                + "1) no alternate accounts\n"
                + "2) no using exploits to get points\n"
                + "3) no self botting or spamming, to farm points"
                + "\nRules are subject to change."
            }.Build();
        }

        Embed GetVoteHelp()
        {
            return new EmbedBuilder() { Title = "How to vote", Description = "Every user has 2 types of points. Leaderboard points \"actual points\" and stored points." +
            " Actual points are achieved when a user is given them by another user. When a user votes for another person, the also get half the points they vote. " +
            "\n\n**Example Vote with 1000 pts:**\n\n\nNitroContest vote @user 1000\n\n\n**Example with UserID:**\n\n\nNitroContest vote 123456789 1000"
            , Color = new Color(0, 102, 255) }.Build(); 
        }

        bool MessageIsFromAdmin(SocketMessage message, bool useCache = true)
        {
            if (useCache && message.Id == cacheMessageID)
            {
                return messageCacheAdmin;
            }

            bool isAdmin = false;
            if (message.Channel is SocketGuildChannel)
            {
                isAdmin = (message.Channel as SocketGuildChannel).Guild.GetUser(message.Author.Id).GuildPermissions.Administrator;
                cacheMessageID = message.Id;
                messageCacheAdmin = isAdmin;
            }

            return isAdmin;
        }
    }
}
