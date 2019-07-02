using System;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Job.NeoGasDistributor.Domain;
using Lykke.Job.NeoGasDistributor.Domain.Repositories;
using Lykke.Job.NeoGasDistributor.Domain.Services;

namespace Lykke.Job.NeoGasDistributor.Services
{
    [UsedImplicitly]
    public class BalanceService : IBalanceService
    {
        private readonly IBalanceUpdateRepository _balanceUpdateRepository;
        private readonly ISnapshotRepository _snapshotRepository;
        private readonly ILog _log;


        public BalanceService(
            ILogFactory logFactory,
            IBalanceUpdateRepository balanceUpdateRepository,
            ISnapshotRepository snapshotRepository)
        {
            _log = logFactory.CreateLog(this);
            _balanceUpdateRepository = balanceUpdateRepository;
            _snapshotRepository = snapshotRepository;
        }

        
        public async Task CreateSnapshotAsync(
            DateTime from,
            DateTime to)
        {
            _log.Info($"Creating balance snapshot for {from:s} - {to:s}...");

            var snapshotBalances = (await _balanceUpdateRepository
                .GetAsync(from, to))
                .OrderBy(x => x.EventTimestamp)
                .GroupBy(x => x.WalletId)
                .ToDictionary(x => x.Key, x => x.Select(y => y.NewBalance).Last());
            
            _log.Info($"{snapshotBalances.Count} balance updates found");

            var previousSnapshot = await _snapshotRepository.TryGetAsync(from);
            if (previousSnapshot != null)
            {
                _log.Info($"Previous snapshot found with {previousSnapshot.Balances.Count} balances");

                foreach (var balance in previousSnapshot.Balances)
                {
                    if (!snapshotBalances.ContainsKey(balance.WalletId))
                    {
                        snapshotBalances[balance.WalletId] = balance.Value;
                    }
                }
            }
            
            var newSnapshot = SnapshotAggregate.CreateOrRestore
            (
                balances: snapshotBalances.Select(x => SnapshotAggregate.Balance.CreateOrRestore
                (
                    balance: x.Value,
                    walletId: x.Key
                )),
                timestamp: to
            );

            await _snapshotRepository.SaveAsync(newSnapshot);
        }

        public Task<DateTime?> TryGetFirstBalanceUpdateTimestampAsync()
        {
            return _balanceUpdateRepository.TryGetFirstTimestampAsync();
        }

        public Task RegisterBalanceUpdateAsync(
            Guid walletId,
            DateTime eventTimestamp,
            decimal newBalance)
        {
            return _balanceUpdateRepository.SaveAsync(BalanceUpdateAggregate.CreateOrRestore
            (
                eventTimestamp: eventTimestamp,
                newBalance: newBalance,
                walletId: walletId)
            );
        }

        public Task<DateTime?> TryGetLatestSnapshotTimestampAsync()
        {
            return _snapshotRepository.TryGetLatestTimestampAsync();
        }
    }
}
