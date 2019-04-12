using System;
using System.Threading.Tasks;

namespace Lykke.Job.NeoGasDistributor.Domain.Services
{
    public interface IGasClaimService
    {
        Task RegisterGasClaimAsync(
            Guid transactionId,
            DateTime transactionBroadcastingMoment,
            decimal amount);
    }
}
