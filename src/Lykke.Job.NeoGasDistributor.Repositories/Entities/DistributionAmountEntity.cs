using System;
using Lykke.AzureStorage.Tables;

namespace Lykke.Job.NeoGasDistributor.Repositories.Entities
{
    public class DistributionAmountEntity : AzureTableEntity
    {
        public Guid AmountId { get; set; }

        public decimal AmountValue { get; set; }
        
        public Guid WalletId { get; set; }
    }
}
