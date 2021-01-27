using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using System.IO;
using Asparagus_Fern.Tools;
using System.Collections.Generic;
using System.Timers;
using System.Linq;

public partial class Responses
{
    public static string HelpRockPaperScissors = "help rock paper scissors";
    public static string QueueRockPaperScissor = "queue rock paper scissors";
    public static string QueueRockPaperScissorShort1 = "qrps";
    public static string GetStatsRockPaperScissors = "stats rock paper scissors";
}

namespace Asparagus_Fern.Features.RockPaperScissors
{
    class RockPaperScissors : DiscordIO
    {
        [System.Serializable]
        public class User
        {
            public string username { get; set; }
            public ulong userID { get; set; }
            public ushort discrim { get; set; }
            public float MMR { get; set; }
            [NonSerialized] public bool queued;
            public int wins { get; set; }
            public int draws { get; set; }
            public int losses { get; set; }
            public SocketUser cachedUser;
        }

        [System.Serializable]
        public class Game
        {
            public ulong messageID1;
            public ulong messageID2;
            public User user1;
            public User user2;
            public int user1reaction = -1;
            public int user2reaction = -1;
        }

        public static Color color = Color.LightOrange;

        Dictionary<string, User> users = new Dictionary<string, User>();
        List<User> queue = new List<User>();
        Dictionary<ulong, Game> activeGames = new Dictionary<ulong, Game>();
        bool updatedGroup = false;

        public RockPaperScissors()
        {
            if (SaveAndLoad.FileExists("rockPaperScissors", "rockPaperScissorsSave.json"))
            {
                SaveAndLoad.LoadFile(out users, "rockPaperScissors", "rockPaperScissorsSave.json");
            }
        }

        float GetMMRDif(float winner, float loser)
        {
            float dif = 0;
            if (winner == 0)
            {
                dif = 2;
            }
            else
            {
                dif = Math.Clamp(loser / winner, 0.5f, 2f);
            }

            return (float)(10 * Math.Pow(2, dif));
        }

        public override Task Message(SocketMessage message, string lowercase, bool isAdmin)
        {
            if (lowercase.Equals(Responses.HelpRockPaperScissors)) Help(message);
            else if (lowercase.Equals(Responses.QueueRockPaperScissor)) Queue(message);
            else if (lowercase.Equals(Responses.QueueRockPaperScissorShort1)) Queue(message);
            else if (lowercase.Equals(Responses.GetStatsRockPaperScissors)) GetStats(message);
            return base.Message(message, lowercase, isAdmin);
        }

        public override async void MinuteTask(object source, ElapsedEventArgs e)
        {
            List<User> usersToMakeActive;
            if (queue.Count == 1 || queue.Count == 0)
            {
                return;
            }
            else if (queue.Count % 2 == 1)
            {
                usersToMakeActive = queue.Take(queue.Count - 1).OrderBy(x => x.MMR).ToList();
                queue = queue.Skip(queue.Count - 1).Take(1).ToList();
            }
            else
            {
                usersToMakeActive = queue.OrderBy(x => x.MMR).ToList();
                queue = new List<User>();
            }

            int groups = usersToMakeActive.Count / 2;
            for (int i = 0; i < groups; i++)
            {
                Game game = new Game();
                var users = usersToMakeActive.Skip(i * 2).Take(2).ToArray();

                game.user1 = users[0];
                game.user2 = users[1];

                var embed = new EmbedBuilder()
                {
                    Title = "Rock Paper Scissors",
                    Description = $"Your match up is\n**{game.user1.MMR}-MMR {game.user1.username}\n{game.user2.MMR}-MMR {game.user2.username}**",
                    Color = color
                }.Build();

                var m1 = await game.user1.cachedUser.SendMessageAsync(embed: embed);
                var m2 = await game.user2.cachedUser.SendMessageAsync(embed: embed);
                game.messageID1 = m1.Id;
                game.messageID2 = m2.Id;

                await m1.AddReactionAsync(new Emoji(EmojiList.rock));
                await m1.AddReactionAsync(new Emoji(EmojiList.scroll));
                await m1.AddReactionAsync(new Emoji(EmojiList.scissors));

                await m2.AddReactionAsync(new Emoji(EmojiList.rock));
                await m2.AddReactionAsync(new Emoji(EmojiList.scroll));
                await m2.AddReactionAsync(new Emoji(EmojiList.scissors));

                activeGames[game.messageID1] = game;
                activeGames[game.messageID2] = game;
            }
        }

        public override void FiveMinuteTask(object source, ElapsedEventArgs e)
        {
            if (updatedGroup)
            {
                updatedGroup = false;
                SaveAndLoad.SaveFile(users, "rockPaperScissors", "rockPaperScissorsSave.json");
            }
            base.FiveMinuteTask(source, e);
        }

        public void GetStats(SocketMessage message)
        {
            User user;
            if (users.ContainsKey(message.Author.Id.ToString()))
            {
                user = users[message.Author.Id.ToString()];
            }
            else
            {
                user = new User()
                {
                    username = message.Author.Username,
                    userID = message.Author.Id,
                    discrim = message.Author.DiscriminatorValue,
                    MMR = 500,
                    queued = false,
                };
                users[message.Author.Id.ToString()] = user;
            }

            var embed = new EmbedBuilder()
            {
                Title = $"{message.Author.Username}",
                Description = $"current MMR: {user.MMR.ToString("F1")}\nwins: {user.wins}\nlosses: {user.losses}\ndraws: {user.draws}\n",
                Color = color
            }.Build();
            message.Channel.SendMessageAsync(embed: embed);
        }

        public void Queue(SocketMessage message)
        {
            updatedGroup = true;

            User user;
            if (users.ContainsKey(message.Author.Id.ToString()))
            {
                user = users[message.Author.Id.ToString()];
                if (user.queued)
                {
                    var res = new EmbedBuilder()
                    {
                        Title = "Rock paper scissors",
                        Description = "You've already been queued.",
                        Color = color
                    }.Build();
                    message.Author.SendMessageAsync(embed: res);
                    return;
                }
                user.queued = true;
            }
            else
            {
                user = new User()
                {
                    username = message.Author.Username,
                    userID = message.Author.Id,
                    discrim = message.Author.DiscriminatorValue,
                    MMR = 500,
                    queued = true,
                };
                users[message.Author.Id.ToString()] = user;
            }

            user.cachedUser = message.Author;
            queue.Add(user);

            var embed = new EmbedBuilder()
            {
                Title = "Rock paper scissors",
                Description = "You've been queued! you'll be matched up soon.",
                Color = color
            }.Build();
            message.Author.SendMessageAsync(embed: embed);
        }

        public override async Task OnReaction(Cacheable<IUserMessage, ulong> cachedMessage, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (activeGames.ContainsKey(cachedMessage.Id) && reaction.User.IsSpecified && !reaction.User.Value.IsBot)
            {
                var game = activeGames[cachedMessage.Id];
                
                if (game.messageID1 == cachedMessage.Id)
                {
                    if (reaction.Emote.Name.Equals(EmojiList.rock)) game.user1reaction = 0;
                    if (reaction.Emote.Name.Equals(EmojiList.scroll)) game.user1reaction = 1;
                    if (reaction.Emote.Name.Equals(EmojiList.scissors)) game.user1reaction = 2;
                }
                
                if (game.messageID2 == cachedMessage.Id)
                {
                    if (reaction.Emote.Name.Equals(EmojiList.rock)) game.user2reaction = 0;
                    if (reaction.Emote.Name.Equals(EmojiList.scroll)) game.user2reaction = 1;
                    if (reaction.Emote.Name.Equals(EmojiList.scissors)) game.user2reaction = 2;
                }

                if (game.user1reaction != -1 && game.user2reaction != -1)
                {
                    activeGames.Remove(game.messageID1);
                    activeGames.Remove(game.messageID2);

                    int win = 0;
                    if (game.user1reaction == game.user2reaction)
                    {
                        win = 0;
                    }
                    else if (game.user1reaction == 0 && game.user2reaction == 2) win = 1;
                    else if (game.user1reaction == 2 && game.user2reaction == 0) win = 2;
                    else if (game.user1reaction == 1 && game.user2reaction == 0) win = 1;
                    else if (game.user1reaction == 0 && game.user2reaction == 1) win = 2;
                    else if (game.user1reaction == 2 && game.user2reaction == 1) win = 1;
                    else if (game.user1reaction == 1 && game.user2reaction == 2) win = 2;

                    if (win == 0)
                    {
                        await game.user1.cachedUser.SendMessageAsync($"Your game against {game.user2.username} was a tied.");
                        await game.user2.cachedUser.SendMessageAsync($"Your game against {game.user1.username} was a tied.");
                        game.user1.draws++;
                        game.user2.draws++;
                    }
                    else if (win == 1)
                    {
                        float mmr = GetMMRDif(game.user1.MMR, game.user2.MMR);
                        if (mmr >= game.user2.MMR)
                        {
                            mmr = game.user2.MMR;
                        }

                        await game.user1.cachedUser.SendMessageAsync($"Your game against {game.user2.username} was won! you gained {mmr.ToString("F1")} MMR and are now at {(game.user1.MMR + mmr).ToString("F1")}.");
                        await game.user2.cachedUser.SendMessageAsync($"Your game against {game.user1.username} was lost! you lost {mmr.ToString("F1")} MMR and are now at {(game.user2.MMR - mmr).ToString("F1")}.");

                        game.user1.MMR = game.user1.MMR + mmr;
                        game.user1.wins++;

                        game.user2.MMR = game.user2.MMR - mmr;
                        game.user2.losses++;
                    }
                    else if (win == 2)
                    {
                        float mmr = GetMMRDif(game.user2.MMR, game.user1.MMR);
                        if (mmr >= game.user1.MMR)
                        {
                            mmr = game.user1.MMR;
                        }

                        await game.user2.cachedUser.SendMessageAsync($"Your game against {game.user1.username} was won! you gained {mmr} MMR and are now at {(game.user2.MMR + mmr).ToString("F1")}.");
                        await game.user1.cachedUser.SendMessageAsync($"Your game against {game.user2.username} was lost! you lost {mmr} MMR and are now at {(game.user1.MMR - mmr).ToString("F1")}.");

                        game.user2.MMR = game.user2.MMR + mmr;
                        game.user2.wins++;

                        game.user1.MMR = game.user1.MMR - mmr;
                        game.user1.losses++;
                    }

                    game.user1.cachedUser = null;
                    game.user2.cachedUser = null;
                    game.user1.queued = false;
                    game.user2.queued = false;
                }
            }

            await base.OnReaction(cachedMessage, channel, reaction);
        }

        public override async Task OnRemoveReaction(Cacheable<IUserMessage, ulong> cachedMessage, ISocketMessageChannel channel, SocketReaction reaction)
        {
            await base.OnReaction(cachedMessage, channel, reaction);
        }

        public void Help(SocketMessage message)
        {
            var embed = new EmbedBuilder() 
            { Title = "Rock paper scissors", Description = 
                $"To play rock paper, you can join the queue using the command \n\n`{Responses.QueueRockPaperScissor}`\n\n"
                + $"In the queue you'll be placed against a player of similar ELO. When a match is found, "
                + $"you'll be given a message by the asparagus fern. To play react with rock paper or scissors, only the first "
                + $"reaction will be taken. Both people in the match will be told who the winner is.\n\n",
                Color = color
            }.Build();
            message.Author.SendMessageAsync(embed: embed);
        }
    }
}
