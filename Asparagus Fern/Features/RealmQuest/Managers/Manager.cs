using Discord.WebSocket;
using System.Threading.Tasks;

namespace Asparagus_Fern.Features.RealmQuest.Managers
{
    public abstract class Manager
    {
        public abstract string helpCommand { get; }
        public abstract string questMasterHelpCommand { get; }
        public virtual async Task Help(SocketMessage message)
        {
        }
    }
}
