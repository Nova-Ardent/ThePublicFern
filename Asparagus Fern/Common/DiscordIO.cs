using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using System.IO;
using System.Collections.Generic;
using System.Timers;
using System.Text.RegularExpressions;

namespace Asparagus_Fern.Common
{
    public abstract class DiscordIO<T> : DiscordIO where T : Enum
    {
        public override string HelpMessage(bool isAdmin)
        {
            string response = "";
            foreach (var command in Utilities.GetEnums(typeof(T)))
            {
                var attribute = Utilities.GetAttribute<Responses.ResponseAttribute>(command);
                if (attribute != null)
                {
                    response += $"{String.Format(attribute.helpMessage, EnumToCommand(command))}\n\n";
                }
            }
            return response;
        }
    }

    public abstract class DiscordIO
    {
        static Regex enumToCommand = new Regex("(\\B[A-Z])");
        public DiscordSocketClient client;
        public DiscordSocketRestClient restClient;

        public async Task SetRest()
        {
            restClient = client.Rest;
            await Task.CompletedTask;
        }
        public virtual async Task Connected() { await Task.CompletedTask; }
        public virtual Task Message(SocketMessage message, string lowercase, bool isAdmin) { return Task.CompletedTask; }
        public virtual Task Command(Enum command, SocketMessage message, string messageStripped, bool admin) { return Task.CompletedTask; }
        public virtual async Task AsyncMessage(SocketMessage message, string lowercase, bool isAdmin) { await Task.CompletedTask; }
        public virtual async Task AsyncCommand(Enum command, SocketMessage message, string messageStripped, bool admin) { await Task.CompletedTask; }
        public virtual async Task Joined(SocketGuildUser guild) { await Task.CompletedTask; }
        public virtual Task Logout() { return Task.CompletedTask; }
        public virtual void FiveMinuteTask(object source, ElapsedEventArgs e) { return; }
        public virtual void MinuteTask(object source, ElapsedEventArgs e) { return; }
        public virtual void ThirtySecondTask(object source, ElapsedEventArgs e) { return; }
        public virtual void TenSecondTask(object source, ElapsedEventArgs e) { return; }
        public virtual async Task OnRemoveReaction(Cacheable<IUserMessage, ulong> cachedMessage, ISocketMessageChannel channel, SocketReaction reaction) { await Task.CompletedTask; }
        public virtual async Task OnReaction(Cacheable<IUserMessage, ulong> cachedMessage, ISocketMessageChannel channel, SocketReaction reaction) { await Task.CompletedTask; }
        public abstract Enum? HelpCommand();
        public virtual string HelpMessage(bool isAdmin) { return ""; }
        public abstract string FeatureName();
        public virtual Color FeatureColor() { return Color.Default; }
        public static string EnumToCommand(Enum val) 
        {
            return enumToCommand.Replace(val.ToString(), " $1").ToLower();
        }
    }
}
