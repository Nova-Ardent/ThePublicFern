using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using System.IO;
using Asparagus_Fern.Tools;
using Asparagus_Fern.Features.RockPaperScissors;
using Asparagus_Fern.Features.EightBall;
using Asparagus_Fern.Features.ReactionRoles;
using Asparagus_Fern.Features.MinorApplications;
using System.Reflection;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Timers;

public partial class Responses
{
    public static IEnumerable<string> GetAllResponses()
    {
        return typeof(Responses)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(string))
            .Select(f => (string)f.GetValue(null));
    }
}

namespace Asparagus_Fern
{
    class Program
    {
#if _WINDOWS
        private const string TOKEN_PATH = "C:\\Token\\AsFern.txt";
#else
        private const string TOKEN_PATH = "/Users/bleh/Documents/AsFern.txt";
#endif
        
        private string token = null;
        private DiscordSocketClient _client;
        private DiscordSocketRestClient _restClient;

        List<Func<SocketMessage, string, bool, Task>> messageRecievedList = new List<Func<SocketMessage, string, bool, Task>>();
        List<Func<SocketMessage, string, bool, Task>> messageAsyncRecievedList = new List<Func<SocketMessage, string, bool, Task>>();
        List<Action<object, ElapsedEventArgs>> timed5MinFunctionList = new List<Action<object, ElapsedEventArgs>>();
        List<Action<object, ElapsedEventArgs>> timed1MinFunctionList = new List<Action<object, ElapsedEventArgs>>();

        DiscordIO[] features = new DiscordIO[] { 
            new RockPaperScissors(),
            new EightBall(),
            new ReactionRoles(),
            new PercentResponse(),
            new Woosh()
        };

        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            _client = new DiscordSocketClient();
            _client.Log += Log;
            _client.MessageReceived += Message;
            _client.MessageReceived += AsyncMessage;

            _restClient = _client.Rest;

            var a5Timer = new System.Timers.Timer(5 * 60 * 1000);
            a5Timer.Elapsed += new ElapsedEventHandler(TimedFunctionFiveMinutes);
            a5Timer.Start();

            var a1Timer = new System.Timers.Timer(60 * 1000);
            a1Timer.Elapsed += new ElapsedEventHandler(TimedFunctionOneMinutes);
            a1Timer.Start();

            foreach (var feature in features)
            {
                feature.client = _client;
                feature.restClient = _restClient;
                messageRecievedList.Add(feature.Message);
                messageAsyncRecievedList.Add(feature.AsyncMessage);
                timed5MinFunctionList.Add(feature.FiveMinuteTask);
                timed1MinFunctionList.Add(feature.MinuteTask);
                _client.Connected += feature.Connected;
                _client.Connected += feature.SetRest;
                _client.LoggedOut += feature.Logout;
                _client.ReactionAdded += feature.OnReaction;
                _client.ReactionRemoved += feature.OnRemoveReaction;
            }

            TextWriter errorWriter = Console.Error;
            var res = Responses.GetAllResponses().GroupBy(v => v);
            foreach (var responses in res)
            {
                if (responses.Count() > 1)
                {
                    errorWriter.WriteLine($"WARNING YOU HAVE A DUPLICATE RESPONSE: \"{responses.Key}\" : {responses.Count()}");
                }
            }

            using (FileStream fs = File.Open(TOKEN_PATH, FileMode.Open, FileAccess.Read))
            {
                StreamReader sr = new StreamReader(fs);
                token = sr.ReadLine();
            }

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();
            await Task.Delay(-1);
        }

        Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        Task Message(SocketMessage message)
        {
            bool isAdmin = false;
            if (message.Channel is SocketGuildChannel)
            {
                var user = (message.Channel as SocketGuildChannel).Guild.GetUser(message.Author.Id);
                isAdmin = user != null && user.GuildPermissions.Administrator;
            }

            string content = message.Content.ToLower();

            foreach (var funcs in messageRecievedList)
            {
                funcs(message, content, isAdmin);
            }
            return Task.CompletedTask;
        }

        async Task AsyncMessage(SocketMessage message)
        {
            bool isAdmin = false;
            if (message.Channel is SocketGuildChannel)
            {
                var user = (message.Channel as SocketGuildChannel).Guild.GetUser(message.Author.Id);
                isAdmin = user != null && user.GuildPermissions.Administrator;
            }

            string content = message.Content.ToLower();

            foreach (var funcs in messageAsyncRecievedList)
            {
                await funcs(message, content, isAdmin);
            }
        }

        void TimedFunctionFiveMinutes(object source, ElapsedEventArgs e)
        {
            foreach (var funcs in timed5MinFunctionList)
            {
                funcs(source, e);
            }
        }

        void TimedFunctionOneMinutes(object source, ElapsedEventArgs e)
        {
            foreach (var funcs in timed1MinFunctionList)
            {
                funcs(source, e);
            }
        }
    }
}
