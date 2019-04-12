using System;
using System.Threading.Tasks;

namespace Lykke.Job.NeoGasDistributor.Domain.Services
{
    public interface IDistributionPlanService
    {
        Task CreatePlanAsync(
            DateTime from,
            DateTime to);

        Task ExecutePlanAsync(
            Guid planId);

        Task<bool> PlanExistsAsync(
            Guid planId);

        Task<DateTime?> TryGetLatestPlanTimestampAsync();
    }
}
