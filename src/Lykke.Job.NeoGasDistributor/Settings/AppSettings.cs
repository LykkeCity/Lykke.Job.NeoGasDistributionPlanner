using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.Sdk.Settings;
using Lykke.Service.Assets.Client;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.NeoGasDistributor.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AppSettings : BaseAppSettings
    {
        public AssetServiceSettings AssetService { get; set; }
        
        [Optional]
        public ChaosSettings Chaos { get; set; }

        public CqrsSettings Cqrs { get; set; }
        
        public MatchingEngineClientSettings MatchingEngineClient { get; set; } 
        
        public NeoGasDistributorSettings NeoGasDistributor { get; set; }
    }
}
