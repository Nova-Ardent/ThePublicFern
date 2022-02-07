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

namespace Asparagus_Fern.Features.Phasmo
{
    public class PhasmoPhotoRandomizer
    {
        Random random = new Random();

        public static string header = "Photo Randomizer!";
        public static string rules = "To play photo randomizer, you start off with access to all three photo cams. For each unique success photo (worth photo evidence) you take with exception to interactions and foot prints where you get 2, you get 1 additional random item to help you figure out the ghost.\n";
        public static int maxInteractivePhotos = 2;
        public static int maxObjectives = 3;

        public enum CompleteType
        {
            Ghost,
            Possession,
            Bone,
            Objectives,
            InteractivePhoto,
            FingerPrints,
            FootSteps,
            Sink
        }

        public string owner;
        public string avatar;
        public ulong latestMessage;

        public bool ghostPhoto;
        public bool possessionPhoto;
        public bool bonePhoto;
        public int objectives;
        public int interactionPhotos;
        public int fingerPrintsPhotos;
        public int footStepPhotos;
        public bool sinkPhoto;

        public List<PhasmoBot.PhasmoItem> availableItems = new List<PhasmoBot.PhasmoItem>();
        public List<PhasmoBot.PhasmoItem> currentItems = new List<PhasmoBot.PhasmoItem>();

        public PhasmoPhotoRandomizer(string owner, string avatar, PhasmoBot.PhasmoItem[] items)
        {
            this.owner = owner;
            this.avatar = avatar;

            foreach (var item in items)
            {
                availableItems.AddRange(ArrayList.Repeat(item, item.maxCount).Cast<PhasmoBot.PhasmoItem>().ToArray());
            }
        }

        public Embed GetGameHeader()
        {
            return new EmbedBuilder()
            {
                Title = "Fern Photo Randomizer!",
                Description = $"**{owner}**\n {rules}\n\n**your items**\n{GetCurrentItems()}\n\n**Complete:**\n{GetCompleted()}",
                Color = Color.DarkBlue,
                ThumbnailUrl = avatar
            }.Build();
        }

        public void AddItemByName(string name)
        {
            var item = String.Concat(name.Skip(Responses.FernGetPhasItem.Length)).Trim();
            var itemIndex = availableItems.IndexOf(x => x.name.Contains(name));

            AddItem(itemIndex);
        }

        public void AddRandomItem()
        {
            var itemIndex = random.Next(0, availableItems.Count());
            AddItem(itemIndex);
        }

        public void AddItem(int index)
        {
            if (index < 0 || index > availableItems.Count)
            {
                return;
            }

            currentItems.Add(availableItems.ElementAt(index));
            availableItems.RemoveAt(index);
        }

        public string GetCurrentItems()
        {
            var query = currentItems
                .Select(x => x.name)
                .GroupBy(x => x, (y, z) => new { name = y, count = z.Count() });

            string results = "";
            foreach (var result in query)
            {
                results += $"{result.name} x{result.count}\n";
            }

            return results;
        }

        public void Completed(CompleteType completeType)
        {
            switch (completeType)
            {
                case CompleteType.Ghost:
                    if (ghostPhoto) return;    

                    ghostPhoto = true;
                    AddRandomItem();
                    break;
                case CompleteType.Possession:
                    if (possessionPhoto) return;

                    possessionPhoto = true;
                    break;
                case CompleteType.Bone:
                    if (bonePhoto) return;

                    bonePhoto = true;
                    break;
                case CompleteType.Objectives:
                    if (objectives == maxObjectives) return;

                    objectives = Math.Min(objectives + 1, maxObjectives);
                    break;
                case CompleteType.InteractivePhoto:
                    if (interactionPhotos == maxInteractivePhotos) return;

                    interactionPhotos = Math.Min(interactionPhotos + 1, maxInteractivePhotos);
                    break;
                case CompleteType.FingerPrints:
                    if (fingerPrintsPhotos == maxInteractivePhotos) return;

                    fingerPrintsPhotos = Math.Min(fingerPrintsPhotos + 1, maxInteractivePhotos);
                    break;
                case CompleteType.FootSteps:
                    if (footStepPhotos == maxInteractivePhotos) return;

                    footStepPhotos = Math.Max(footStepPhotos + 1, maxInteractivePhotos);
                    break;
                case CompleteType.Sink:
                    if (sinkPhoto) return;

                    sinkPhoto = true;
                    break;
            }

            AddRandomItem();
        }

        public string GetCompleted()
        {
            string results = "";

            results += $"{EmojiList.ghost} ghost photo {(ghostPhoto ? EmojiList.check : EmojiList.cross)}\n";
            results += $"{EmojiList.devil} possession photo {(possessionPhoto ? EmojiList.check : EmojiList.cross)}\n";
            results += $"{EmojiList.bone} bone photo {(bonePhoto ? EmojiList.check : EmojiList.cross)}\n";
            results += $"{EmojiList.objectives} objectives {EmojiList.ints[objectives]}/{EmojiList.ints[maxObjectives]}\n";
            results += $"{EmojiList.hand} fingerprints photo {EmojiList.ints[fingerPrintsPhotos]}/{EmojiList.ints[maxInteractivePhotos]}\n";
            results += $"{EmojiList.foot} footsteps photo {EmojiList.ints[footStepPhotos]}/{EmojiList.ints[maxInteractivePhotos]}\n";
            results += $"{EmojiList.camera} interaction photo {EmojiList.ints[interactionPhotos]}/{EmojiList.ints[maxInteractivePhotos]}\n";
            results += $"{EmojiList.water} sink photo {(sinkPhoto ? EmojiList.check : EmojiList.cross)}\n";

            return results;
        }

        public void UpdateLatestMessage(ulong messageID)
        {
            latestMessage = messageID;
        }
    }
}
