using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using System.IO;
using System.Text.Json;
using System.Linq;

namespace Asparagus_Fern.Features
{
    class GuessThatRank
    {
        [System.Serializable]
        public class Channels
        {
            public ulong postGuild { get; set; }
            public ulong postChannel { get; set; }
        }

        private const string channelsFile = "channels.json";
        public Channels channels = new Channels();
        Dictionary<ulong, SocketMessage> pendingReplays = new Dictionary<ulong, SocketMessage>();

        private DiscordSocketClient client;

        public GuessThatRank(DiscordSocketClient client)
        {
            this.client = client;
            client.MessageReceived += Message;

            if (SaveAndLoad.FileExists(Directory.GetCurrentDirectory(), channelsFile))
            {
                SaveAndLoad.LoadFile(out channels, Directory.GetCurrentDirectory(), channelsFile);
            }
            else
            {
                channels = new Channels();
                SaveAndLoad.SaveFile(channels, Directory.GetCurrentDirectory(), channelsFile);
            }
        }

        private async Task Message(SocketMessage message)
        {
            if (message.Author.IsBot)
            {
                return;
            }

            bool isAdmin = false;
            if (message.Channel is SocketGuildChannel)
            {
                isAdmin = (message.Channel as SocketGuildChannel).Guild.GetUser(message.Author.Id).GuildPermissions.Administrator;
            }

            if (message.Content.StartsWith("!guess help"))
            {
                var embed = new EmbedBuilder() { Title = "Guess that rank help message!", Description = GetHelpMessage(isAdmin), Color = Color.Green }.Build();
                await message.Author.SendMessageAsync(embed : embed);
                return;
            }

            if (isAdmin && message.Content.StartsWith("!guess submission"))
            {
                if (message.Channel is SocketGuildChannel)
                {
                    channels.postGuild = (message.Channel as SocketGuildChannel).Guild.Id;
                    channels.postChannel = message.Channel.Id;
                    SaveAndLoad.SaveFile(channels, Directory.GetCurrentDirectory(), channelsFile);

                    await message.Channel.SendMessageAsync("This channel as been set as the \"guess that rank\" channel. All guess that rank submissions will be posted here.\n");
                }
                else
                {
                    await message.Channel.SendMessageAsync("something went wrong, make sure the channel is accessible by me, and is a discord server not a DM\n");
                }
                return;
            }

            if (message.Content.StartsWith("!guess rank"))
            {
                if (!pendingReplays.ContainsKey(message.Author.Id))
                {
                    message.Channel.SendMessageAsync("you haven't submitted a replay to apply a rank to.");
                    await message.DeleteAsync();
                    return;
                }

                string[] split = message.Content.Split();
                if (split.Count() > 2)
                {
                    var replayMessage = pendingReplays[message.Author.Id];
                    var channel = client.GetGuild(channels.postGuild).GetChannel(channels.postChannel) as ISocketMessageChannel;

                    string padding = new string(' ', 50 - message.Content.Substring(11).Length);
                    var embed = new EmbedBuilder() { Title = $"||`{message.Content.Substring(11)}{padding}`||", Description = replayMessage.Content.Substring(7) + "\n\n" + replayMessage.Attachments.First().Url, Color = Color.Green }.Build();
                    await channel.SendMessageAsync(embed : embed);
                    return;
                    
                }
                else
                {
                    message.Channel.SendMessageAsync("please include a rank and a division.");
                    await message.DeleteAsync();
                    return;
                }
            }
            else if (message.Content.StartsWith("!guess "))
            {
                if (message.Channel is SocketGuildChannel)
                {
                    message.Channel.SendMessageAsync("Please make sure submissions are sent to me in private, so the answer isn't given away by accident.");
                    await message.DeleteAsync();
                    return;
                }
                else
                {
                    if (message.Attachments.Count() == 0)
                    {
                        message.Channel.SendMessageAsync("please make sure you have an attached replay to the submission.");
                        await message.DeleteAsync();
                        return;
                    }
                    else
                    {
                        pendingReplays[message.Author.Id] = message;
                        await message.Author.SendMessageAsync("What rank is your replay? To tell me the rank type \"!guess rank\" followed by your rank. Example:\n\n\n!guess rank bronze 3");
                        return;
                    }
                }
            }
        }

        private string GetHelpMessage(bool admin)
        {
            string additional = "";
            if (admin)
            {
                additional = "To set a channel as a guess that rank replays channel, type !guess submission\nexample:\n\n\n!guess submission\n\n";
            }

            return additional + "To set up a guess that rank, send me a message in private that is prefaced with \"guess\" that contains an attachment of your replay. Please make sure your replay is attached, and is named after the person you would like to follow, as this will make it easier."
            + " An Example message would be as follows.\n\n\n"
            + "!guess Hey, here is my replay for guessing. The player I want you to follow is Jimmy."
            + "\n\nYour replays can be found at: C:\\Users\\USERNAME\\Documents\\My Games\\Rocket League\\TAGame or downloaded from https://ballchasing.com/"; 
        }
    }
}
