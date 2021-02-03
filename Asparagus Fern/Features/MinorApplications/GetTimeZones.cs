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
    public static string GetTimeZones = "get time zones";
    public static string FindTimeZone = "find time zone";
}

namespace Asparagus_Fern.Features.MinorApplications
{
    class GetTimeZones : DiscordIO
    {
#if _WINDOWS
        TimeZoneInfo[] timeZones = TimeZoneInfo.GetSystemTimeZones()
                .OrderBy(x => x.BaseUtcOffset)
                .ToArray();
#else

#endif

        public GetTimeZones()
        {
#if _WINDOWS
            string[] serializedTimezones = timeZones.Select(x => x.ToSerializedString()).ToArray();
            SaveAndLoad.SaveFile(serializedTimezones, Program.DataPath, "timezones.json");
#else

#endif
        }

        public override async Task AsyncMessage(SocketMessage message, string lowercase, bool isAdmin)
        {
            if (lowercase.StartsWith(Responses.GetTimeZones)) await GetTimeZonesList(message);
            if (lowercase.StartsWith(Responses.FindTimeZone)) await FindTimeZones(message, lowercase);
            await base.AsyncMessage(message, lowercase, isAdmin);
        }

        async Task GetTimeZonesList(SocketMessage message)
        {
            DateTime timeNow = DateTime.UtcNow;

            int i = 0;
            var embeds = timeZones
                .OrderBy(x => x.BaseUtcOffset)
                .Select(x =>
                {
                    return new EmbedFieldBuilder()
                    {
                        Name = $"`{i++})` {x.DisplayName}",
                        Value = $"{TimeZoneInfo.ConvertTimeFromUtc(timeNow, x)}",
                        IsInline = true
                    };
                }).ToArray();


            for (int j = 0; j < embeds.Length / 24 + 1; j++)
            {
                var embed = new EmbedBuilder()
                {
                    Title = j == 0 ? "Time Zones" : "",
                    Fields = embeds.Skip(j * 24).Take(24).ToList(),
                    Color = Color.Teal
                }.Build();
                await message.Channel.SendMessageAsync(embed: embed);
            }
        }

        async Task FindTimeZones(SocketMessage message, string lowercase)
        {
            var search = lowercase.Substring(Responses.FindTimeZone.Length).Trim();

            DateTime timeNow = DateTime.UtcNow;

            int i = 0;
            var embeds = timeZones
                .OrderBy(x => x.BaseUtcOffset)
                .Select(x => (first : x, second : i++))
                .Where(x => x.first.DisplayName.ToLower().Contains(search))
                .Select(x =>
                {
                    return new EmbedFieldBuilder()
                    {
                        Name = $"`{x.second})` {x.first.DisplayName}",
                        Value = $"{TimeZoneInfo.ConvertTimeFromUtc(timeNow, x.first)}",
                        IsInline = true
                    };
                }).ToArray();

            for (int j = 0; j < embeds.Length / 24 + 1; j++)
            {
                var embed = new EmbedBuilder()
                {
                    Title = j == 0 ? "Time Zones" : "",
                    Fields = embeds.Skip(j * 24).Take(24).ToList(),
                    Color = Color.Teal
                }.Build();
                await message.Channel.SendMessageAsync(embed: embed);
            }
        }

        public override string HelpMessage(bool isAdmin)
        {
            return $"`{Responses.GetTimeZones}` and `{Responses.FindTimeZone} <your search>` to get a list of timezones, and their current times.";
        }
    }
}
