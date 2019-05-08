using JetBrains.Annotations;

namespace Lykke.Job.NeoGasDistributor.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class MatchingEngineClientSettings
    {
        public string Host { get; set; }
        
        public int Port { get; set; }
    }
}
