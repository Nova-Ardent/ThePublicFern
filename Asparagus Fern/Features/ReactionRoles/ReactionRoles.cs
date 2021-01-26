using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using System.IO;
using Asparagus_Fern.Tools;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

public partial class Responses
{
    public static string reactionRolesGenerate = "reaction roles generate";
    public static string reactionRolesUpdate = "reaction roles update";
    public static string reactionRolesAssign = "reaction roles assign";
    public static string reactionRolesRoles = "reaction roles roles";
    public static string reactionRolesClear = "reaction roles clear";
    public static string reactionRolesHelp = "reaction roles help";
}

namespace Asparagus_Fern.Features.ReactionRoles
{
    class ReactionRoles : DiscordIO
    {
        [Serializable]
        public struct EmojiAndRole
        {
            public ulong ID { get; set; }
            public string emoji { get; set; }
        }

        private const string reactionRolesPath = "reactionRoles.json";
        private const string reactionMessages = "reactionMessages.json";

        Dictionary<string, List<EmojiAndRole>> rolesForServers = new Dictionary<string, List<EmojiAndRole>>();
        Dictionary<string, ulong> messagesForReactions = new Dictionary<string, ulong>();

        public ReactionRoles()
        {
            if (SaveAndLoad.FileExists(Directory.GetCurrentDirectory(), reactionRolesPath)) SaveAndLoad.LoadFile(out rolesForServers, Directory.GetCurrentDirectory(), reactionRolesPath);
            if (SaveAndLoad.FileExists(Directory.GetCurrentDirectory(), reactionMessages)) SaveAndLoad.LoadFile(out messagesForReactions, Directory.GetCurrentDirectory(), reactionMessages);
        }

        public override async Task AsyncMessage(SocketMessage message, string lowercase, bool isAdmin)
        {
            if (!isAdmin) await base.Message(message, lowercase, isAdmin);

            if (lowercase.StartsWith(Responses.reactionRolesAssign)) await Assign(message, lowercase);
            else if (lowercase.StartsWith(Responses.reactionRolesGenerate)) await Generate(message);
            else if (lowercase.StartsWith(Responses.reactionRolesUpdate)) await Update(message);
            else if (lowercase.StartsWith(Responses.reactionRolesRoles)) await Roles(message);
            else if (lowercase.StartsWith(Responses.reactionRolesClear)) await Clear(message);
            else if (lowercase.StartsWith(Responses.reactionRolesHelp)) await Help(message);

            await base.Message(message, lowercase, isAdmin);
        }

        async Task Assign(SocketMessage message, string lowercase)
        {
            string[] data = lowercase.Split(" ");
            if (data.Length >= 5)
            {
                var emoji = new Emoji(data[3]);
                try
                {
                    await message.AddReactionAsync(emoji);
                    if (message.MentionedRoles.Count() == 1)
                    {
                        ulong roleID = message.MentionedRoles.First().Id;
                        if (message.Channel is SocketGuildChannel)
                        {
                            var guildID = (message.Channel as SocketGuildChannel).Guild.Id;
                            if (!rolesForServers.ContainsKey(guildID.ToString())) rolesForServers[guildID.ToString()] = new List<EmojiAndRole>();

                            rolesForServers[guildID.ToString()].Add(new EmojiAndRole { ID = roleID, emoji = data[3] });

                            await message.Channel.SendMessageAsync($"{roleID} given {emoji}");
                        }
                        else
                        {
                            await message.Channel.SendMessageAsync("action must be performed in a discord server/guild");
                        }
                    }
                    else
                    {
                        await message.Channel.SendMessageAsync("invalid number of mentions");
                    }
                }
                catch (Exception e)
                {
                    await message.Channel.SendMessageAsync("invalid emoji type");
                }
                SaveAndLoad.SaveFile(rolesForServers, Directory.GetCurrentDirectory(), reactionRolesPath);
            }
            else
            {
                await message.Channel.SendMessageAsync($"Something went wrong");
            }
        }

        async Task Generate(SocketMessage message)
        {
            if (message.Channel is SocketGuildChannel)
            {
                var guild = (message.Channel as SocketGuildChannel).Guild;
                var guildID = guild.Id;
                if (rolesForServers.ContainsKey(guildID.ToString()))
                {
                    List<EmbedFieldBuilder> embedFieldBuilders = new List<EmbedFieldBuilder>();
                    foreach (var role in rolesForServers[guildID.ToString()])
                    {
                        var guildRole = guild.GetRole(role.ID);
                        embedFieldBuilders.Add(new EmbedFieldBuilder() { Value = guildRole.Mention, Name = role.emoji, IsInline = true });
                    }

                    var embed = new EmbedBuilder()
                    {
                        Author = new EmbedAuthorBuilder() { Name = "React Roles" },
                        Description = "React with the appropriate Emote to get the role!",
                        Color = Color.Green,
                        Fields = embedFieldBuilders
                    }.Build();

                    var response = await message.Channel.SendMessageAsync(embed: embed);
                    foreach (var role in rolesForServers[guildID.ToString()])
                    {
                        await response.AddReactionAsync(new Emoji(role.emoji));
                    }

                    messagesForReactions[guildID.ToString()] = response.Id;
                    SaveAndLoad.SaveFile(messagesForReactions, Directory.GetCurrentDirectory(), reactionMessages);
                }
            }
        }

        async Task Update(SocketMessage message)
        {
            var guild = (message.Channel as SocketGuildChannel).Guild;
            if (messagesForReactions.ContainsKey(guild.Id.ToString()))
            {
                var messageID = messagesForReactions[guild.Id.ToString()];
                var reactMessage = await message.Channel.GetMessageAsync(messageID);
                if (reactMessage is Discord.Rest.RestUserMessage)
                {
                    var restMessage = reactMessage as Discord.Rest.RestUserMessage;
                    await restMessage.ModifyAsync(m => {
                        List<EmbedFieldBuilder> embedFieldBuilders = new List<EmbedFieldBuilder>();
                        foreach (var role in rolesForServers[guild.Id.ToString()])
                        {
                            var guildRole = guild.GetRole(role.ID);
                            embedFieldBuilders.Add(new EmbedFieldBuilder() { Value = guildRole.Mention, Name = role.emoji, IsInline = true });
                        }

                        var embed = new EmbedBuilder()
                        {
                            Author = new EmbedAuthorBuilder() { Name = "React Roles" },
                            Description = "React with the appropriate Emote to get the role!",
                            Color = Color.Green,
                            Fields = embedFieldBuilders
                        }.Build();

                        m.Embed = embed;
                    }, new RequestOptions() { RetryMode = RetryMode.AlwaysRetry });

                    foreach (var role in rolesForServers[guild.Id.ToString()])
                    {
                        await restMessage.AddReactionAsync(new Emoji(role.emoji));
                    }
                    await message.Channel.SendMessageAsync("done");
                }

            }
        }

        async Task Roles(SocketMessage message)
        {
            if (message.Channel is SocketGuildChannel)
            {
                var guildID = (message.Channel as SocketGuildChannel).Guild.Id;
                if (rolesForServers.ContainsKey(guildID.ToString()))
                {
                    List<EmbedFieldBuilder> embedFieldBuilders = new List<EmbedFieldBuilder>();
                    foreach (var role in rolesForServers[guildID.ToString()])
                    {
                        embedFieldBuilders.Add(new EmbedFieldBuilder() { Name = $"{role.ID}", Value = role.emoji, IsInline = true });
                    }

                    var embed = new EmbedBuilder()
                    {
                        Color = Color.Green,
                        Fields = embedFieldBuilders
                    }.Build();
                    await message.Channel.SendMessageAsync(embed: embed);
                }
            }
        }

        async Task Clear(SocketMessage message)
        {
            if (message.Channel is SocketGuildChannel)
            {
                var guildID = (message.Channel as SocketGuildChannel).Guild.Id;
                rolesForServers[guildID.ToString()] = new List<EmojiAndRole>();
                await message.Channel.SendMessageAsync("done");
            }
        }

        async Task Help(SocketMessage message)
        {
            var embed = new EmbedBuilder()
            {
                Title = "Reaction roles",
                Description = $"assigning a role to an emoji\n`{Responses.reactionRolesAssign} emoji @rolename`\n\n"
                    + $"generating a new react role message\n`{Responses.reactionRolesGenerate}`\n\n"
                    + $"update the current react role message\n`{Responses.reactionRolesGenerate}`\n\n"
                    + $"clear the roles\n`{Responses.reactionRolesClear}`\n\n"
                    + $"get list of the roles\n`{Responses.reactionRolesRoles}`\n\n",
                Color = Color.DarkBlue
            }.Build();
            await message.Channel.SendMessageAsync(embed: embed);
        }

        public override async Task OnReaction(Cacheable<IUserMessage, ulong> cachedMessage, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (reaction.User.IsSpecified && reaction.User.Value.IsBot) return;
            
            if (channel is SocketGuildChannel)
            {
                var guild = await restClient.GetGuildAsync(((SocketGuildChannel)channel).Guild.Id);
                var messageID = cachedMessage.Id;
                var IDString = cachedMessage.Id.ToString();

                if (messagesForReactions.ContainsKey(guild.Id.ToString()) 
                    && rolesForServers.ContainsKey(guild.Id.ToString()) 
                    && messagesForReactions[guild.Id.ToString()] == messageID)
                {
                    var list = rolesForServers[guild.Id.ToString()];
                    var emote = reaction.Emote;

                    foreach (var roles in list)
                    {
                        if (roles.emoji == emote.Name)
                        {
                            var user = await guild.GetUserAsync(reaction.UserId);
                            var role = guild.GetRole(roles.ID);
                            var userRoles = user.RoleIds;
                            if (user != null && !userRoles.Any(x => x == roles.ID))
                            {
                                await user.AddRoleAsync(role);
                            }
                            break;
                        }
                    }
                }
            }

            await base.OnReaction(cachedMessage, channel, reaction);
        }

        public override async Task OnRemoveReaction(Cacheable<IUserMessage, ulong> cachedMessage, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (reaction.User.IsSpecified && reaction.User.Value.IsBot) return;
            if (channel is SocketGuildChannel)
            {
                var guild = await restClient.GetGuildAsync(((SocketGuildChannel)channel).Guild.Id);
                var messageID = cachedMessage.Id;
                var IDString = cachedMessage.Id.ToString();

                if (messagesForReactions.ContainsKey(guild.Id.ToString())
                    && rolesForServers.ContainsKey(guild.Id.ToString())
                    && messagesForReactions[guild.Id.ToString()] == messageID)
                {
                    var list = rolesForServers[guild.Id.ToString()];
                    var emote = reaction.Emote;

                    foreach (var roles in list)
                    {
                        if (roles.emoji == emote.Name)
                        {
                            var user = await guild.GetUserAsync(reaction.UserId);
                            var role = guild.GetRole(roles.ID);
                            var userRoles = user.RoleIds;
                            if (user != null && userRoles.Any(x => x == roles.ID))
                            {
                                await user.RemoveRoleAsync(role);
                            }
                            break;
                        }
                    }
                }
            }

            await base.OnRemoveReaction(cachedMessage, channel, reaction);
        }
    }
}
