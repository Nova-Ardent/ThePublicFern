using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using System.IO;
using Asparagus_Fern.Features;

namespace Asparagus_Fern
{
    class Program
    {
#if _WINDOWS
        private const string TOKEN_PATH = "C:\\Token\\AsFern.txt";
#else
        private const string TOKEN_PATH = "/Users/jordanszwed/Documents/AsFern.txt";
#endif

        private string token = null;
        private DiscordSocketClient _client;

        GuessThatRank guessThatRank;
        PartyUp partyUp;

        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            _client = new DiscordSocketClient();
            _client.Log += Log;
            _client.MessageReceived += Message;

            guessThatRank = new GuessThatRank(_client);
            partyUp = new PartyUp(_client);

            using (FileStream fs = File.Open(TOKEN_PATH, FileMode.Open, FileAccess.Read))
            {
                StreamReader sr = new StreamReader(fs);
                token = sr.ReadLine();
            }

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();
            await Task.Delay(-1);
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private Task Message(SocketMessage message)
        {
            if (message.Content.StartsWith("who sucks fern?"))
            {
                message.Channel.SendMessageAsync("Juicy penguin, why?");
            }

            Console.WriteLine(message.Content);
            return Task.CompletedTask;
        }
    }
}
