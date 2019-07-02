using Autofac;
using Cronos;
using Hangfire;
using Hangfire.Mongo;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Job.NeoGasDistributor.Domain.Services;
using Lykke.Job.NeoGasDistributor.Jobs;
using Lykke.Job.NeoGasDistributor.Settings;
using Lykke.Job.NeoGasDistributor.Utils;
using Lykke.Logs.Hangfire;
using Lykke.SettingsReader;


namespace Lykke.Job.NeoGasDistributor.Modules
{
    [UsedImplicitly]
    public class JobsModule : Module
    {
        private readonly DbSettings _dbSettings;
        private readonly NeoGasDistributorSettings _jobSettings;
        
        public JobsModule(
            IReloadingManager<AppSettings> appSettings)
        {
            _dbSettings = appSettings.Nested(x => x.NeoGasDistributor.Db).CurrentValue;
            _jobSettings = appSettings.Nested(x => x.NeoGasDistributor).CurrentValue;
        }
        
        
        protected override void Load(
            ContainerBuilder builder)
        {
            builder
                .Register(ctx => new CreateBalanceSnapshotJob
                (
                    ctx.Resolve<ILogFactory>(),
                    ctx.Resolve<IBalanceService>(),
                    CronExpression.Parse(_jobSettings.CreateBalanceSnapshotCron),
                    _jobSettings.CreateBalanceSnapshotDelay,
                    ctx.Resolve<IDateTimeProvider>()
                ))
                .AsSelf()
                .SingleInstance();

            builder
                .Register(ctx => new CreateDistributionPlanJob
                (
                    ctx.Resolve<ILogFactory>(),
                    ctx.Resolve<IBalanceService>(),
                    CronExpression.Parse(_jobSettings.CreateDistributionPlanCron),
                    _jobSettings.CreateDistributionPlanDelay,
                    ctx.Resolve<IDateTimeProvider>(),
                    ctx.Resolve<IDistributionPlanService>()
                ))
                .AsSelf()
                .SingleInstance();
            
            builder
                .RegisterBuildCallback(StartHangfireServer)
                .Register(ctx => new BackgroundJobServer())
                .SingleInstance();
        }
        
        private void StartHangfireServer(
            IContainer container)
        {
            GlobalConfiguration.Configuration
                .UseMongoStorage
                (
                    connectionString: _dbSettings.MongoConnString,
                    databaseName: "NeoGasDistributor"
                );
            
            GlobalConfiguration.Configuration
                .UseLykkeLogProvider(container)
                .UseAutofacActivator(container);

            container.Resolve<BackgroundJobServer>();
        }
    }
}
