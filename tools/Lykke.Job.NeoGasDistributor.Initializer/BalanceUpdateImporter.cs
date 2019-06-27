using System;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Job.NeoGasDistributor.Domain;
using Lykke.Job.NeoGasDistributor.Domain.Repositories;
using Lykke.Job.NeoGasDistributor.Repositories;
using MoreLinq;

namespace Lykke.Job.NeoGasDistributor
{
    public class BalanceUpdateImporter
    {
        private readonly string _neoAssetId;
        private readonly IBalanceUpdateRepository _balanceUpdateRepository;
        private readonly ILog _log;
        private readonly MeLogReader _meLogReader;
        
        
        public BalanceUpdateImporter(
            string neoAssetId,
            IBalanceUpdateRepository balanceUpdateRepository,
            ILogFactory logFactory,
            MeLogReader meLogReader)
        {
            _neoAssetId = neoAssetId;
            _balanceUpdateRepository = balanceUpdateRepository;
            _log = logFactory.CreateLog(this);
            _meLogReader = meLogReader;
        }

        public async Task ImportBalancesAsync()
        {
            var balanceUpdateBatches = _meLogReader.GetBalanceUpdates()
                .SelectMany
                (
                    x => x.BalanceUpdates
                        .Where(b => b.AssetId == _neoAssetId)
                        .Select(b => new
                        {
                            Date = x.Header.Timestamp.UtcDateTime, 
                            WalletId = b.WalletId,
                            NewBalance = b.NewBalance
                        })
                )
                .GroupBy(x => BalanceUpdateRepository.GetPartitionKey(x.Date));

            _log.Info($"Starting asset {_neoAssetId} balance updates import...");

            var batchesCounter = 0;
            var balanceUpdatesCounter = 0;
            
            try
            {
                foreach (var balanceUpdatesGroup in balanceUpdateBatches)
                {
                    foreach (var balanceUpdatesBatch in balanceUpdatesGroup.Batch(1000))
                    {
                        var aggregates = balanceUpdatesBatch
                            .Select(x => BalanceUpdateAggregate.CreateOrRestore
                            (
                                eventTimestamp: x.Date,
                                newBalance: x.NewBalance,
                                walletId: x.WalletId
                            ))
                            .Distinct(BalanceUpdateAggregateComparer.Instance)
                            .ToArray();

                        await _balanceUpdateRepository.SaveBatchAsync(aggregates);

                        balanceUpdatesCounter += aggregates.Count();

                        _log.Info($"{++batchesCounter} balance update batches and {balanceUpdatesCounter} balances have been imported.");
                    }
                }
                
                _log.Info($"Balance updates import has been completed. {balanceUpdatesCounter} balance updates have been imported.");
            }
            catch (Exception e)
            {
                _log.Error(e, "Balance updates import failed.");
            }
        }
    }
}
