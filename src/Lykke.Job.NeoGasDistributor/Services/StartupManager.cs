using System.Threading.Tasks;
using Hangfire;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Job.NeoGasDistributor.Jobs;
using Lykke.Sdk;

namespace Lykke.Job.NeoGasDistributor.Services
{
    [UsedImplicitly]
    public class StartupManager : IStartupManager
    {
        private readonly ICqrsEngine _cqrsEngine;
        private readonly string _createBalanceSnapshotCron;
        private readonly string _createDistributionPlanCron;

        
        public StartupManager(
            ICqrsEngine cqrsEngine,
            string createBalanceSnapshotCron,
            string createDistributionPlanCron)
        {
            _cqrsEngine = cqrsEngine;
            _createBalanceSnapshotCron = createBalanceSnapshotCron;
            _createDistributionPlanCron = createDistributionPlanCron;
        }

        
        public Task StartAsync()
        {
            _cqrsEngine.StartSubscribers();
            _cqrsEngine.StartProcesses();
            
            AddOrUpdateRecurringJob<CreateBalanceSnapshotJob>(_createBalanceSnapshotCron);
            AddOrUpdateRecurringJob<CreateDistributionPlanJob>(_createDistributionPlanCron);
            
            return Task.CompletedTask;
        }

        private static void AddOrUpdateRecurringJob<T>(
            string cronExpression)
        
            where T : IJob
        {
            RecurringJob.AddOrUpdate<T>
            (
                recurringJobId: typeof(T).Name,
                methodCall: job => job.ExecuteAsync(),
                cronExpression: cronExpression
            );
        }
    }
}
