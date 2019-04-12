using System;
using JetBrains.Annotations;

namespace Lykke.Job.NeoGasDistributor.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class NeoGasDistributorSettings
    {
        public string CreateDistributionPlanCron { get; set; }
        
        public TimeSpan CreateDistributionPlanDelay { get; set; }
        
        public string CreateBalanceSnapshotCron { get; set; }
        
        public TimeSpan CreateBalanceSnapshotDelay { get; set; }
        
        public DbSettings Db { get; set; }
        
        public string GasAssetId { get; set; }
        
        public string NeoAssetId { get; set; }
    }
}
