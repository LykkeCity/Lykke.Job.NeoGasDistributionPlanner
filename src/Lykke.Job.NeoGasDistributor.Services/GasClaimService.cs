using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Job.NeoGasDistributor.Domain;
using Lykke.Job.NeoGasDistributor.Domain.Repositories;
using Lykke.Job.NeoGasDistributor.Domain.Services;

namespace Lykke.Job.NeoGasDistributor.Services
{
    [UsedImplicitly]
    public class GasClaimService : IGasClaimService
    {
        private readonly IClaimedGasAmountRepository _claimedGasAmountRepository;

        
        public GasClaimService(
            IClaimedGasAmountRepository claimedGasAmountRepository)
        {
            _claimedGasAmountRepository = claimedGasAmountRepository;
        }

        
        public Task RegisterGasClaimAsync(
            Guid transactionId,
            DateTime transactionBroadcastingMoment,
            decimal amount)
        {
            return _claimedGasAmountRepository.SaveAsync(ClaimedGasAmountAggregate.CreateOrRestore
            (
                transactionId,
                transactionBroadcastingMoment,
                amount
            ));
        }
    }
}
