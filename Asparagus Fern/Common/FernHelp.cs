using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;

namespace Asparagus_Fern.Common
{
    class FernHelp : DiscordIO<Responses.Default>
    {
        DiscordIO[] features;

        public FernHelp()
        {
            
        }

        public void AddFeatures(DiscordIO[] features)
        {
            this.features = features;
        }

        public override async Task AsyncCommand(Enum command, SocketMessage message, string strippedContent, bool admin)
        {
            foreach (var feature in features)
            {
                var helpCommand = feature.HelpCommand();
                if (helpCommand != null && command.Equals(helpCommand))
                {
                    await message.Channel.SendMessageAsync(embed: new EmbedBuilder()
                    {
                        Title = $"**help:** {feature.FeatureName()}",
                        Description = feature.HelpMessage(admin),
                        Color = feature.FeatureColor()
                    }.Build());
                }
            }
        }

        public override string FeatureName()
        {
            return "General Help";
        }

        public override Color FeatureColor()
        {
            return Color.Green;
        }

        public override Enum? HelpCommand()
        {
            return Responses.Default.FernHelp;
        }

        public override string HelpMessage(bool isAdmin)
        {
            string response = "";
            foreach (var feature in features)
            {
                var command = feature.HelpCommand();
                if (command == null)
                {
                    continue;
                }

                var attribute = Utilities.GetAttribute<Responses.ResponseAttribute>(command);


                try
                {
                    response += $"{String.Format(attribute.helpMessage, EnumToCommand(command))}\n\n";
                }
                catch (Exception e)
                {
                    Console.WriteLine($"issue with feature help command : {feature}");
                    Console.WriteLine(e);
                }
            }
            return response;
        }
    }
}
