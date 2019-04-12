using System;
using Lykke.AzureStorage.Tables;

namespace Lykke.Job.NeoGasDistributor.Repositories.Entities
{
    public class ClaimedGasAmountEntity : AzureTableEntity
    {
        public decimal Amount { get; set; }
        
        public Guid TransactionId  { get; set; }

        public DateTime TransactionBroadcastingMoment  { get; set; }
    }
}
