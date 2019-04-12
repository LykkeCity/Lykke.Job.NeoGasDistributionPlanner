using System;
using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.NeoGasDistributor.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class CqrsSettings
    {
        [AmqpCheck]
        public string RabbitConnectionString { get; set; }

        public TimeSpan RetryDelay { get; set; }
    }
}
