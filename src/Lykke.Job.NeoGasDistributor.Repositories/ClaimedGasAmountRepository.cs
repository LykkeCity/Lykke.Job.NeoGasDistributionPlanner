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
    public class ClaimedGasAmountRepository : IClaimedGasAmountRepository
    {
        
        
        private readonly INoSQLTableStorage<ClaimedGasAmountEntity> _claimedGasAmountTable;

        
        private ClaimedGasAmountRepository(
            INoSQLTableStorage<ClaimedGasAmountEntity> claimedGasAmountTable)
        {
            _claimedGasAmountTable = claimedGasAmountTable;
        }
        
        public static IClaimedGasAmountRepository Create(
            IReloadingManager<string> connectionString,
            ILogFactory logFactory)
        {
            var claimedGasAmountTable = AzureTableStorage<ClaimedGasAmountEntity>.Create
            (
                connectionStringManager: connectionString,
                tableName: "ClaimedGasAmounts",
                logFactory: logFactory
            );
            
            return new ClaimedGasAmountRepository(claimedGasAmountTable);
        }

        
        public async Task<IReadOnlyCollection<ClaimedGasAmountAggregate>> GetAsync(
            DateTime from,
            DateTime to)
        {
            var claimedGasAmounts = new List<ClaimedGasAmountAggregate>();

            foreach (var rangeQuery in GetRangeQueries(from, to))
            {
                var continuationToken = (string) null;
                
                do
                {
                    IEnumerable<ClaimedGasAmountEntity> entities;
                    
                    (entities, continuationToken) = await _claimedGasAmountTable
                        .GetDataWithContinuationTokenAsync(rangeQuery, continuationToken);

                    claimedGasAmounts.AddRange(entities.Select(x => ClaimedGasAmountAggregate.CreateOrRestore
                    (
                        amount: x.Amount,
                        transactionId: x.TransactionId,
                        transactionBroadcastingMoment: x.TransactionBroadcastingMoment
                    )));

                } while (continuationToken != null);
            }
            
            return claimedGasAmounts;
        }

        public Task SaveAsync(
            ClaimedGasAmountAggregate claimedGasAmount)
        {
            return _claimedGasAmountTable.InsertOrReplaceAsync(new ClaimedGasAmountEntity
            {
                Amount = claimedGasAmount.Amount,
                TransactionId = claimedGasAmount.TransactionId,
                TransactionBroadcastingMoment = claimedGasAmount.TransactionBroadcastingMoment,

                PartitionKey = GetPartitionKey(claimedGasAmount.TransactionBroadcastingMoment),
                RowKey = GetRowKey(claimedGasAmount.TransactionBroadcastingMoment, claimedGasAmount.TransactionId)
            });
        }


        private static string GetPartitionKey(
            DateTime transactionBroadcastingMoment)
        {
            return transactionBroadcastingMoment.ToString("yyyyMM");
        }

        private static IEnumerable<TableQuery<ClaimedGasAmountEntity>> GetRangeQueries(
            DateTime from,
            DateTime to)
        {
            for (var partition = from.Date; partition.Date <= to; partition = partition.AddMonths(1))
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
            
                yield return new TableQuery<ClaimedGasAmountEntity>().Where(rangeQuery);
            }
        }

        private static string GetRowKey(
            DateTime transactionBroadcastingMoment,
            Guid transactionId)
        {
            return $"{transactionBroadcastingMoment.Ticks:D19}-{transactionId}";
        } 
    }
}
