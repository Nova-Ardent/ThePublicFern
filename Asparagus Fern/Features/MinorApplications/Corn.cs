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


namespace Asparagus_Fern.Features.MinorApplications
{
    public class Corn : DiscordIO
    {
        bool goodForNext = false;
        Random random = new Random();
        string[] fileNames;

        public Corn()
        {
            goodForNext = true;
            fileNames = Directory.GetFiles("C:\\Users\\Duckie\\Desktop\\discord\\ThePublicFern\\Corn\\")
                .Select(x => x.Split("\\").Last())
                .ToArray();
        }

        public override async Task AsyncMessage(SocketMessage message, string lowercase, bool isAdmin)
        {
            if (goodForNext && lowercase.Contains("corn"))
            {
                goodForNext = false;
                var file = fileNames[random.Next(0, fileNames.Length)];
                await message.Channel.SendFileAsync("C:\\Users\\Duckie\\Desktop\\discord\\ThePublicFern\\Corn\\" + file, $"{EmojiList.corn} cornographer has spoken. {EmojiList.corn} I'll be back in at most 10 seconds with more corn.");
            }
            
        }

        public override void TenSecondTask(object source, ElapsedEventArgs e)
        {
            goodForNext = true;
            base.ThirtySecondTask(source, e);
        }
    }
}
