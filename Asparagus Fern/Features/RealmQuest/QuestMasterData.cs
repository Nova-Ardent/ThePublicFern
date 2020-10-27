using System.Collections.Generic;
using System.IO;
using System.Linq;
using Asparagus_Fern.Tools;

namespace Asparagus_Fern.Features.RealmQuest
{
    [System.Serializable]
    public class QuestMasterData
    {
        [System.Serializable]
        public struct User
        {
            public ulong id { get; set; }
            public string name { get; set; }
            public string discrim { get; set; }
            public ulong createdQuests { get; set; }
            public ulong timesPlayed { get; set; }
        }

        public static string savePath = "QuestOwners.json";

        public ulong owner;
        public string ownerName;
        public string ownerDescrim;
        public List<User> questMasters { get; set; } = new List<User>();

        public QuestMasterData()
        {
        }

        public void Save()
        {
            SaveAndLoad.SaveFile(this, Directory.GetCurrentDirectory(), QuestMasterData.savePath);
        }

        public bool IsQuestMaster(ulong id)
        {
            return owner == id || questMasters.Any(x => x.id == id);
        }

        public bool IsOwner(ulong id)
        {
            return owner == id;
        }
    }
}
