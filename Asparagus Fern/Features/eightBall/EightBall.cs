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
    public static string EightBallAsk1 = "eightball ";
    public static string EightBallAsk2 = "eight ball ";
    public static string EightBallAsk3 = "8ball ";
    public static string EightBallAsk4 = "8 ball ";
}

namespace Asparagus_Fern.Features.EightBall
{
    class EightBall : DiscordIO
    {
        private static string[] responses = {
            "As I see it, yes.",
            "Ask again later.",
            "Better not tell you now.",
            "Cannot predict now.",
            "Concentrate and ask again.",
            "Don’t count on it.",
            "It is certain.",
            "It is decidedly so.",
            "Most likely.",
            "My reply is no.",
            "My sources say no.",
            "Outlook not so good.",
            "Outlook good.",
            "Reply hazy, try again.",
            "Signs point to yes.",
            "Very doubtful.",
            "Without a doubt.",
            "Yes.",
            "Yes – definitely.",
            "You may rely on it.",
        };

        Random rand;

        public EightBall()
        {
            rand = new Random();
        }

        public override Task Message(SocketMessage message, string lowercase, bool isAdmin)
        {
            if (lowercase.StartsWith(Responses.EightBallAsk1)
             || lowercase.StartsWith(Responses.EightBallAsk2)
             || lowercase.StartsWith(Responses.EightBallAsk3)
             || lowercase.StartsWith(Responses.EightBallAsk4))
                EightBallMe(message);

            return base.Message(message, lowercase, isAdmin);
        }

        void EightBallMe(SocketMessage message)
        {
            ulong hash = ((ulong)Math.Abs(message.Content.GetHashCode())) + ((ulong)
                new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, 0, 0).Ticks);

            var embed = new EmbedBuilder()
            {
                Title = $"Hello {message.Author.Username}#{message.Author.DiscriminatorValue.ToString("D4")}",
                ThumbnailUrl = message.Author.GetAvatarUrl(),
                Description = $"**{message.Content}\n\n** {responses[(hash ^ message.Author.Id) % (ulong)responses.Length]}",
                Color = Color.DarkBlue
            }.Build();
            message.Channel.SendMessageAsync(embed: embed);
        }
    }
}
