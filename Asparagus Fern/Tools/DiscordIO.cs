using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using System.IO;
using System.Collections.Generic;
using System.Timers;

namespace Asparagus_Fern.Tools
{
    public class DiscordIO
    {
        public DiscordSocketClient client;
        public DiscordSocketRestClient restClient;

        public async Task SetRest()
        {
            restClient = client.Rest;
            await Task.CompletedTask;
        }
        public virtual async Task Connected() { await Task.CompletedTask; }
        public virtual Task Message(SocketMessage message, string lowercase, bool isAdmin) { return Task.CompletedTask; }
        public virtual async Task AsyncMessage(SocketMessage message, string lowercase, bool isAdmin) { await Task.CompletedTask; }
        public virtual Task Logout() { return Task.CompletedTask; }
        public virtual void FiveMinuteTask(object source, ElapsedEventArgs e) { return; }
        public virtual void MinuteTask(object source, ElapsedEventArgs e) { return; }
        public virtual async Task OnRemoveReaction(Cacheable<IUserMessage, ulong> cachedMessage, ISocketMessageChannel channel, SocketReaction reaction) { await Task.CompletedTask; }
        public virtual async Task OnReaction(Cacheable<IUserMessage, ulong> cachedMessage, ISocketMessageChannel channel, SocketReaction reaction) { await Task.CompletedTask; }
    }
}
