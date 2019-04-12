using Autofac;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.Common.Log;
using Lykke.Job.NeoGasDistributor.Domain.Repositories;
using Lykke.Job.NeoGasDistributor.Repositories;
using Lykke.Job.NeoGasDistributor.Settings;
using Lykke.SettingsReader;


namespace Lykke.Job.NeoGasDistributor.Modules
{
    [UsedImplicitly]
    public class RepositoriesModule : Module
    {
        private readonly IReloadingManager<string> _connectionString;
        

        public RepositoriesModule(
            IReloadingManager<AppSettings> appSettings)
        {
            _connectionString = appSettings.Nested(x => x.NeoGasDistributor.Db.DataConnString);
        }

        protected override void Load(
            ContainerBuilder builder)
        {

            builder
                .Register(ctx => BalanceUpdateRepository.Create(
                    _connectionString,
                    ctx.Resolve<ILogFactory>()
                ))
                .As<IBalanceUpdateRepository>()
                .SingleInstance();

            builder
                .Register(ctx => ClaimedGasAmountRepository.Create(
                    _connectionString,
                    ctx.Resolve<ILogFactory>()
                ))
                .As<IClaimedGasAmountRepository>()
                .SingleInstance();

            builder
                .Register(ctx => DistributionPlanRepository.Create(
                    _connectionString,
                    ctx.Resolve<ILogFactory>(),
                    ctx.Resolve<IChaosKitty>()
                ))
                .As<IDistributionPlanRepository>()
                .SingleInstance();

            builder
                .Register(ctx => SnapshotRepository.Create(
                    _connectionString,
                    ctx.Resolve<ILogFactory>(),
                    ctx.Resolve<IChaosKitty>()
                ))
                .As<ISnapshotRepository>()
                .SingleInstance();
        }
    }
}
