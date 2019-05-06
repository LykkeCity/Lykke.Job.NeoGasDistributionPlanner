using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using JetBrains.Annotations;
using Lykke.AzureStorage.Tables;
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
    public class SnapshotRepository : ISnapshotRepository
    {
        private readonly IChaosKitty _chaosKitty;
        private readonly INoSQLTableStorage<SnapshotEntity> _snapshotTable;
        private readonly INoSQLTableStorage<SnapshotBalanceEntity> _balanceTable;

        public SnapshotRepository(
            IChaosKitty chaosKitty,
            INoSQLTableStorage<SnapshotEntity> snapshotTable,
            INoSQLTableStorage<SnapshotBalanceEntity> balanceTable)
        {
            _chaosKitty = chaosKitty;
            _snapshotTable = snapshotTable;
            _balanceTable = balanceTable;
        }
        
        public static ISnapshotRepository Create(
            IReloadingManager<string> connectionString,
            ILogFactory logFactory,
            IChaosKitty chaosKitty)
        {
            var snapshotTable = AzureTableStorage<SnapshotEntity>.Create
            (
                connectionStringManager: connectionString,
                tableName: "Snapshots",
                logFactory: logFactory
            );
            
            var balanceTable = AzureTableStorage<SnapshotBalanceEntity>.Create
            (
                connectionStringManager: connectionString,
                tableName: "SnapshotBalances",
                logFactory: logFactory
            );
            
            return new SnapshotRepository
            (
                chaosKitty,
                snapshotTable,
                balanceTable
            );
        }


        public async Task<IReadOnlyCollection<SnapshotAggregate>> GetAsync(
            DateTime from,
            DateTime to)
        {
            var snapshotEntities = await GetSnapshotEntitiesAsync(from, to);
            var balanceEntities = await GetBalanceEntitiesAsync(snapshotEntities); 

            var snapshots = new List<SnapshotAggregate>(snapshotEntities.Length);
            
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var snapshotEntity in snapshotEntities)
            {
                var balances = balanceEntities
                    .Where(x => x.SnapshotTimestamp == snapshotEntity.SnapshotTimestamp)
                    .Select(x => SnapshotAggregate.Balance.CreateOrRestore
                    (
                        balance: x.BalanceValue,
                        walletId: x.WalletId
                    ));

                var snapshot = SnapshotAggregate.CreateOrRestore
                (
                    balances: balances,
                    timestamp: snapshotEntity.SnapshotTimestamp
                );
                
                snapshots.Add(snapshot);
            }

            return snapshots;
        }

        public async Task SaveAsync(
            SnapshotAggregate snapshot)
        {
            await _balanceTable.InsertOrReplaceBatchAsync
            (
                snapshot.Balances.Select(x => new SnapshotBalanceEntity
                {
                    BalanceValue = x.Value,
                    SnapshotTimestamp = snapshot.Timestamp,
                    WalletId = x.WalletId,
                    
                    PartitionKey = $"{GetSnapshotPartitionKey(snapshot.Timestamp)}-{GetSnapshotRowKey(snapshot.Timestamp)}" ,
                    RowKey = $"{x.WalletId}"
                })
            );
            
            _chaosKitty.Meow(snapshot.Timestamp);

            await _snapshotTable.InsertOrReplaceAsync(new SnapshotEntity
            {
                SnapshotTimestamp = snapshot.Timestamp,
                
                PartitionKey = GetSnapshotPartitionKey(snapshot.Timestamp),
                RowKey = GetSnapshotRowKey(snapshot.Timestamp)
            });
        }

        public async Task<SnapshotAggregate> TryGetAsync(
            DateTime timestamp)
        {
            var snapshotEntity = await _snapshotTable.GetDataAsync
            (
                partition: GetSnapshotPartitionKey(timestamp),
                row: GetSnapshotRowKey(timestamp)
            );

            if (snapshotEntity != null)
            {
                var balanceEntities = await GetBalanceEntitiesAsync(snapshotEntity);

                return SnapshotAggregate.CreateOrRestore
                (
                    balances: balanceEntities.Select(x => SnapshotAggregate.Balance.CreateOrRestore
                    (
                        balance: x.BalanceValue,
                        walletId: x.WalletId
                    )),
                    timestamp: snapshotEntity.SnapshotTimestamp
                );
            }
            else
            {
                return null;
            }
        }

        public async Task<DateTime?> TryGetLatestTimestampAsync()
        {
            return (await _snapshotTable
                .GetTopRecordAsync(new TableQuery<SnapshotEntity>()))?
                .SnapshotTimestamp;
        }

        private async Task<IReadOnlyCollection<SnapshotBalanceEntity>> GetBalanceEntitiesAsync(
            params SnapshotEntity[] snapshotEntities)
        {
            var balanceEntities = new List<SnapshotBalanceEntity>();

            foreach (var snapshotEntity in snapshotEntities)
            {
                var continuationToken = (string) null;
                var rangeQuery = new TableQuery<SnapshotBalanceEntity>().Where(TableQuery.GenerateFilterCondition
                (
                    propertyName: nameof(AzureTableEntity.PartitionKey),
                    operation: QueryComparisons.Equal,
                    givenValue: $"{GetSnapshotPartitionKey(snapshotEntity.SnapshotTimestamp)}-{GetSnapshotRowKey(snapshotEntity.SnapshotTimestamp)}"
                ));
            
                do
                {
                    IEnumerable<SnapshotBalanceEntity> entities;

                    (entities, continuationToken) = await _balanceTable
                        .GetDataWithContinuationTokenAsync(rangeQuery, 1000, continuationToken);
                
                    balanceEntities.AddRange(entities);

                } while (continuationToken != null);
            }
            
            return balanceEntities;
        }

        private async Task<SnapshotEntity[]> GetSnapshotEntitiesAsync(
            DateTime from,
            DateTime to)
        {
            var snapshotEntities = new List<SnapshotEntity>();

            foreach (var rangeQuery in GetRangeQueries(from, to))
            {
                var continuationToken = (string) null;

                do
                {
                    IEnumerable<SnapshotEntity> entities;

                    (entities, continuationToken) = await _snapshotTable
                        .GetDataWithContinuationTokenAsync(rangeQuery, continuationToken);

                    snapshotEntities.AddRange(entities);

                } while (continuationToken != null);
            }
            
            return snapshotEntities.ToArray();
        }


        private static string GetSnapshotPartitionKey(
            DateTime snapshotTimestamp)
        {
            return $"{(9999 - snapshotTimestamp.Year) * 100 + 12 - snapshotTimestamp.Month:D6}";
        }
        
        private static IEnumerable<TableQuery<SnapshotEntity>> GetRangeQueries(
            DateTime from,
            DateTime to)
        {
            for (var partition = from.Date; partition.Date <= to; partition = partition.AddMonths(1))
            {
                var partitionQuery = TableQuery.GenerateFilterCondition
                (
                    propertyName: nameof(AzureTableEntity.PartitionKey),
                    operation: QueryComparisons.Equal,
                    givenValue: GetSnapshotPartitionKey(partition)
                );

                var fromQuery = TableQuery.GenerateFilterCondition
                (
                    propertyName: nameof(AzureTableEntity.RowKey),
                    operation: QueryComparisons.GreaterThanOrEqual,
                    givenValue: GetSnapshotRowKey(to)
                );
                
                var toQuery = TableQuery.GenerateFilterCondition
                (
                    propertyName: nameof(AzureTableEntity.RowKey),
                    operation: QueryComparisons.LessThan,
                    givenValue: GetSnapshotRowKey(from)
                );

                var rangeQuery = TableQuery.CombineFilters
                (
                    partitionQuery,
                    TableOperators.And,
                    TableQuery.CombineFilters
                    (
                        fromQuery,
                        TableOperators.And,
                        toQuery
                    )
                );
            
                yield return new TableQuery<SnapshotEntity>().Where(rangeQuery);
            }
        }
        
        private static string GetSnapshotRowKey(
            DateTime snapshotTimestamp)
        {
            return $"{DateTime.MaxValue.Ticks - snapshotTimestamp.Ticks:D19}";
        }
    }
}
