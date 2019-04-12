using System;
using System.Threading.Tasks;
using Cronos;
using Lykke.Job.NeoGasDistributor.Domain.Services;

namespace Lykke.Job.NeoGasDistributor.Jobs
{
    public class CreateBalanceSnapshotJob : IJob
    {
        private readonly IBalanceService _balanceService;
        private readonly CronExpression _createBalanceSnapshotCron;
        private readonly TimeSpan _createBalanceSnapshotDelay;

        
        public CreateBalanceSnapshotJob(
            IBalanceService balanceService,
            CronExpression createBalanceSnapshotCron,
            TimeSpan createBalanceSnapshotDelay)
        {
            _balanceService = balanceService;
            _createBalanceSnapshotCron = createBalanceSnapshotCron;
            _createBalanceSnapshotDelay = createBalanceSnapshotDelay;
        }

        
        public async Task ExecuteAsync()
        {
            var from = await _balanceService.TryGetLatestSnapshotTimestampAsync()
                    ?? await _balanceService.TryGetFirstBalanceUpdateTimestampAsync();

            if (from != null)
            {
                var missedTimestamps = _createBalanceSnapshotCron.GetOccurrences
                (
                    fromUtc: from.Value,
                    toUtc: DateTime.UtcNow - _createBalanceSnapshotDelay,
                    fromInclusive: false,
                    toInclusive: true
                );

                foreach (var to in missedTimestamps)
                {
                    await _balanceService.CreateSnapshotAsync(from.Value, to);

                    from = to;
                }
            }
        }
    }
}
