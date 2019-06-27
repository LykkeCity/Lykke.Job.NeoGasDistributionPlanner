using CommandLine;
using JetBrains.Annotations;

namespace Lykke.Job.NeoGasDistributor
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class Options
    {
        [Option("logs", Required = true, HelpText = "ME balance updates logs folder path.")]
        public string BalanceLogsFolderPath { get; set; } 
        
        [Option("settings", Required = true, HelpText = "NeoGasDistributor settings url.")]
        public string NeoGasDistributorSettingsUrl { get; set; }
    }
}
