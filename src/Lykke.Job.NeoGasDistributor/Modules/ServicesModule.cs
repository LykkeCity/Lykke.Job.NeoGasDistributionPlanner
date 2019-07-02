using Autofac;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Job.NeoGasDistributor.Domain.Repositories;
using Lykke.Job.NeoGasDistributor.Domain.Services;
using Lykke.Job.NeoGasDistributor.Services;
using Lykke.Job.NeoGasDistributor.Settings;
using Lykke.MatchingEngine.Connector.Abstractions.Services;
using Lykke.Sdk;
using Lykke.Service.Assets.Client;
using Lykke.SettingsReader;

namespace Lykke.Job.NeoGasDistributor.Modules
{
    [UsedImplicitly]
    public class ServicesModule : Module
    {
        private readonly NeoGasDistributorSettings _jobSettings;
        

        public ServicesModule(
            IReloadingManager<AppSettings> appSettings)
        {
            _jobSettings = appSettings.Nested(x => x.NeoGasDistributor).CurrentValue;
        }

        protected override void Load(
            ContainerBuilder builder)
        {
            builder
                .Register(ctx => new StartupManager
                (
                    ctx.Resolve<ICqrsEngine>(),
                    _jobSettings.CreateBalanceSnapshotCron,
                    _jobSettings.CreateDistributionPlanCron
                ))
                .As<IStartupManager>()
                .SingleInstance();

            builder
                .Register(ctx => new BalanceService
                (
                    ctx.Resolve<ILogFactory>(),
                    ctx.Resolve<IBalanceUpdateRepository>(),
                    ctx.Resolve<ISnapshotRepository>()
                ))
                .As<IBalanceService>()
                .SingleInstance();

            builder
                .Register(ctx => new DistributionPlanService
                (
                    ctx.Resolve<IAssetsService>(),
                    ctx.Resolve<IClaimedGasAmountRepository>(),
                    ctx.Resolve<IDistributionPlanRepository>(),
                    ctx.Resolve<ILogFactory>(),
                    ctx.Resolve<IMatchingEngineClient>(),
                    ctx.Resolve<ISnapshotRepository>(),
                    _jobSettings.GasAssetId
                ))
                .As<IDistributionPlanService>()
                .SingleInstance();

            builder
                .Register(ctx => new GasClaimService
                (
                    ctx.Resolve<IClaimedGasAmountRepository>()
                ))
                .As<IGasClaimService>()
                .SingleInstance();
        }
    }
}
