using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

public partial class Responses
{
    public enum Acronym
    {
        FernHelpAcryonym
    }
}

namespace Asparagus_Fern.Common
{
    class Acronym : DiscordIO
    {
        public enum Found
        {
            found
        }

        Responses.LookupTree<Enum> acryonymLookup = new Responses.LookupTree<Enum>();


        public const string PathName = "Dictionary";
        public const string FileNameNoSwears = "worlds_alphaNS.txt";

        Regex alpha = new Regex("[a-z](?!$)");

        public Acronym()
        {
            var filePathNS = Path.Combine(PathName, FileNameNoSwears);

            using (StreamReader streamReader = new StreamReader(filePathNS, Encoding.ASCII, true))
            {
                while (!streamReader.EndOfStream)
                {
                    string text = streamReader.ReadLine();
                    if (text.Length < 3 || text.IsSingleChar() || text.NoVowels())
                    {
                        continue;
                    }

                    acryonymLookup.AddValue(text.ToLower(), Found.found);
                }

                streamReader.Close();
            }
        }

        public override async Task AsyncMessage(SocketMessage message, string lowercase, bool isAdmin)
        {
            Console.WriteLine($"{message.Author} -- {message.Content}");
            if (String.IsNullOrWhiteSpace(lowercase))
            {
                return;
            }

            var split = lowercase.Split(' ').Select(s => s[0]).ToArray();
            var word = new string(split);
            if (acryonymLookup.FindExactHit(word) != null)
            {
                var acryonym = alpha.Replace(word, "$0. ").ToUpper();
                await message.Channel.SendMessageAsync(acryonym);

                Console.WriteLine($"{message.Author} -- {acryonym} : {message.Content}");
            }
        }

        public override string FeatureName()
        {
            return "";
        }

        public override Enum? HelpCommand()
        {
            return null;
        }
    }
}
