using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using System.IO;
using Asparagus_Fern.Tools;
using Asparagus_Fern.Common;
using Asparagus_Fern.Dice_Roller;
using System.Reflection;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using Asparagus_Fern.GPTFern;
using Asparagus_Fern.Dice_Roller.Initiative;

namespace Asparagus_Fern
{
    class Program
    {
#if _WINDOWS
        private const string TOKEN_PATH = "C:\\Token\\AsFern.txt";
#else
        private const string TOKEN_PATH = ../token/token.txt";
#endif
        public static string DataPath = "Data";

        private string token = null;
        private string gpttoken = null;
        private DiscordSocketClient _client;
        private DiscordSocketRestClient _restClient;

        List<Func<SocketGuildUser, Task>> joinedList = new List<Func<SocketGuildUser, Task>>();
        List<Func<SocketMessage, string, bool, Task>> messageRecievedList = new List<Func<SocketMessage, string, bool, Task>>();
        List<Func<Enum, SocketMessage, string, bool, Task>> commandRecievedList = new List<Func<Enum, SocketMessage, string, bool, Task>>();
        List<Func<SocketMessage, string, bool, Task>> messageAsyncRecievedList = new List<Func<SocketMessage, string, bool, Task>>();
        List<Func<Enum, SocketMessage, string, bool, Task>> commandAsyncRecievedList = new List<Func<Enum, SocketMessage, string, bool, Task>>();
        List<Action<object, ElapsedEventArgs>> timed5MinFunctionList = new List<Action<object, ElapsedEventArgs>>();
        List<Action<object, ElapsedEventArgs>> timed1MinFunctionList = new List<Action<object, ElapsedEventArgs>>();
        List<Action<object, ElapsedEventArgs>> timed30secFunctionList = new List<Action<object, ElapsedEventArgs>>();
        List<Action<object, ElapsedEventArgs>> timed10secFunctionList = new List<Action<object, ElapsedEventArgs>>();

        FernHelp fernHelp = new FernHelp();
        DiscordIO[] features;

        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            using (FileStream fs = File.Open(TOKEN_PATH, FileMode.Open, FileAccess.Read))
            {
                StreamReader sr = new StreamReader(fs);
                token = sr.ReadLine();
                gpttoken = sr.ReadLine();
            }

            GPTFernIO gpt = new GPTFernIO(gpttoken);

            features = new DiscordIO[] {
                fernHelp,
                new Acronym(),
                new DiceRoller(),
                new InitiativeRoller(),
                gpt,
            };
            fernHelp.AddFeatures(features);
            foreach (var feature in features)
            {
                await feature.Init();
            }

            _client = new DiscordSocketClient();
            _client.Log += Log;
            _client.MessageReceived += Message;
            _client.MessageReceived += AsyncMessage;
            _client.UserJoined += JoinedServer;

            _restClient = _client.Rest;

            var a5Timer = new System.Timers.Timer(5 * 60 * 1000);
            a5Timer.Elapsed += new ElapsedEventHandler(TimedFunctionFiveMinutes);
            a5Timer.Start();

            var a1Timer = new System.Timers.Timer(30 * 1000);
            a1Timer.Elapsed += new ElapsedEventHandler(TimedFunctionOneMinutes);
            a1Timer.Start();

            var a30sTimer = new System.Timers.Timer(30 * 1000);
            a30sTimer.Elapsed += new ElapsedEventHandler(TimedFunction30Seconds);
            a30sTimer.Start();

            var a10sTimer = new System.Timers.Timer(10 * 1000);
            a10sTimer.Elapsed += new ElapsedEventHandler(TimedFunction10Seconds);
            a10sTimer.Start();

            foreach (var feature in features)
            {
                feature.client = _client;
                feature.restClient = _restClient;
                joinedList.Add(feature.Joined);
                messageRecievedList.Add(feature.Message);
                commandRecievedList.Add(feature.Command);
                messageAsyncRecievedList.Add(feature.AsyncMessage);
                commandAsyncRecievedList.Add(feature.AsyncCommand);
                timed5MinFunctionList.Add(feature.FiveMinuteTask);
                timed1MinFunctionList.Add(feature.MinuteTask);
                timed30secFunctionList.Add(feature.ThirtySecondTask);
                timed10secFunctionList.Add(feature.TenSecondTask);
                _client.Connected += feature.Connected;
                _client.Connected += feature.SetRest;
                _client.LoggedOut += feature.Logout;
                _client.ReactionAdded += feature.OnReaction;
                _client.ReactionRemoved += feature.OnRemoveReaction;
            }

            TextWriter errorWriter = Console.Error;
            Responses.CompileResponses();

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            gpt.SetBotID(_client.Rest.CurrentUser.Id);
            
            await Task.Delay(-1);
        }

        Task Log(LogMessage msg)
        {
            return Task.CompletedTask;
        }

        async Task JoinedServer(SocketGuildUser joined)
        {
            foreach (var joinedFunctions in joinedList)
            {
                await joinedFunctions(joined);
            }
        }

        Task Message(SocketMessage message)
        {    
            if (message.Author.IsBot) return Task.CompletedTask;

            bool isAdmin = false;
            if (message.Channel is SocketGuildChannel)
            {
                var user = (message.Channel as SocketGuildChannel).Guild.GetUser(message.Author.Id);
                isAdmin = user != null && user.GuildPermissions.Administrator;
            }

            string content = message.Content.ToLower();
            Enum command = Responses.SearchForCommand(content);

            if (command != null)
            {
                int length = DiscordIO.EnumToCommand(command).Length;
                int contentLength = content.Length - length;
                string contentStripped = content.Substring(DiscordIO.EnumToCommand(command).Length, contentLength).Trim();

                foreach (var funcs in commandRecievedList)
                {
                    try
                    {
                        funcs(command, message, contentStripped, isAdmin);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }

            foreach (var funcs in messageRecievedList)
            {
                try
                {
                    funcs(message, content, isAdmin);
                }
                    catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return Task.CompletedTask;
        }

        async Task AsyncMessage(SocketMessage message)
        {
            if (message.Author.IsBot) return;

            bool isAdmin = false;
            if (message.Channel is SocketGuildChannel)
            {
                var user = (message.Channel as SocketGuildChannel).Guild.GetUser(message.Author.Id);
                isAdmin = user != null && user.GuildPermissions.Administrator;
            }

            string content = message.Content.ToLower();
            Enum command = Responses.SearchForCommand(content);

            if (command != null)
            {
                int length = DiscordIO.EnumToCommand(command).Length;
                int contentLength = content.Length - length;
                string contentStripped = content.Substring(DiscordIO.EnumToCommand(command).Length, contentLength).Trim();

                foreach (var funcs in commandAsyncRecievedList)
                {
                    try
                    {
                        await funcs(command, message, contentStripped, isAdmin);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }

            foreach (var funcs in messageAsyncRecievedList)
            {
                try
                {
                    await funcs(message, content, isAdmin);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
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

        void TimedFunction30Seconds(object source, ElapsedEventArgs e)
        {
            foreach (var funcs in timed30secFunctionList)
            {
                funcs(source, e);
            }
        }

        void TimedFunction10Seconds(object source, ElapsedEventArgs e)
        {
            foreach (var funcs in timed10secFunctionList)
            {
                funcs(source, e);
            }
        }
    }
}
