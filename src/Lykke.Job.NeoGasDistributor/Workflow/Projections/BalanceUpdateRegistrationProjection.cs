using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Job.NeoGasDistributor.Domain.Services;
using Lykke.Service.Balances.Client.Events;

namespace Lykke.Job.NeoGasDistributor.Workflow.Projections
{
    public class BalanceUpdateRegistrationProjection
    {
        private readonly IBalanceService _balanceService;
        private readonly string _neoAssetId;

        
        public BalanceUpdateRegistrationProjection(
            IBalanceService balanceService,
            string neoAssetId)
        {
            _balanceService = balanceService;
            _neoAssetId = neoAssetId;
        }

        
        [UsedImplicitly]
        public async Task Handle(
            BalanceUpdatedEvent evt)
        {
            if (evt.AssetId == _neoAssetId)
            {
                await _balanceService.RegisterBalanceUpdateAsync
                (
                    walletId: Guid.Parse(evt.WalletId),
                    eventTimestamp: evt.Timestamp,
                    newBalance: decimal.Parse(evt.Balance)
                );
            }
        }
    }
}
