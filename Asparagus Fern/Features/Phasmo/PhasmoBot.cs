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
    public static string HelpFernPhasmo = "help fern phasmo";
    public static string FernGetPhasItems = "fern get phasmo items";
    public static string FernGetPhasItem = "fern get phasmo item ";
    public static string FernGetPhasRandomItem = "fern get phasmo random item";
    public static string FernStartPhotoRandomizer = "fern start phasmo photo randomizer";
    public static string FernSetPhasmoGame = "fern set phasmo game ";
    public static string FernMyPhasmoGame = "fern my phasmo game";
    public static string FernRemovePhasmoGame = "fern remove phasmo game";
    public static string FernPhasmoGames = "fern phasmo games";
}

namespace Asparagus_Fern.Features.Phasmo
{
    public class PhasmoBot : DiscordIO
    {
        Random random = new Random();

        string folderPath = "PhasmoData";
        string equipmentFile = "equipment.json";

        [System.Serializable]
        public class PhasmoItem
        {
            public string name { get; set; }
            public int maxCount { get; set; }
            public int cost { get; set; }
            public string description { get; set; }
            public string url { get; set; }
            public string tips { get; set; }
        }

        PhasmoItem[] equipmentItems = new PhasmoItem[] { };
        Dictionary<ulong, PhasmoPhotoRandomizer> activeRandomizerGames = new Dictionary<ulong, PhasmoPhotoRandomizer>();
        Dictionary<ulong, string> activeCodes = new Dictionary<ulong, string>();

        public PhasmoBot()
        {
            if (SaveAndLoad.FileExists(folderPath, equipmentFile))
            {
                SaveAndLoad.LoadFile(out equipmentItems, folderPath, equipmentFile);
            }
        }

        public override async Task AsyncMessage(SocketMessage message, string lowercase, bool isAdmin)
        {
            if (lowercase.StartsWith(Responses.HelpFernPhasmo))
            {
                Help(message);
            }
            else if (lowercase.StartsWith(Responses.FernGetPhasItems))
            {
                var embed = new EmbedBuilder()
                {
                    Title = "Fern Phasmo Items!",
                    Description =
                        EquipmentList(),
                    Color = Color.DarkBlue,
                    ThumbnailUrl = "https://cdn.akamai.steamstatic.com/steam/apps/739630/header.jpg?t=1638041534"
                }.Build();
                await message.Channel.SendMessageAsync(embed: embed);
            }
            else if (lowercase.StartsWith(Responses.FernGetPhasItem))
            {
                if (lowercase.Length > Responses.FernGetPhasItem.Length)
                {
                    var item = String.Concat(lowercase.Skip(Responses.FernGetPhasItem.Length)).Trim();
                    var itemIndex = equipmentItems.IndexOf(x => x.name.Contains(item));

                    if (itemIndex != -1)
                    {
                        var embed = new EmbedBuilder()
                        {
                            Title = "Fern Phasmo Item!",
                            Description =
                                EquipmentItem(itemIndex),
                            Color = Color.DarkBlue,
                            ThumbnailUrl = equipmentItems[itemIndex].url
                        }.Build();
                        await message.Channel.SendMessageAsync(embed: embed);
                    }
                }
            }
            else if (lowercase.StartsWith(Responses.FernGetPhasRandomItem))
            {
                var itemIndex = random.Next(0, equipmentItems.Length);

                if (itemIndex != -1)
                {
                    var embed = new EmbedBuilder()
                    {
                        Title = "Fern Phasmo Item!",
                        Description =
                            EquipmentItem(itemIndex),
                        Color = Color.DarkBlue,
                        ThumbnailUrl = equipmentItems[itemIndex].url
                    }.Build();
                    await message.Channel.SendMessageAsync(embed: embed);
                }
            }
            else if (lowercase.StartsWith(Responses.FernStartPhotoRandomizer))
            {
                activeRandomizerGames[message.Author.Id] = new PhasmoPhotoRandomizer(message.Author.Username, message.Author.GetAvatarUrl(), equipmentItems);
                activeRandomizerGames[message.Author.Id].AddItemByName("photo camera");
                activeRandomizerGames[message.Author.Id].AddItemByName("photo camera");
                activeRandomizerGames[message.Author.Id].AddItemByName("photo camera");
                activeRandomizerGames[message.Author.Id].AddItemByName("flashlight");
                activeRandomizerGames[message.Author.Id].AddItemByName("flashlight");
                activeRandomizerGames[message.Author.Id].AddItemByName("flashlight");
                activeRandomizerGames[message.Author.Id].AddItemByName("flashlight");
                activeRandomizerGames[message.Author.Id].AddItemByName("lighter");
                await CreateRandomizerMessage(message);
            }
            else if (lowercase.StartsWith(Responses.FernSetPhasmoGame))
            {
                var gameCode = String.Concat(lowercase.Skip(Responses.FernSetPhasmoGame.Length)).Trim();
                activeCodes[message.Author.Id] = message.Author.Username + "'s game: " + gameCode;
                await message.Channel.SendMessageAsync($"done! {gameCode}");
            }
            else if (lowercase.StartsWith(Responses.FernMyPhasmoGame))
            {
                await message.Channel.SendMessageAsync(activeCodes[message.Author.Id]);
            }
            else if (lowercase.StartsWith(Responses.FernPhasmoGames))
            {
                await message.Channel.SendMessageAsync(String.Concat(activeCodes.Select(x => $"{x.Value}\n")));
            }
            else if (lowercase.StartsWith(Responses.FernRemovePhasmoGame))
            {
                if (activeCodes.ContainsKey(message.Author.Id))
                {
                    activeCodes.Remove(message.Author.Id);
                    await message.Channel.SendMessageAsync($"done!");
                }
            }

            await Task.CompletedTask;
        }

        private string EquipmentList()
        {
            string items = "";
            foreach (var item in equipmentItems)
            {
                items += $"**{item.name}** `(max x{item.maxCount}) ${item.cost}`\n";
            }

            return items.Substring(0, items.Length - 1);
        }

        private string EquipmentItem(int item)
        {
            return $"**{equipmentItems[item].name}**    (max: x{equipmentItems[item].maxCount}) ${equipmentItems[item].cost}\n\n{equipmentItems[item].description}\n\n{equipmentItems[item].tips}";
        }

        public async Task CreateRandomizerMessage(SocketMessage message)
        {
            var m = await message.Channel.SendMessageAsync(embed: activeRandomizerGames[message.Author.Id].GetGameHeader());
            await m.AddReactionAsync(new Emoji(EmojiList.ghost));
            await m.AddReactionAsync(new Emoji(EmojiList.devil));
            await m.AddReactionAsync(new Emoji(EmojiList.bone));
            await m.AddReactionAsync(new Emoji(EmojiList.objectives));
            await m.AddReactionAsync(new Emoji(EmojiList.hand));
            await m.AddReactionAsync(new Emoji(EmojiList.foot));
            await m.AddReactionAsync(new Emoji(EmojiList.camera));
            await m.AddReactionAsync(new Emoji(EmojiList.water));

            activeRandomizerGames[message.Author.Id].UpdateLatestMessage(m.Id);
        }

        public async Task CreateRandomizerMessage(SocketReaction reaction)
        {
            var m = await reaction.Channel.SendMessageAsync(embed: activeRandomizerGames[reaction.User.Value.Id].GetGameHeader());
            await m.AddReactionAsync(new Emoji(EmojiList.ghost));
            await m.AddReactionAsync(new Emoji(EmojiList.devil));
            await m.AddReactionAsync(new Emoji(EmojiList.bone));
            await m.AddReactionAsync(new Emoji(EmojiList.objectives));
            await m.AddReactionAsync(new Emoji(EmojiList.hand));
            await m.AddReactionAsync(new Emoji(EmojiList.foot));
            await m.AddReactionAsync(new Emoji(EmojiList.camera));
            await m.AddReactionAsync(new Emoji(EmojiList.water));

            activeRandomizerGames[reaction.User.Value.Id].UpdateLatestMessage(m.Id);
        }

        public override async Task OnReaction(Cacheable<IUserMessage, ulong> cachedMessage, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (!reaction.User.IsSpecified || reaction.User.Value.IsBot || !activeRandomizerGames.ContainsKey(reaction.User.Value.Id) || activeRandomizerGames[reaction.User.Value.Id].latestMessage != cachedMessage.Id)
            {
                await base.OnReaction(cachedMessage, channel, reaction);
                return;
            }

            if (EmojiList.ghost.Equals(reaction.Emote.Name))
            {
                activeRandomizerGames[reaction.User.Value.Id].Completed(PhasmoPhotoRandomizer.CompleteType.Ghost);
            }
            if (EmojiList.devil.Equals(reaction.Emote.Name)) activeRandomizerGames[reaction.User.Value.Id].Completed(PhasmoPhotoRandomizer.CompleteType.Possession);
            if (EmojiList.bone.Equals(reaction.Emote.Name)) activeRandomizerGames[reaction.User.Value.Id].Completed(PhasmoPhotoRandomizer.CompleteType.Bone);
            if (EmojiList.objectives.Equals(reaction.Emote.Name)) activeRandomizerGames[reaction.User.Value.Id].Completed(PhasmoPhotoRandomizer.CompleteType.Objectives);
            if (EmojiList.hand.Equals(reaction.Emote.Name)) activeRandomizerGames[reaction.User.Value.Id].Completed(PhasmoPhotoRandomizer.CompleteType.FingerPrints);
            if (EmojiList.foot.Equals(reaction.Emote.Name)) activeRandomizerGames[reaction.User.Value.Id].Completed(PhasmoPhotoRandomizer.CompleteType.FootSteps);
            if (EmojiList.camera.Equals(reaction.Emote.Name)) activeRandomizerGames[reaction.User.Value.Id].Completed(PhasmoPhotoRandomizer.CompleteType.InteractivePhoto);
            if (EmojiList.water.Equals(reaction.Emote.Name)) activeRandomizerGames[reaction.User.Value.Id].Completed(PhasmoPhotoRandomizer.CompleteType.Sink);
            await CreateRandomizerMessage(reaction);
        }

        public override string HelpMessage(bool isAdmin)
        {
            return $"for a list of phasmophobia commands use `{Responses.HelpFernPhasmo}`";
        }

        public void Help(SocketMessage message)
        {
            var embed = new EmbedBuilder()
            {
                Title = "Fern Phasmo Help!",
                Description =
                    $"`{Responses.FernGetPhasItems}` to get a list of equipment items\n"+
                    $"`{Responses.FernGetPhasItem} <item>` to get a specific item\n" +
                    $"`{Responses.FernGetPhasRandomItem}` to get a random item\n" +
                    $"`{Responses.FernStartPhotoRandomizer}` to start a game of photo randomizer.\n" + 
                    $"`{Responses.FernSetPhasmoGame}` followed by your game information will save your game information.\n" + 
                    $"`{Responses.FernMyPhasmoGame}` will get your current game information.\n" + 
                    $"`{Responses.FernRemovePhasmoGame}` will remove your phasmo game information.\n" + 
                    $"`{Responses.FernPhasmoGames}` will get all the latest codes.\n",
                    //$"{}",
                Color = Color.DarkBlue,
                ThumbnailUrl = "https://cdn.akamai.steamstatic.com/steam/apps/739630/header.jpg?t=1638041534"
            }.Build();
            message.Channel.SendMessageAsync(embed: embed);
        }
    }
}
