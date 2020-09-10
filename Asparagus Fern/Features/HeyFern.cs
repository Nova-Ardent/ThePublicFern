using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using System.IO;
using System.Text.Json;
using System.Linq;

namespace Asparagus_Fern.Features
{
    class HeyFern
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
        public HeyFern(DiscordSocketClient client)
        {
            rand = new Random();
            client.MessageReceived += Message;
        }

        private async Task Message(SocketMessage message)
        {
            if (message.Author.IsBot) return;

            if (message.Content.ToLower().Contains("fern"))
            {
                int init = rand.Next(responseInit.Length);
                int mid = rand.Next(responseMid.Length);
                int end = rand.Next(responseEnd.Length);
                await message.Channel.SendMessageAsync($"{responseInit[init]}{responseMid[init]}{responseEnd[init]}");
            }
        }
    }
}
