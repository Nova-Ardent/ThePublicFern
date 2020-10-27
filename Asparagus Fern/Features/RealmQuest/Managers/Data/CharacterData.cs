using System;
using System.Linq;

namespace Asparagus_Fern.Features.RealmQuest.Managers.Data
{
    public class AdventurerAttribute : Attribute
    {
        public const int initialPointsValues = 100;
        public const float percentCap = 1.05f;
        public const int addedCap = 5;

        public ulong vit;
        public ulong str;
        public ulong dex;
        public ulong intel;
        public ulong luck;

        public string emote;
        public string name;
        public string statDescription;

        public AdventurerAttribute() { }
        public AdventurerAttribute(ulong vit, ulong str, ulong dex, ulong intel, ulong luck, string emote, string name, string statDescription)
        {
            this.vit = vit;
            this.str = str;
            this.dex = dex;
            this.intel = intel;
            this.luck = luck;

            this.emote = emote;
            this.name = name;
            this.statDescription = statDescription;
        }

        public ulong[] GetPointsPerStat()
        {
            ulong[] maxValuesPerStat = new ulong[] { 
                (ulong)(vit * percentCap + addedCap), 
                (ulong)(str * percentCap + addedCap), 
                (ulong)(dex * percentCap + addedCap), 
                (ulong)(intel * percentCap + addedCap), 
                (ulong)(luck * percentCap + addedCap)
            };
            ulong[] Percentages = new ulong[] { vit, str, dex, intel, luck };
            ulong[] Points = new ulong[] { 0, 0, 0, 0, 0 };

            Random rand = new Random();
            ulong sumPercents = Percentages.Aggregate((a, c) => a + c);
            for (int i = 0; i < 100; i++)
            {
                ulong nextPoint = (ulong)rand.Next(0, (int)sumPercents);
                ulong currentPercentageSum = 0;
                for (int j = 0; j < 5; j++)
                {
                    currentPercentageSum += Percentages[j];
                    if (nextPoint < currentPercentageSum)
                    {
                        Points[j]++;
                        if (Points[j] >= maxValuesPerStat[j])
                        {
                            Percentages[j] = 0;
                        }
                        break;
                    }
                }
            }

            return Points;
        }
    }

    public enum AdventurerType
    {
        [AdventurerAttribute(1, 1, 1, 1, 1, "", "", "")] None,
        [AdventurerAttribute(35, 35, 10, 10, 10, "<:sword:768319277836664842>", "Warrior", "A warrior of strength of vitality")] Warrior,
        [AdventurerAttribute(10, 10, 10, 55, 15, "<:staff:768486309869387786>", "Warlock", "A masterful wizard, with extreme intelligence")] Warlock,
        [AdventurerAttribute(10, 10, 35, 10, 35, "<:thiefBag:768487611081162753>", "Thief", "A Swift thief, bound by luck")] Thief,
        [AdventurerAttribute(20, 10, 30, 20, 20, "<:knife:768488958707367996>", "Assassin", "A sneaky assasin of the night, lucky, swift, and tough")] Assassin,
        [AdventurerAttribute(35, 10, 10, 35, 10, "<:clericCross:768489856719650826>", "Cleric", "A intelligent, and tough healer")] Cleric,
        [AdventurerAttribute(25, 20, 10, 25, 10, "<:TemplarCross:768490278641336330>", "Templar", "A warrior of the light, intelligent, and strong")] Templar,
        [AdventurerAttribute(20, 20, 20, 20, 20, "<:spade:768491178865328190>", "Jack of Trades", "Never a master, but always capable of everything")] JackOfTrades
    }

    [System.Serializable]
    public class CharacterData
    {
        public ulong userID { get; set; }
        public string name { get; set; }

        public ulong experience { get; set; } = 0;
        public int level { get; set; } = -1;
        public ulong vitality { get; set; } = 0;
        public ulong strength { get; set; } = 0;
        public ulong dexterity { get; set; } = 0;
        public ulong intelligence { get; set; } = 0;
        public ulong luck { get; set; } = 0;

        public AdventurerType type { get; set; } = AdventurerType.None;

        public CharacterData() { }
        public CharacterData(ulong userID)
        {
            this.userID = userID;
        }
    }
}
