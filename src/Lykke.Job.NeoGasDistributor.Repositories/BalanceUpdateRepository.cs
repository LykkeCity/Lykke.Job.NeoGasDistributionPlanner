using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using JetBrains.Annotations;
using Lykke.AzureStorage.Tables;
using Lykke.Common.Log;
using Lykke.Job.NeoGasDistributor.Domain;
using Lykke.Job.NeoGasDistributor.Domain.Repositories;
using Lykke.Job.NeoGasDistributor.Repositories.Entities;
using Lykke.SettingsReader;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Job.NeoGasDistributor.Repositories
{
    [UsedImplicitly]
    public class BalanceUpdateRepository : IBalanceUpdateRepository
    {
        private readonly INoSQLTableStorage<BalanceUpdateEntity> _balanceUpdateTable;

        private BalanceUpdateRepository(
            INoSQLTableStorage<BalanceUpdateEntity> balanceUpdateTable)
        {
            _balanceUpdateTable = balanceUpdateTable;
        }

        public static IBalanceUpdateRepository Create(
            IReloadingManager<string> connectionString,
            ILogFactory logFactory)
        {
            var balanceUpdateTable = AzureTableStorage<BalanceUpdateEntity>.Create
            (
                connectionStringManager: connectionString,
                tableName: "BalanceUpdates",
                logFactory: logFactory
            );
            
            return new BalanceUpdateRepository(balanceUpdateTable);
        }
        

        public async Task<IReadOnlyCollection<BalanceUpdateAggregate>> GetAsync(
            DateTime from,
            DateTime to)
        {
            var balanceUpdates = new List<BalanceUpdateAggregate>();
            
            foreach (var rangeQuery in GetRangeQueries(from, to))
            {
                var continuationToken = (string) null;

                do
                {
                    IEnumerable<BalanceUpdateEntity> entities;

                    (entities, continuationToken) = await _balanceUpdateTable
                        .GetDataWithContinuationTokenAsync(rangeQuery, 100, continuationToken);

                    balanceUpdates.AddRange(entities.Select(x => BalanceUpdateAggregate.CreateOrRestore
                    (
                        x.EventTimestamp,
                        x.NewBalance,
                        x.WalletId
                    )));
                    
                } while (continuationToken != null);
            }

            return balanceUpdates;
        }

        public Task SaveAsync(
            BalanceUpdateAggregate balanceUpdate)
        {
            var entity = new BalanceUpdateEntity
            {
                EventTimestamp = balanceUpdate.EventTimestamp,
                NewBalance = balanceUpdate.NewBalance,
                WalletId = balanceUpdate.WalletId,

                PartitionKey = GetPartitionKey(balanceUpdate.EventTimestamp),
                RowKey = GetRowKey(balanceUpdate.EventTimestamp, balanceUpdate.WalletId)
            };
            
            return _balanceUpdateTable.InsertOrReplaceAsync(entity);
        }

        public async Task<DateTime?> TryGetFirstTimestampAsync()
        {
            return (await _balanceUpdateTable
                    .GetTopRecordAsync(new TableQuery<BalanceUpdateEntity>()))?
                .EventTimestamp;
        }

        private static string GetPartitionKey(
            DateTime eventTimestamp)
        {
            return eventTimestamp.ToString("yyyyMMdd");
        }

        private static IEnumerable<TableQuery<BalanceUpdateEntity>> GetRangeQueries(
            DateTime from,
            DateTime to)
        {
            for (var partition = from.Date; partition.Date <= to; partition = partition.AddDays(1))
            {
                var partitionQuery = TableQuery.GenerateFilterCondition
                (
                    propertyName: nameof(AzureTableEntity.PartitionKey),
                    operation: QueryComparisons.Equal,
                    givenValue: GetPartitionKey(partition)
                );

                var fromQuery = TableQuery.GenerateFilterCondition
                (
                    propertyName: nameof(AzureTableEntity.RowKey),
                    operation: QueryComparisons.GreaterThan,
                    givenValue: GetRowKey(from, GuidMinMax.Min)
                );
                
                var toQuery = TableQuery.GenerateFilterCondition
                (
                    propertyName: nameof(AzureTableEntity.RowKey),
                    operation: QueryComparisons.LessThanOrEqual,
                    givenValue: GetRowKey(to, GuidMinMax.Max)
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
            
                yield return new TableQuery<BalanceUpdateEntity>().Where(rangeQuery);
            }
        }
        
        private static string GetRowKey(
            DateTime eventTimestamp,
            Guid walletId)
        {
            return $"{eventTimestamp.Ticks:D19}-{walletId}";
        }
    }
}
