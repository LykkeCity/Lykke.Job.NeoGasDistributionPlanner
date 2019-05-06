using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.NeoGasDistributor.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class DbSettings
    {
        [AzureTableCheck]
        public string DataConnString { get; set; }
        
        public string MongoConnString { get; set; }
        
        [AzureTableCheck]
        public string LogsConnString { get; set; }
    }
}
