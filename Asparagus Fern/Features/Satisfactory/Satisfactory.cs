using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using System.IO;
using Asparagus_Fern.Tools;
using System.Collections.Generic;
using System.Timers;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections;

public partial class Responses
{
    public static string HelpFernSatisfactory = "help fern satisfactory";
    public static string FernGetSatisfactoryWorld = "fern get satisfactory world";
}

namespace Asparagus_Fern.Features.Satisfactory
{
    class Satisfactory : DiscordIO
    {
        //public static string WorldPath = "~/.config/Epic/FactoryGame/Saved/SaveGames/server/";

        public Satisfactory()
        {

        }

        public override async Task AsyncMessage(SocketMessage message, string lowercase, bool isAdmin)
        {
            if (lowercase.StartsWith(Responses.HelpFernSatisfactory))
            {
                Help(message);
            }
            else if (lowercase.StartsWith(Responses.FernGetSatisfactoryWorld))
            {
                await GetWorld(message);
            }

            await Task.CompletedTask;
        }

        public async Task GetWorld(SocketMessage message)
        {
            var file = Directory.GetFiles(WorldPath).OrderBy(f => f).First();
            await message.Channel.SendFileAsync(file, "here is the latest world");
        }

        public override string HelpMessage(bool isAdmin)
        {
            return $"for a list of satisfactory commands use `{Responses.HelpFernSatisfactory}`";
        }

        public void Help(SocketMessage message)
        {
            var embed = new EmbedBuilder()
            {
                Title = "Fern Phasmo Help!",
                Description =
                    $"`{Responses.FernGetSatisfactoryWorld}` to get the latest world\n",
                Color = Color.DarkBlue,
                ThumbnailUrl = "https://cdn.akamai.steamstatic.com/steam/apps/739630/header.jpg?t=1638041534"
            }.Build();
            message.Channel.SendMessageAsync(embed: embed);
        }
    }
}
