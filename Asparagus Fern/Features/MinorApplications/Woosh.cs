using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Asparagus_Fern.Tools;


namespace Asparagus_Fern.Features.MinorApplications
{
    class Woosh : DiscordIO
    {
        private string[] responseInit = new string[] {
            "uhhh...",
            "",
            "wooooosh",
            "wooosh",
            "Woosh",
            "uhhh whoosh."
        };
        private string[] responseMid = new string[] {
            "",
            " Woooooooooosh",
            " wosh",
            " woosh",
            " woooosh...",
            " wooooooooosh",
            " WOOOOOOOOSH",
            " ..."
        };
        private string[] responseEnd = new string[] {
            "...",
            " stop talking about the plant.",
            " uhhh... yeah. woosh",
            " woosh",
            " woooosh...",
            " wooooooooosh",
            " WOOOOOOOOSH",
        };

        Random rand;
        public Woosh()
        {
            rand = new Random();
        }

        public override async Task AsyncMessage(SocketMessage message, string lowercase, bool isAdmin)
        {
            if (message.Author.IsBot) await base.AsyncMessage(message, lowercase, isAdmin);

            if (message.Content.ToLower().Contains("fern"))
            {
                int init = rand.Next(responseInit.Length);
                int mid = rand.Next(responseMid.Length);
                int end = rand.Next(responseEnd.Length);
                await message.Channel.SendMessageAsync($"{responseInit[init]}{responseMid[mid]}{responseEnd[end]}");
            }
        }
    }
}
