using System;
using Lykke.AzureStorage.Tables;

namespace Lykke.Job.NeoGasDistributor.Repositories.Entities
{
    public class BalanceUpdateEntity : AzureTableEntity
    {
        public DateTime EventTimestamp { get; set; }
        
        public decimal NewBalance { get; set; }

        public Guid WalletId { get; set; }
    }
}
