using System;
using Lykke.AzureStorage.Tables;

namespace Lykke.Job.NeoGasDistributor.Repositories.Entities
{
    public class SnapshotEntity : AzureTableEntity
    {
        public DateTime SnapshotTimestamp { get; set; }
    }
}
