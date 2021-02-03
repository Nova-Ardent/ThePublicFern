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

namespace Asparagus_Fern.Features.MinorApplications
{
    class MessageRecord : DiscordIO
    {
        ISocketMessageChannel lastChannel;

        public MessageRecord()
        {
            
        }

        public override Task Message(SocketMessage message, string lowercase, bool isAdmin)
        {
            if (message.Channel != lastChannel)
            {
                lastChannel = message.Channel;
                if (lastChannel is SocketGuildChannel)
                {
                    Console.WriteLine($"------- {(lastChannel as SocketGuildChannel).Guild} #{lastChannel.Name} -------");
                }
                else
                {
                    Console.WriteLine($"------- {lastChannel.Name} ------");
                }
            }

            Console.WriteLine($"{message.Author.Username}#{message.Author.Discriminator}:-- {message.Content}\n");
            return base.Message(message, lowercase, isAdmin);
        }
    }
}
