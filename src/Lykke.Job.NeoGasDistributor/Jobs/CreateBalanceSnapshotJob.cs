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
    public class CreateBalanceSnapshotJob : IJob
    {
        private readonly IBalanceService _balanceService;
        private readonly CronExpression _createBalanceSnapshotCron;
        private readonly TimeSpan _createBalanceSnapshotDelay;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILog _log;


        public CreateBalanceSnapshotJob(
            ILogFactory logFactory,
            IBalanceService balanceService,
            CronExpression createBalanceSnapshotCron,
            TimeSpan createBalanceSnapshotDelay,
            IDateTimeProvider dateTimeProvider)
        {
            _log = logFactory.CreateLog(this);
            _balanceService = balanceService;
            _createBalanceSnapshotCron = createBalanceSnapshotCron;
            _createBalanceSnapshotDelay = createBalanceSnapshotDelay;
            _dateTimeProvider = dateTimeProvider;
        }

        
        public async Task ExecuteAsync()
        {
            _log.Info("Executing the job...");

            var from = await _balanceService.TryGetLatestSnapshotTimestampAsync()
                    ?? await _balanceService.TryGetFirstBalanceUpdateTimestampAsync();

            if (from != null)
            {
                var to = _dateTimeProvider.UtcNow;
                
                var missedExecutions = _createBalanceSnapshotCron.GetOccurrences
                (
                    fromUtc: from.Value + _createBalanceSnapshotDelay,
                    toUtc: to,
                    fromInclusive: false,
                    toInclusive: true
                ).ToList();

                _log.Info("Context", new
                {
                    from,
                    to,
                    missedExecutions
                });

                foreach (var missedExecution in missedExecutions)
                {
                    to = missedExecution - _createBalanceSnapshotDelay;
                    
                    await _balanceService.CreateSnapshotAsync(from.Value, to);

                    from = to;
                }

                _log.Info("Done");
            }
            else
            {
                _log.Info("No balance snapshots have been created. 'from' is null");
            }
        }
    }
}
