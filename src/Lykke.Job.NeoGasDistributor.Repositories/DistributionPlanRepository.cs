using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using AzureStorage.Tables.Templates.Index;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.Common.Log;
using Lykke.Job.NeoGasDistributor.Domain;
using Lykke.Job.NeoGasDistributor.Domain.Repositories;
using Lykke.Job.NeoGasDistributor.Repositories.Entities;
using Lykke.SettingsReader;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Job.NeoGasDistributor.Repositories
{
    [UsedImplicitly]
    public class DistributionPlanRepository : IDistributionPlanRepository
    {
        private static readonly TableQuery<DistributionPlanEntity> EmptyQuery = new TableQuery<DistributionPlanEntity>();
        
        private readonly IChaosKitty _chaosKitty;
        private readonly INoSQLTableStorage<DistributionAmountEntity> _distributionAmountTable;
        private readonly INoSQLTableStorage<AzureIndex> _distributionPlanIndexTable;
        private readonly INoSQLTableStorage<DistributionPlanEntity> _distributionPlanTable;

        
        private DistributionPlanRepository(
            IChaosKitty chaosKitty,
            INoSQLTableStorage<DistributionAmountEntity> distributionAmountTable,
            INoSQLTableStorage<AzureIndex> distributionPlanIndexTable,
            INoSQLTableStorage<DistributionPlanEntity> distributionPlanTable)
        {
            _chaosKitty = chaosKitty;
            _distributionAmountTable = distributionAmountTable;
            _distributionPlanIndexTable = distributionPlanIndexTable;
            _distributionPlanTable = distributionPlanTable;
        }
        
        public static IDistributionPlanRepository Create(
            IReloadingManager<string> connectionString,
            ILogFactory logFactory,
            IChaosKitty chaosKitty)
        {
            var distributionAmountTable = AzureTableStorage<DistributionAmountEntity>.Create
            (
                connectionStringManager: connectionString,
                tableName: "DistributionAmounts",
                logFactory: logFactory
            );
            
            var distributionPlanIndexTable = AzureTableStorage<AzureIndex>.Create
            (
                connectionStringManager: connectionString,
                tableName: "DistributionPlanIndices",
                logFactory: logFactory
            );
            
            var distributionPlanTable = AzureTableStorage<DistributionPlanEntity>.Create
            (
                connectionStringManager: connectionString,
                tableName: "DistributionPlans",
                logFactory: logFactory
            );
            
            return new DistributionPlanRepository
            (
                chaosKitty,
                distributionAmountTable,
                distributionPlanIndexTable,
                distributionPlanTable
            );
        }


        public async Task<bool> PlanExistsAsync(Guid planId)
        {
            var indexEntity = await _distributionPlanIndexTable.GetDataAsync
            (
                partition: GetIndexPartitionKey(planId),
                row: GetIndexRowKey(planId)
            );

            return indexEntity != null;
        }

        public async Task SaveAsync(
            DistributionPlanAggregate distributionPlan)
        {
            var planEntity = new DistributionPlanEntity
            {
                PlanId = distributionPlan.Id,
                PlanTimestamp = distributionPlan.Timestamp,

                PartitionKey = GetPlanPartitionKey(distributionPlan.Timestamp),
                RowKey = GetPlanRowKey(distributionPlan.Timestamp)
            };

            var indexEntity = new AzureIndex
            {
                PrimaryPartitionKey = planEntity.PartitionKey,
                PrimaryRowKey = planEntity.RowKey,

                PartitionKey = GetIndexPartitionKey(distributionPlan.Id),
                RowKey = GetIndexRowKey(distributionPlan.Id)
            };

            var amountsPartition = GetAmountPartitionKey(distributionPlan.Timestamp);
            var amountEntities = distributionPlan.Amounts.Select(x => new DistributionAmountEntity
            {
                AmountValue = x.Value,
                AmountId = x.Id,
                WalletId = x.WalletId,

                PartitionKey = amountsPartition,
                RowKey = GetAmountRowKey(x.WalletId)
            });

            await _distributionAmountTable.InsertOrReplaceBatchAsync(amountEntities);
            
            _chaosKitty.Meow(distributionPlan.Id);

            await _distributionPlanIndexTable.InsertOrReplaceAsync(indexEntity);
            
            _chaosKitty.Meow(distributionPlan.Id);

            await _distributionPlanTable.InsertOrReplaceAsync(planEntity);
        }

        public async Task<DistributionPlanAggregate> TryGetAsync(
            Guid planId)
        {
            var indexEntity = await _distributionPlanIndexTable.GetDataAsync
            (
                partition: GetIndexPartitionKey(planId),
                row: GetIndexRowKey(planId)
            );

            if (indexEntity == null)
            {
                return null;
            }
            
            var planEntity = await _distributionPlanTable.GetDataAsync
            (
                partition: indexEntity.PrimaryPartitionKey,
                row: indexEntity.PrimaryRowKey
            );

            if (planEntity == null)
            {
                return null;
            }
            
            var amountEntities = new List<DistributionAmountEntity>();
            var amountPartitionKey = GetAmountPartitionKey(planEntity.PlanTimestamp);
            var continuationToken = (string) null;

            do
            {
                IEnumerable<DistributionAmountEntity> entities;

                (entities, continuationToken) = await _distributionAmountTable
                    .GetDataWithContinuationTokenAsync(amountPartitionKey, 100, continuationToken);

                amountEntities.AddRange(entities);
                    
            } while (continuationToken != null);
                
            return DistributionPlanAggregate.Restore
            (planEntity.PlanId,
                planEntity.PlanTimestamp, amountEntities.Select(x => DistributionPlanAggregate.Amount.Restore(walletId: x.WalletId, amountId: x.AmountId, value: x.AmountValue)));
        }

        public async Task<DateTime?> TryGetLatestTimestampAsync()
        {
            return (await _distributionPlanTable
                .GetTopRecordAsync(EmptyQuery))?
                .PlanTimestamp;
        }

        private static string GetAmountPartitionKey(
            DateTime planTimestamp)
        {
            return $"{planTimestamp.Ticks:D19}";
        }
        
        private static string GetAmountRowKey(
            Guid walletId)
        {
            return walletId.ToString();
        }
        
        private static string GetIndexPartitionKey(
            Guid planId)
        {
            return planId.ToString();
        }
        
        private static string GetIndexRowKey(
            Guid planId)
        {
            return planId.ToString();
        }

        private static string GetPlanPartitionKey(
            DateTime planTimestamp)
        {
            return $"{9999 - planTimestamp.Year:D4}";
        }
        
        private static string GetPlanRowKey(
            DateTime planTimestamp)
        {
            return $"{DateTime.MaxValue.Ticks - planTimestamp.Ticks:D19}";
        }
    }
}
