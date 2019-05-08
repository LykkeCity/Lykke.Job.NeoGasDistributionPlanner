using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Job.NeoClaimTransactionsExecutor.Contract;
using Lykke.Job.NeoGasDistributor.Domain.Services;

namespace Lykke.Job.NeoGasDistributor.Workflow.Projections
{
    public class GasClaimRegistrationProjection
    {
        private readonly IGasClaimService _gasClaimService;

        public GasClaimRegistrationProjection(
            IGasClaimService gasClaimService)
        {
            _gasClaimService = gasClaimService;
        }

        [UsedImplicitly]
        private Task Handle(
            GasClaimTransactionExecutedEvent evt)
        {
            return _gasClaimService.RegisterGasClaimAsync
            (
                transactionId: evt.TransactionId,
                transactionBroadcastingMoment: evt.BroadcastingMoment,
                amount: evt.Amount
            );
        }
    }
}
