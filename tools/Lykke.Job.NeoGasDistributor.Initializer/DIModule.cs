using Autofac;
using Lykke.Common.Log;
using Lykke.Job.NeoGasDistributor.Domain.Repositories;
using Lykke.Job.NeoGasDistributor.Repositories;
using Lykke.Job.NeoGasDistributor.Settings;
using Lykke.Logs;
using Lykke.Logs.Loggers.LykkeConsole;
using Lykke.SettingsReader;

namespace Lykke.Job.NeoGasDistributor
{
    public class DIModule : Module
    {
        private readonly string _meLogsFolderPath;
        private readonly IReloadingManager<AppSettings> _neoGasDistributorSettings;
        
        public DIModule(
            string meLogsFolderPath,
            IReloadingManager<AppSettings> neoGasDistributorSettings)
        {
            _meLogsFolderPath = meLogsFolderPath;
            _neoGasDistributorSettings = neoGasDistributorSettings;
        }
        
        protected override void Load(
            ContainerBuilder builder)
        {
            var logFactory = LogFactory.Create().AddConsole();

            builder
                .RegisterInstance(logFactory)
                .As<ILogFactory>();
            
            builder
                .RegisterType<MeLogReader>()
                .WithParameter(TypedParameter.From(_meLogsFolderPath))
                .SingleInstance()
                .AsSelf();
            
            builder
                .Register(ctx => BalanceUpdateRepository.Create(
                    _neoGasDistributorSettings.ConnectionString(x => x.NeoGasDistributor.Db.DataConnString),
                    ctx.Resolve<ILogFactory>()
                ))
                .As<IBalanceUpdateRepository>()
                .SingleInstance();

            builder
                .Register(ctx => new BalanceUpdateImporter
                (
                    _neoGasDistributorSettings.CurrentValue.NeoGasDistributor.NeoAssetId,
                    ctx.Resolve<IBalanceUpdateRepository>(),
                    ctx.Resolve<ILogFactory>(),
                    ctx.Resolve<MeLogReader>()
                ));
        }
    }
}
