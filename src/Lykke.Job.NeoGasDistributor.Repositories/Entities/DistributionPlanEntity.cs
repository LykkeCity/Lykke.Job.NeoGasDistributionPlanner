using System;
using Lykke.AzureStorage.Tables;

namespace Lykke.Job.NeoGasDistributor.Repositories.Entities
{
    public class DistributionPlanEntity : AzureTableEntity
    {
        public Guid PlanId { get; set; }
        
        public DateTime PlanTimestamp { get; set; }
    }
}
