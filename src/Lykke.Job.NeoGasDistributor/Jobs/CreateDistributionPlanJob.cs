using System;
using System.Linq;
using System.Threading.Tasks;
using Cronos;
using Lykke.Job.NeoGasDistributor.Domain.Services;
using Lykke.Job.NeoGasDistributor.Utils;

namespace Lykke.Job.NeoGasDistributor.Jobs
{
    public class CreateDistributionPlanJob : IJob
    {
        private readonly IBalanceService _balanceService;
        private readonly CronExpression _createDistributionPlanCron;
        private readonly TimeSpan _createDistributionPlanDelay;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IDistributionPlanService _distributionPlanService;

        public CreateDistributionPlanJob(
            IBalanceService balanceService,
            CronExpression createDistributionPlanCron,
            TimeSpan createDistributionPlanDelay,
            IDateTimeProvider dateTimeProvider,
            IDistributionPlanService distributionPlanService)
        {
            _balanceService = balanceService;
            _createDistributionPlanCron = createDistributionPlanCron;
            _createDistributionPlanDelay = createDistributionPlanDelay;
            _dateTimeProvider = dateTimeProvider;
            _distributionPlanService = distributionPlanService;
        }

        public async Task ExecuteAsync()
        {
            var from = await _distributionPlanService.TryGetLatestPlanTimestampAsync()
                    ?? await _balanceService.TryGetFirstBalanceUpdateTimestampAsync();

            if (from != null)
            {
                var missedExecutions = _createDistributionPlanCron.GetOccurrences
                (
                    fromUtc: from.Value,
                    toUtc: _dateTimeProvider.UtcNow,
                    fromInclusive: false,
                    toInclusive: true
                ).ToList();

                if (missedExecutions.Any())
                {
                    var to = missedExecutions.Last() - _createDistributionPlanDelay;
                    
                    await _distributionPlanService.CreatePlanAsync(from.Value, to);
                }
            }
        }
    }
}
