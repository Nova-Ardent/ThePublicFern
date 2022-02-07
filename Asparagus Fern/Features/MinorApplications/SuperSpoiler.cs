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
    public static string FernSuperSpoilerHelp = "help fern super spoiler";
    public static string FernSuperSpoiler = "fern super spoiler ";
    public static string Fern1DaySuperSpoiler = "fern 1 day super spoiler ";
    public static string Fern1HourSuperSpoiler = "fern 1 hour super spoiler ";
    public static string Fern1MinSuperSpoiler = "fern 1 min super spoiler ";
}

namespace Asparagus_Fern.Features.MinorApplications
{
    class SuperSpoiler : DiscordIO
    {
        public enum Duration
        {
            None,
            Day,
            Hour,
            Min,
        }

        public struct MessageData
        {
            public string content;
            public long until;
        }

        Dictionary<ulong, MessageData> messageResponses = new Dictionary<ulong, MessageData>();

        public SuperSpoiler()
        {

        }

        public override async Task AsyncMessage(SocketMessage message, string lowercase, bool isAdmin)
        {
            if (lowercase.StartsWith(Responses.FernSuperSpoilerHelp))
            {
                Help(message);
            }

            int trimLength = 0;
            Duration duration = Duration.None;
            if (lowercase.StartsWith(Responses.FernSuperSpoiler))
            {
                duration = Duration.Day;
                trimLength = Responses.FernSuperSpoiler.Length;
            }
            else if (lowercase.StartsWith(Responses.Fern1DaySuperSpoiler))
            {
                duration = Duration.Day;
                trimLength = Responses.Fern1DaySuperSpoiler.Length;
            }
            else if (lowercase.StartsWith(Responses.Fern1HourSuperSpoiler))
            {
                duration = Duration.Hour;
                trimLength = Responses.Fern1HourSuperSpoiler.Length;
            }
            else if (lowercase.StartsWith(Responses.Fern1MinSuperSpoiler))
            {
                duration = Duration.Min;
                trimLength = Responses.Fern1MinSuperSpoiler.Length;
            }

            if (duration != Duration.None)
            {
                string content = message.Content;
                string user = message.Author.Username;
                string url = message.Author.GetAvatarUrl();
                await message.DeleteAsync();

                long EPOCH_2038_SAFE = 0;
                switch (duration)
                {
                    case Duration.Day: EPOCH_2038_SAFE = (long)(DateTime.UtcNow.AddDays(1) - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds; break;
                    case Duration.Hour: EPOCH_2038_SAFE = (long)(DateTime.UtcNow.AddHours(1) - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds; break;
                    case Duration.Min: EPOCH_2038_SAFE = (long)(DateTime.UtcNow.AddMinutes(1) - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds; break;
                }

                var embed = new EmbedBuilder()
                {
                    Title = "Fern super spoiler",
                    Description =
                        $"super spoiler created by: {user}.\n\npress {EmojiList.boom} to reveal.\nmessage destroyed at: <t:{EPOCH_2038_SAFE}:F>",
                    Color = Color.DarkBlue,
                    ThumbnailUrl = url
                }.Build();


                var response = await message.Channel.SendMessageAsync(embed: embed);
                await response.AddReactionAsync(new Emoji(EmojiList.boom));
                messageResponses[response.Id] = new MessageData()
                {
                    content = content.Substring(trimLength, content.Length - trimLength),
                    until = EPOCH_2038_SAFE
                };
            }

            await Task.CompletedTask;
        }

        public override async Task OnReaction(Cacheable<IUserMessage, ulong> cachedMessage, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (!reaction.User.IsSpecified || reaction.User.Value.IsBot || !messageResponses.ContainsKey(cachedMessage.Id))
            {
                await base.OnReaction(cachedMessage, channel, reaction);
                return;
            }

            if (EmojiList.boom.Equals(reaction.Emote.Name))
            {
                var embed = new EmbedBuilder()
                {
                    Title = "Fern super spoiler.",
                    Description =
                        $"super spoiler:\n\n{messageResponses[cachedMessage.Id].content}",
                    Color = Color.DarkBlue,
                }.Build();
                await reaction.User.Value.SendMessageAsync(embed: embed);
            }
        }

        public override void MinuteTask(object source, ElapsedEventArgs e)
        {
            long EPOCH_2038_SAFE = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;

            List<ulong> removeKeyList = new List<ulong>();
            foreach (var keyValues in messageResponses)
            {
                var timeVal = keyValues.Value.until;
                if (timeVal < EPOCH_2038_SAFE)
                {
                    removeKeyList.Add(keyValues.Key);
                }
            }

            foreach (var removeKeys in removeKeyList)
            {
                messageResponses.Remove(removeKeys);
            }

            base.MinuteTask(source, e);
        }

        public override string HelpMessage(bool isAdmin)
        {
            return $"for a list of super spoiler commands use `{Responses.FernSuperSpoilerHelp}`";
        }

        public void Help(SocketMessage message)
        {
            var embed = new EmbedBuilder()
            {
                Title = "Fern Super Spoiler Help!",
                Description =
                    $"`{Responses.FernSuperSpoiler}` 1 day long super spoiler\n" +
                    $"`{Responses.Fern1DaySuperSpoiler}` 1 dau long super spoiler\n" +
                    $"`{Responses.Fern1HourSuperSpoiler}` 1 hour long super spoiler\n" +
                    $"`{Responses.Fern1MinSuperSpoiler}` 1 min long super spoiler\n",
                //$"{}",
                Color = Color.DarkBlue,
            }.Build();
            message.Channel.SendMessageAsync(embed: embed);
        }
    }
}
