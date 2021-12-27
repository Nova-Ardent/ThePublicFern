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

public partial class Responses
{
    public static string FernRememberThat = "fern remember that";
    public static string FernRemember = "fern remember";
    public static string FernWhatDoYouRemember = "fern what do you remember";
    public static string FernForget = "fern forget";
    public static string HelpFernRemember = "help fern remember";
}

namespace Asparagus_Fern.Features.MinorApplications
{
    class FernRemember : DiscordIO
    {
        Dictionary<string, Dictionary<string, string>> remembered = new Dictionary<string, Dictionary<string, string>>();
        Regex rgx = new Regex("[^a-zA-Z0-9 -]");

        public FernRemember()
        {
            if (SaveAndLoad.FileExists("Remember", "Remembered.json"))
            {
                SaveAndLoad.LoadFile(out remembered, "Remember", "Remembered.json");
            }
        }

        public override void FiveMinuteTask(object source, ElapsedEventArgs e)
        {
            SaveAndLoad.SaveFile(remembered, "Remember", "Remembered.json");
            base.FiveMinuteTask(source, e);
        }

        public override Task Message(SocketMessage message, string lowercase, bool isAdmin)
        {
            lowercase = rgx.Replace(lowercase, "");
            if ((message.Channel is SocketGuildChannel) && !remembered.ContainsKey((message.Channel as SocketGuildChannel).Guild.Id.ToString()))
            {
                remembered[(message.Channel as SocketGuildChannel).Guild.Id.ToString()] = new Dictionary<string, string>();
            }

            if (lowercase.StartsWith(Responses.FernRememberThat)) RememberThat(message, lowercase);
            else if (lowercase.StartsWith(Responses.FernRemember)) Remember(message, lowercase);
            else if (lowercase.StartsWith(Responses.FernWhatDoYouRemember)) WhatDoYouRemember(message, lowercase);
            else if (lowercase.StartsWith(Responses.FernForget)) Forget(message, lowercase);
            else if (lowercase.StartsWith(Responses.HelpFernRemember)) Help(message);
            return base.Message(message, lowercase, isAdmin);
        }

        public void RememberThat(SocketMessage message, string lowercase)
        {
            if (!(message.Channel is SocketGuildChannel))
            {
                return;
            }

            string[] split = lowercase.Split();
            if (split.Length > 4)
            {
                string rememberKey = split[3];
                string rememberValue = split
                    .Skip(4)
                    .Aggregate((x, y) => $"{x} {y}");

                if (split[3].Length > 100)
                {
                    message.Channel.SendMessageAsync("please choose a smaller word to remember.");
                    return;
                }

                var embed = new EmbedBuilder()
                {
                    Title = $"Ok I'll remember that {rememberKey}",
                    Description = rememberValue,
                    Color = Color.Purple
                }.Build();
                message.Channel.SendMessageAsync(embed: embed);

                remembered[(message.Channel as SocketGuildChannel).Guild.Id.ToString()][rememberKey] = rememberValue;
            }
        }

        public void Forget(SocketMessage message, string lowercase)
        {
            if (!(message.Channel is SocketGuildChannel))
            {
                return;
            }

            string[] split = lowercase.Split();
            if (split.Length > 2)
            {
                if (remembered[(message.Channel as SocketGuildChannel).Guild.Id.ToString()].ContainsKey(split[2]))
                {
                    remembered[(message.Channel as SocketGuildChannel).Guild.Id.ToString()].Remove(split[2]);
                    message.Channel.SendMessageAsync("done!");
                }
                else 
                {
                    message.Channel.SendMessageAsync("I don't remember that.");
                }
            }
        }

        public void Remember(SocketMessage message, string lowercase)
        {
            if (!(message.Channel is SocketGuildChannel))
            {
                return;
            }

            string[] split = lowercase.Split();
            if (split.Length > 2)
            {
                if (remembered[(message.Channel as SocketGuildChannel).Guild.Id.ToString()].ContainsKey(split[2]))
                {
                    string memory = remembered[(message.Channel as SocketGuildChannel).Guild.Id.ToString()][split[2]];
                    var embed = new EmbedBuilder()
                    {
                        Title = $"Yes!",
                        Description = $"I remember that {split[2]} {memory}",
                        Color = Color.Purple
                    }.Build();
                    message.Channel.SendMessageAsync(embed: embed);
                }
                else
                {
                    var embed = new EmbedBuilder()
                    {
                        Title = $"Sorry",
                        Description = $"I don't remember that.",
                        Color = Color.Teal
                    }.Build();
                    message.Channel.SendMessageAsync(embed: embed);
                }
            }
        }

        public void WhatDoYouRemember(SocketMessage message, string lowercase)
        {
            if (!(message.Channel is SocketGuildChannel))
            {
                return;
            }

            string keys = "```";
            foreach (var key in remembered[(message.Channel as SocketGuildChannel).Guild.Id.ToString()].Keys)
            {
                keys += $"__{key}__  ";
                if (keys.Length > 1000)
                {
                    keys += "```";
                    message.Channel.SendMessageAsync(keys);
                    keys = "";
                }
            }

            keys += "```";
            message.Channel.SendMessageAsync(keys);
        }

        public void Help(SocketMessage message)
        {
            var embed = new EmbedBuilder()
            {
                Title = "Fern Remember Help!",
                Description = 
                    $"`{Responses.FernRememberThat} <term> <phrase>` to tell the fern to remember something.\n" +
                    $"`{Responses.FernRemember} <term>` to get the fern to recall something.\n" +
                    $"`{Responses.FernForget} <term>` to get the fern to forget something.\n" +
                    $"`{Responses.FernWhatDoYouRemember}` to ask the fern what he remembers.\n",
                Color = Color.Purple
            }.Build();
            message.Channel.SendMessageAsync(embed: embed);
        }

        public override string HelpMessage(bool isAdmin)
        {
            return $"`{Responses.HelpFernRemember}` for help on the fern remember feature.";
        }
    }
}
