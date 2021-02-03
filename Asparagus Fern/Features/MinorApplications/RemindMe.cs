using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using System.IO;
using Asparagus_Fern.Tools;
using System.Collections.Generic;
using System.Timers;
using System.Linq;

public partial class Responses
{
    public static string RemindMeHelp = "remind me help";
    public static string RemindMeSetup = "remind me setup";
    public static string RemindMeSetupTimeZoneGroup = "remind me setup time zone group";
    public static string RemindMeSetupTimeZone = "remind me setup time zone";
    public static string RemindMeAt = "remind me at";
}

namespace Asparagus_Fern.Features.MinorApplications
{
    public class RemindMe : DiscordIO
    {
        [System.Serializable]
        public class User
        {
            public string username { get; set; }
            public ulong userID { get; set; }
            public ushort discrim { get; set; }
            public int timezoneGroup { get; set; }
            public int timezone { get; set; }

        }

        IGrouping<TimeSpan, TimeZoneInfo>[] timeZoneGroups = TimeZoneInfo.GetSystemTimeZones()
                .OrderBy(x => x.BaseUtcOffset)
                .GroupBy(x => x.BaseUtcOffset)
                .ToArray();

        public static string UserPath = "RemindMeUsers";

        [System.Serializable]
        public class Reminders
        {
            
        }

        public RemindMe()
        {
        }

        public override Task Message(SocketMessage message, string lowercase, bool isAdmin)
        {
            if (lowercase.StartsWith(Responses.RemindMeSetupTimeZoneGroup)) RemindMeSetupTimeZoneGroup(message, lowercase);
            if (lowercase.StartsWith(Responses.RemindMeSetupTimeZone)) RemindMeSetupTimeZone(message, lowercase);
            else if (lowercase.StartsWith(Responses.RemindMeSetup)) RemindMeSetup(message, lowercase);

            return base.Message(message, lowercase, isAdmin);
        }

        void RemindMeSetup(SocketMessage message, string lowercase)
        {
            if (SaveAndLoad.FileExists(UserPath, message.Author.Id.ToString() + ".json"))
            {
                message.Author.SendFileAsync("You already have a remind me user.");
                return;
            }

            User user = new User() {
                username = message.Author.Username,
                discrim = message.Author.DiscriminatorValue,
                userID = message.Author.Id,
            };

            SaveAndLoad.SaveFile(user, UserPath, message.Author.Id.ToString() + ".json");
        }

        void RemindMeSetupTimeZoneGroup(SocketMessage message, string lowercase)
        {
            var firstInt = new string
                (lowercase
                .SkipWhile(x => !char.IsDigit(x))
                .TakeWhile(x => char.IsDigit(x))
                .ToArray()
                );

            int parse = 0;
            if (String.IsNullOrEmpty(firstInt))
            {
                message.Channel.SendMessageAsync($"use the command `{Responses.GetTimeZones}` to get a list of time zones. Next to each time zone will be a number."
                    + " Just repeat this command followed by the number associated to your time zone.\n");
                return;
            }

            parse = int.Parse(firstInt);
            if (parse >= 0 && parse < timeZoneGroups.Length)
            {
                var timezone = timeZoneGroups[parse];
                message.Channel.SendMessageAsync($"you selected the timezone; (GMT{(timezone.First().BaseUtcOffset > TimeSpan.Zero ? "+" : "")}{timezone.First().BaseUtcOffset})");
            }
        }

        void RemindMeSetupTimeZone(SocketMessage message, string lowercase)
        {
            
        }

        void RemindMeHelp()
        {
            
        }

        public override string HelpMessage(bool isAdmin)
        {
            return $"`{Responses.RemindMeHelp}` to learn more about how to setup reminders with the bot.";
        }
    }
}
