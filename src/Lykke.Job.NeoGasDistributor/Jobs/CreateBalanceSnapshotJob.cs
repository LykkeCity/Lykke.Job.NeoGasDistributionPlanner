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
                var missedExecutions = _createBalanceSnapshotCron.GetOccurrences
                (
                    fromUtc: from.Value + _createBalanceSnapshotDelay,
                    toUtc: _dateTimeProvider.UtcNow - _createBalanceSnapshotDelay,
                    fromInclusive: false,
                    toInclusive: true
                );

                foreach (var missedExecution in missedExecutions)
                {
                    var to = missedExecution - _createBalanceSnapshotDelay;
                    
                    await _balanceService.CreateSnapshotAsync(from.Value, to);

                    from = to;
                }
            }
        }
    }
}
