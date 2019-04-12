using System;
using System.Threading.Tasks;

namespace Lykke.Job.NeoGasDistributor.Domain.Repositories
{
    public interface IDistributionPlanRepository
    {
        Task<bool> PlanExistsAsync(
            Guid planId);
        
        Task SaveAsync(
            DistributionPlanAggregate distributionPlan);

        Task<DistributionPlanAggregate> TryGetAsync(
            Guid planId);

        Task<DateTime?> TryGetLatestTimestampAsync();
    }
}
