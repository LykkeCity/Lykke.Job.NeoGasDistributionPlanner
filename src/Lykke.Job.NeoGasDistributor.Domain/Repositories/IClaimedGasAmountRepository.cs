using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Job.NeoGasDistributor.Domain.Repositories
{
    public interface IClaimedGasAmountRepository
    {
        Task<IReadOnlyCollection<ClaimedGasAmountAggregate>> GetAsync(
            DateTime from,
            DateTime to);

        Task SaveAsync(
            ClaimedGasAmountAggregate claimedGasAmount);
    }
}
