using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Asparagus_Fern.Tools;


namespace Asparagus_Fern.Features.MinorApplications
{
    class PercentResponse : DiscordIO
    {
        public PercentResponse()
        {
            
        }

        public override async Task AsyncMessage(SocketMessage message, string lowercase, bool isAdmin)
        {
            if (message.Author.IsBot) return;
            if (message.Content.Contains("150"))
            {
                await message.Channel.SendMessageAsync("<a:partyblob:751420504640061481> 150 percent intensifies <a:partyblob:751420504640061481>");
            }

            await base.AsyncMessage(message, lowercase, isAdmin);
        }
    }
}
