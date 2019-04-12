using System;
using Lykke.AzureStorage.Tables;

namespace Lykke.Job.NeoGasDistributor.Repositories.Entities
{
    public class SnapshotBalanceEntity : AzureTableEntity
    {
        public decimal BalanceValue { get; set; }
        
        public DateTime SnapshotTimestamp { get; set; }
        
        public Guid WalletId { get; set; }
    }
}
