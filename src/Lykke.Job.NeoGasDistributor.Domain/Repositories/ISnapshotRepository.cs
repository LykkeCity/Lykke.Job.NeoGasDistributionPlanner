using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Job.NeoGasDistributor.Domain.Repositories
{
    public interface ISnapshotRepository
    {
        Task<IReadOnlyCollection<SnapshotAggregate>> GetAsync(
            DateTime from,
            DateTime to);

        Task SaveAsync(
            SnapshotAggregate snapshot);

        Task<SnapshotAggregate> TryGetAsync(
            DateTime timestamp);
        
        Task<DateTime?> TryGetLatestTimestampAsync();
    }
}
