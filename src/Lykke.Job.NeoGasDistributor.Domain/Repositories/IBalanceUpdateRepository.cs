using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Job.NeoGasDistributor.Domain.Repositories
{
    public interface IBalanceUpdateRepository
    {
        Task<IReadOnlyCollection<BalanceUpdateAggregate>> GetAsync(
            DateTime from,
            DateTime to);

        Task SaveAsync(
            BalanceUpdateAggregate balanceUpdate);

        Task SaveBatchAsync(
            IEnumerable<BalanceUpdateAggregate> balanceUpdates);

        Task<DateTime?> TryGetFirstTimestampAsync();
    }
}
