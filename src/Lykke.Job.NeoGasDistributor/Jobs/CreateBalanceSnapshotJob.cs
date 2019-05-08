using System;
using System.Linq;
using System.Threading.Tasks;
using Cronos;
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

        
        public CreateBalanceSnapshotJob(
            IBalanceService balanceService,
            CronExpression createBalanceSnapshotCron,
            TimeSpan createBalanceSnapshotDelay,
            IDateTimeProvider dateTimeProvider)
        {
            _balanceService = balanceService;
            _createBalanceSnapshotCron = createBalanceSnapshotCron;
            _createBalanceSnapshotDelay = createBalanceSnapshotDelay;
            _dateTimeProvider = dateTimeProvider;
        }

        
        public async Task ExecuteAsync()
        {
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

                foreach (var missedExecution in missedExecutions)
                {
                    to = missedExecution - _createBalanceSnapshotDelay;
                    
                    await _balanceService.CreateSnapshotAsync(from.Value, to);

                    from = to;
                }
            }
        }
    }
}
