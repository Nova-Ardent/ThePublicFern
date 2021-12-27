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
using System.Text;

public partial class Responses
{
    public static string helpJoinedServer = "help joined server";
    public static string joinedServerMessage = "joined server message ";
    public static string joinedServerCurrentMessage = "joined server current message";
}

namespace Asparagus_Fern.Features.MinorApplications
{
    public class JoinedServer : DiscordIO
    {
        Dictionary<string, string> serverMessage = new Dictionary<string, string>();

        public JoinedServer()
        {
            if (SaveAndLoad.FileExists("joinmessage", "joinedmesssages.json"))
            {
                SaveAndLoad.LoadFile(out serverMessage, "joinmessage", "joinedmesssages.json");
            }
        }

        public override async Task AsyncMessage(SocketMessage message, string lowercase, bool isAdmin)
        {
            if (!isAdmin)
            {
                return;
            }

            if (lowercase.Equals(Responses.helpJoinedServer)) await Help(message);
            if (lowercase.StartsWith(Responses.joinedServerMessage))
            {
                if (message.Channel is SocketGuildChannel socketMessage)
                {
                    var skip = message.Content.Skip(Responses.joinedServerMessage.Length);
                    var sb = new StringBuilder();
                    foreach (var c in skip)
                    {
                        sb.Append(c.ToString());
                    }

                    serverMessage[socketMessage.Guild.Id.ToString()] = sb.ToString();
                    await message.Channel.SendMessageAsync($"Your new server responsse is `{serverMessage[socketMessage.Guild.Id.ToString()]}`");
                    SaveAndLoad.SaveFile(serverMessage, "joinmessage", "joinedmesssages.json");
                }
            }
            else if (lowercase.StartsWith(Responses.joinedServerCurrentMessage))
            {
                if (message.Channel is SocketGuildChannel socketMessage && serverMessage.ContainsKey(socketMessage.Guild.Id.ToString()))
                {
                    await message.Channel.SendMessageAsync($"Your current server responsse is `{serverMessage[socketMessage.Guild.Id.ToString()]}`");
                }
            }
        }

        public override async Task Joined(SocketGuildUser guild)
        {
            if (serverMessage.ContainsKey(guild.Guild.Id.ToString()))
            {
                Console.WriteLine(serverMessage[guild.Guild.Id.ToString()]);
                await guild.SendMessageAsync(serverMessage[guild.Guild.Id.ToString()]);
            }
        }

        async Task Help(SocketMessage message)
        {
            var embed = new EmbedBuilder()
            {
                Title = "joined guild",
                Description = $"set a join messsage with the command `{Responses.joinedServerMessage}`" +
                    $"\nto get your current server message use `{Responses.joinedServerCurrentMessage}`",
                Color = Color.DarkBlue
            }.Build();
            await message.Channel.SendMessageAsync(embed: embed);
        }
    }
}
