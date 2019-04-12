using Autofac;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.Job.NeoGasDistributor.Settings;
using Lykke.Job.NeoGasDistributor.Utils;
using Lykke.SettingsReader;

namespace Lykke.Job.NeoGasDistributor.Modules
{
    [UsedImplicitly]
    public class MiscModule : Module
    {
        private readonly ChaosSettings _chaosSettings;


        public MiscModule(
            IReloadingManager<AppSettings> appSettings)
        {
            _chaosSettings = appSettings.CurrentValue.Chaos;
        }
        
        protected override void Load(
            ContainerBuilder builder)
        {
            builder
                .RegisterChaosKitty(_chaosSettings);

            builder
                .RegisterType<DateTimeProvider>()
                .As<IDateTimeProvider>()
                .SingleInstance();
        }
    }
}
