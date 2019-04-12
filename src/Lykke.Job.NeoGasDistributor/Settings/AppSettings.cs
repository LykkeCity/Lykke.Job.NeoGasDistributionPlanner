using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.Sdk.Settings;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.NeoGasDistributor.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AppSettings : BaseAppSettings
    {
        [Optional]
        public ChaosSettings Chaos { get; set; }

        public CqrsSettings Cqrs { get; set; }
        
        public MatchingEngineClientSettings MatchingEngineClient { get; set; } 
        
        public NeoGasDistributorSettings NeoGasDistributor { get; set; }
    }
}
