using System.Threading.Tasks;
using Lykke.Sdk;

namespace Lykke.Job.NeoGasDistributor
{
    internal static class Program
    {
        public static async Task Main()
        {
            #if DEBUG
            
            await LykkeStarter.Start<Startup>(true);
            
            #else
            
            await LykkeStarter.Start<Startup>(false);
            
            #endif
        }
    }
}
