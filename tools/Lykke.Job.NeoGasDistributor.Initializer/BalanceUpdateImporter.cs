using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Job.NeoGasDistributor.Domain;
using Lykke.Job.NeoGasDistributor.Domain.Repositories;

namespace Lykke.Job.NeoGasDistributor
{
    public class BalanceUpdateImporter
    {
        private readonly IBalanceUpdateRepository _balanceUpdateRepository;
        private readonly ILog _log;
        private readonly MELogReader _meLogReader;
        
        
        public BalanceUpdateImporter(
            IBalanceUpdateRepository balanceUpdateRepository,
            ILogFactory logFactory,
            MELogReader meLogReader)
        {
            _balanceUpdateRepository = balanceUpdateRepository;
            _log = logFactory.CreateLog(this);
            _meLogReader = meLogReader;
        }

        public async Task ImportBalancesAsync()
        {
            var logRecords = _meLogReader
                .GetRecords()
                .OrderBy(x => x.Date)
                .ToImmutableList();
            
            _log.Info($"Preparing {logRecords.Count} balance updates for import.");

            var progressCounter = 0;
            
            try
            {
                foreach (var logRecord in logRecords)
                {
                    await _balanceUpdateRepository.SaveAsync(BalanceUpdateAggregate.CreateOrRestore
                    (
                        eventTimestamp: logRecord.Date,
                        newBalance: logRecord.NewBalance,
                        walletId: logRecord.Id
                    ));
                    
                    _log.Info($"{++progressCounter} of {logRecords.Count} balance updates have been imported.");
                }
                
                _log.Info("Balance updated import has been completed.");
            }
            catch (Exception e)
            {
                _log.Error(e, "Balance updates import failed.");
            }
        }
    }
}
