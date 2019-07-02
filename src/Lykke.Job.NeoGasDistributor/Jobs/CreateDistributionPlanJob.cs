using System;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Cronos;
using Lykke.Common.Log;
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
        private readonly ILog _log;

        public CreateDistributionPlanJob(
            ILogFactory logFactory,
            IBalanceService balanceService,
            CronExpression createDistributionPlanCron,
            TimeSpan createDistributionPlanDelay,
            IDateTimeProvider dateTimeProvider,
            IDistributionPlanService distributionPlanService)
        {
            _log = logFactory.CreateLog(this);
            _balanceService = balanceService;
            _createDistributionPlanCron = createDistributionPlanCron;
            _createDistributionPlanDelay = createDistributionPlanDelay;
            _dateTimeProvider = dateTimeProvider;
            _distributionPlanService = distributionPlanService;
        }

        public async Task ExecuteAsync()
        {
            _log.Info("Executing the job...");

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

                _log.Info("Context", new
                {
                    from,
                    missedExecutions
                });

                if (missedExecutions.Any())
                {
                    var to = missedExecutions.Last() - _createDistributionPlanDelay;
                    
                    await _distributionPlanService.CreatePlanAsync(from.Value, to);
                }

                _log.Info("Done");
            }
            else
            {
                _log.Info("No distribution plan has been created. 'from' is null");
            }
        }
    }
}
