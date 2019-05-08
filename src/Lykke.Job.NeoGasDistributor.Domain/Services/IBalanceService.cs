using System;
using System.Threading.Tasks;

namespace Lykke.Job.NeoGasDistributor.Domain.Services
{
    public interface IBalanceService
    {
        Task CreateSnapshotAsync(
            DateTime from,
            DateTime to);

        Task<DateTime?> TryGetFirstBalanceUpdateTimestampAsync();

        Task<DateTime?> TryGetLatestSnapshotTimestampAsync();
     
        Task RegisterBalanceUpdateAsync(
            Guid walletId,
            DateTime eventTimestamp,
            decimal newBalance);
    }
}
