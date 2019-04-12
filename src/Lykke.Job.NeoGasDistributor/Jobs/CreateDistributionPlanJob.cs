using System;
using System.Threading.Tasks;
using Cronos;
using Lykke.Job.NeoGasDistributor.Domain.Services;

namespace Lykke.Job.NeoGasDistributor.Jobs
{
    public class CreateDistributionPlanJob : IJob
    {
        private readonly IBalanceService _balanceService;
        private readonly CronExpression _createDistributionPlanCron;
        private readonly TimeSpan _createDistributionPlanDelay;
        private readonly IDistributionPlanService _distributionPlanService;

        public CreateDistributionPlanJob(
            IBalanceService balanceService,
            IDistributionPlanService distributionPlanService,
            CronExpression createDistributionPlanCron,
            TimeSpan createDistributionPlanDelay)
        {
            _balanceService = balanceService;
            _createDistributionPlanCron = createDistributionPlanCron;
            _createDistributionPlanDelay = createDistributionPlanDelay;
            _distributionPlanService = distributionPlanService;
        }

        public async Task ExecuteAsync()
        {
            var from = await _distributionPlanService.TryGetLatestPlanTimestampAsync();         
            var to   = DateTime.UtcNow - _createDistributionPlanDelay;
            
            if (from != null)
            {
                var missedTimestamps = _createDistributionPlanCron.GetOccurrences
                (
                    fromUtc: from.Value,
                    toUtc: to,
                    fromInclusive: false,
                    toInclusive: true
                );

                foreach (var missedTimestamp in missedTimestamps)
                {
                    to = missedTimestamp;
                    
                    await _distributionPlanService.CreatePlanAsync(from.Value, to);

                    from = to;
                }
            }
            else
            {
                from = await _balanceService.TryGetFirstBalanceUpdateTimestampAsync();
                
                await _distributionPlanService.CreatePlanAsync(from.Value, to);
            }
        }
    }
}
