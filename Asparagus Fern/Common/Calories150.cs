using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using static Responses;
using Asparagus_Fern.Tools;

public partial class Responses
{
    public enum Calories
    {
        [Response("For more information on what the calory game is use `{0}`")] FernWhatIsTheCaloriesGame,
        [Response("For information on how to play the calorie game use `{0}`")] FernHowDoIPlayCaloriesGame,
        [Response("For the latest build type `{0}`")] FernLatestCaloriesGame,
        [Response("To post an idea for the game use `{0}`")] FernIHaveAnIdea,
        MyIdeasCalled,
        MyIdeaIs,
        FernPostIdea,
        [Response("To search for existing ideas use `{0}` followed by the name")] FernSearchCaloriesGameIdea,
    }
}

namespace Asparagus_Fern.Common
{
    class Calories150 : DiscordIO<Calories>
    {
        [System.Serializable]
        public class Idea
        {
            public string userName { get; set; }
            public string name { get; set; }
            public string content { get; set; }
        }

        public static string WhatIsCaloriesGame = 
            @"The Calories game is a survival game where members of the 150 discord can offer ideas that will be vetted, and voted upon to be added to the game.";

        public static string HowDoIPlayCalorieGame =
            $"Build for the calorie game will be posted here using the command `{EnumToCommand(Calories.FernLatestCaloriesGame)}`, all that is needed is to download and run them.";

        public static string LatestBuildInformation =
            $"The first build for the game has not been created but will be out soon.";

        public static string IdeaNameRecommendation =
            $"Followed by the name of your idea. I recommend the name of your idea be concise, descriptive and easy to search for. Offensive material may be immediately vetted.";

        public static string GiveUsAnIdeaName =
            $"To tell us your idea, first we must know the name of your idea. Use the command `{EnumToCommand(Calories.MyIdeasCalled)}` {IdeaNameRecommendation}";

        public static string YourIdeaIsEmpty =
            $"Your idea is empty. {GiveUsAnIdeaName}";

        public static string GiveUsAnIdeaDescription =
            $"To tell us your idea desciption use `{EnumToCommand(Calories.MyIdeaIs)}`";

        public static string YourIdeaDescriptionIsEmpty =
            $"Your idea description is empty.";

        public static string ToPostYourIdeaUse = 
            $"Use `{EnumToCommand(Calories.FernPostIdea)}` to post your idea. you can edit it by reusing `{EnumToCommand(Calories.MyIdeaIs)}` and `{EnumToCommand(Calories.MyIdeasCalled)}`";

        Dictionary<ulong, Idea> activeIdeas = new Dictionary<ulong, Idea>();
        List<Idea> SavedIdeas = new List<Idea>();

        public const string PathName = "Calories150";
        public const string FileName = "CaloriesIdea.json";

        public Calories150()
        {
            if (SaveAndLoad.FileExists(PathName, FileName))
            {
                SaveAndLoad.LoadFile(out SavedIdeas, PathName, FileName);
            }
        }

        public override async Task AsyncCommand(Enum command, SocketMessage message, string strippedContent, bool admin)
        {
            if (command.Equals(Calories.FernWhatIsTheCaloriesGame))
            {
                await message.Channel.SendMessageAsync(embed: new EmbedBuilder()
                {
                    Title = $"Calories game",
                    Description = WhatIsCaloriesGame,
                    Color = FeatureColor(),
                    ThumbnailUrl = message.Author.GetAvatarUrl()
                }.Build());
            }
            else if (command.Equals(Calories.FernHowDoIPlayCaloriesGame))
            {
                await message.Channel.SendMessageAsync(embed: new EmbedBuilder()
                {
                    Title = $"Calories game",
                    Description = HowDoIPlayCalorieGame,
                    Color = FeatureColor(),
                    ThumbnailUrl = message.Author.GetAvatarUrl()
                }.Build());
            }
            else if (command.Equals(Calories.FernLatestCaloriesGame))
            {
                await message.Channel.SendMessageAsync(embed: new EmbedBuilder()
                {
                    Title = $"Calories game",
                    Description = LatestBuildInformation,
                    Color = FeatureColor(),
                    ThumbnailUrl = message.Author.GetAvatarUrl()
                }.Build());
            }
            else if (command.Equals(Calories.FernIHaveAnIdea))
            {
                activeIdeas[message.Author.Id] = new Idea();
                activeIdeas[message.Author.Id].userName = message.Author.Username;

                await message.Channel.SendMessageAsync(embed: new EmbedBuilder()
                {
                    Title = $"Calories game",
                    Description = GiveUsAnIdeaName,
                    Color = FeatureColor(),
                    ThumbnailUrl = message.Author.GetAvatarUrl()
                }.Build());
            }
            else if (command.Equals(Calories.MyIdeasCalled) && activeIdeas.ContainsKey(message.Author.Id))
            {
                if (String.IsNullOrEmpty(strippedContent))
                {
                    await message.Channel.SendMessageAsync(embed: new EmbedBuilder()
                    {
                        Title = $"Calories game",
                        Description = YourIdeaIsEmpty,
                        Color = FeatureColor(),
                        ThumbnailUrl = message.Author.GetAvatarUrl()
                    }.Build());
                    return;
                }

                activeIdeas[message.Author.Id].name = strippedContent;
                await message.Channel.SendMessageAsync(embed: new EmbedBuilder()
                {
                    Title = $"Calories game",
                    Description = GiveUsAnIdeaDescription,
                    Color = FeatureColor(),
                    ThumbnailUrl = message.Author.GetAvatarUrl()
                }.Build());
            }
            else if (command.Equals(Calories.MyIdeaIs) && activeIdeas.ContainsKey(message.Author.Id))
            {
                if (String.IsNullOrEmpty(strippedContent))
                {
                    await message.Channel.SendMessageAsync(embed: new EmbedBuilder()
                    {
                        Title = $"Calories game",
                        Description = YourIdeaDescriptionIsEmpty,
                        Color = FeatureColor(),
                        ThumbnailUrl = message.Author.GetAvatarUrl()
                    }.Build());
                    return;
                }

                activeIdeas[message.Author.Id].content = strippedContent;
                await message.Channel.SendMessageAsync(ToPostYourIdeaUse, embed: new EmbedBuilder()
                {
                    Title = $"Calories game idea: {activeIdeas[message.Author.Id].name}",
                    Description = activeIdeas[message.Author.Id].content,
                    Color = FeatureColor(),
                    ThumbnailUrl = message.Author.GetAvatarUrl()
                }.Build());
            }
            else if (command.Equals(Calories.FernPostIdea) && activeIdeas.ContainsKey(message.Author.Id) 
                && !String.IsNullOrWhiteSpace(activeIdeas[message.Author.Id].name)
                && !String.IsNullOrWhiteSpace(activeIdeas[message.Author.Id].content))
            {
                await message.Channel.SendMessageAsync($"Thanks for posting your Idea {activeIdeas[message.Author.Id].name}");
                SavedIdeas.Add(activeIdeas[message.Author.Id]);
                activeIdeas.Remove(message.Author.Id);

                SaveAndLoad.SaveFile(SavedIdeas, PathName, FileName);
            }
            else if (command.Equals(Calories.FernSearchCaloriesGameIdea))
            {
                await message.Channel.SendMessageAsync("here are the results to your search:");
                foreach (var idea in SavedIdeas)
                {
                    int val = Utilities.CalcLevenshteinDistance(idea.name, strippedContent);
                    if (val < 2)
                    {
                        await message.Channel.SendMessageAsync(embed: new EmbedBuilder()
                        {
                            Title = $"Calories game idea: {idea.name}",
                            Description = idea.content + "\n\n" + idea.userName,
                            Color = FeatureColor(),
                            ThumbnailUrl = message.Author.GetAvatarUrl()
                        }.Build());
                    }
                }
            }
        }

        public override string FeatureName()
        {
            return "150 calories.";
        }

        public override Color FeatureColor()
        {
            return Color.Teal;
        }

        public override Enum? HelpCommand()
        {
            return Responses.Default.FernHelpCalories150;
        }
    }
}