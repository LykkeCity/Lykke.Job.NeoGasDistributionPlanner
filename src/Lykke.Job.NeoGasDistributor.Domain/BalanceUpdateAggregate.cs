using System;
using System.ComponentModel;

namespace Lykke.Job.NeoGasDistributor.Domain
{
    [ImmutableObject(true)]
    public sealed class BalanceUpdateAggregate
    {
        private BalanceUpdateAggregate(
            DateTime eventTimestamp,
            decimal newBalance,
            Guid walletId)
        {
            NewBalance = newBalance;
            EventTimestamp = eventTimestamp;
            WalletId = walletId;
        }

        
        public DateTime EventTimestamp { get; }
        
        public decimal NewBalance { get; }

        public Guid WalletId { get; }


        public static BalanceUpdateAggregate CreateOrRestore(
            DateTime eventTimestamp,
            decimal newBalance,
            Guid walletId)
        {
            return new BalanceUpdateAggregate
            (
                eventTimestamp,
                newBalance,
                walletId
            );
        }
    }
}
