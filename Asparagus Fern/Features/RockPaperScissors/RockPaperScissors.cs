using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using System.IO;
using Asparagus_Fern.Tools;
using System.Collections.Generic;
using System.Timers;

public partial class Responses
{
    public static string HelpRockPaperScissors = "-help rock paper scissors";
    public static string QueueRockPaperScissor = "-queue rock paper scissors";
    public static string ResportUserRockPaperScissors = "-report rock paper scissors";
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
            public int MMR { get; set; }
            public bool queued { get; set; }
        }

        [System.Serializable]
        public class Game
        {
            public uint messageID1;
            public uint messageID2;
            public User user1;
            public User user2;
        }

        public static Color color = Color.LightOrange;

        Dictionary<string, User> users = new Dictionary<string, User>();
        List<User> queue = new List<User>();
        Dictionary<string, (User, User)> activeGames = new Dictionary<string, (User, User)>();
        bool updatedGroup = false;

        public RockPaperScissors()
        {
            if (SaveAndLoad.FileExists("rockPaperScissorsSave.json"))
            {
                SaveAndLoad.LoadFile(out users, "rockPaperScissors", "rockPaperScissorsSave.json");
                SaveAndLoad.LoadFile(out queue, "rockPaperScissors", "queue.json");
            }
        }

        public override Task Message(SocketMessage message, string lowercase, bool isAdmin)
        {
            if (lowercase.Equals(Responses.HelpRockPaperScissors)) Help(message);
            if (lowercase.Equals(Responses.QueueRockPaperScissor)) Queue(message);
            return base.Message(message, lowercase, isAdmin);
        }

        public override void MinuteTask(object source, ElapsedEventArgs e)
        {
            //todo, set up match making.

            List<User> usersToMakeActive;
            if (queue.Count == 1)
            {
                return;
            }
            else if (queue.Count % 2 == 1)
            {
                
            }
            else
            {
            }
        }

        public override void FiveMinuteTask(object source, ElapsedEventArgs e)
        {
            if (updatedGroup)
            {
                updatedGroup = false;
                SaveAndLoad.SaveFile(users, "rockPaperScissors", "rockPaperScissorsSave.json");
                SaveAndLoad.SaveFile(queue, "rockPaperScissors", "queue.json");
            }
            base.FiveMinuteTask(source, e);
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

            queue.Add(user);

            var embed = new EmbedBuilder()
            {
                Title = "Rock paper scissors",
                Description = "You've been queued! you'll be matched up soon.",
                Color = color
            }.Build();
            message.Author.SendMessageAsync(embed: embed);
        }

        public void Help(SocketMessage message)
        {
            var embed = new EmbedBuilder() 
            { Title = "Rock paper scissors", Description = 
                $"To play rock paper, you can join the queue using the command \n\n`{Responses.QueueRockPaperScissor}`\n\n"
                + $"In the queue you'll be placed against a player of similar ELO. When a match is found, "
                + $"you'll be given a message by the asparagus fern. To play react with rock paper or scissors, only the first "
                + $"reaction will be taken. Both people in the match will be told who the winner is.\n\nIf a user has an innapropriate\n"
                + $"name, you may report the user, for review.\n\n`{Responses.ResportUserRockPaperScissors} userID`",
                Color = color
            }.Build();
            message.Author.SendMessageAsync(embed: embed);
        }
    }
}
