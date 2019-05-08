using System;
using System.ComponentModel;

namespace Lykke.Job.NeoGasDistributor.Domain
{
    [ImmutableObject(true)]
    public sealed class ClaimedGasAmountAggregate
    {
        private ClaimedGasAmountAggregate(
            decimal amount,
            Guid transactionId,
            DateTime transactionBroadcastingMoment)
        {
            Amount = amount;
            TransactionId = transactionId;
            TransactionBroadcastingMoment = transactionBroadcastingMoment;
        }
        
        public decimal Amount { get; }
        
        public Guid TransactionId  { get; }
        
        public DateTime TransactionBroadcastingMoment  { get; }
        
        
        public static ClaimedGasAmountAggregate CreateOrRestore(
            Guid transactionId,
            DateTime transactionBroadcastingMoment,
            decimal amount)
        {
            return new ClaimedGasAmountAggregate
            (
                amount,
                transactionId,
                transactionBroadcastingMoment
            );
        }
    }
}
