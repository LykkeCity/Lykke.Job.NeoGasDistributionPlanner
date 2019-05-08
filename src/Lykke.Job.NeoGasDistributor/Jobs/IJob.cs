using System.Threading.Tasks;

namespace Lykke.Job.NeoGasDistributor.Jobs
{
    public interface IJob
    {
        Task ExecuteAsync();
    }
}
