using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;

namespace Lykke.Job.NeoGasDistributor.Domain
{
    [ImmutableObject(true)]
    public sealed class SnapshotAggregate
    {
        private SnapshotAggregate(
            IEnumerable<Balance> balances,
            DateTime timestamp)
        {
            Balances = balances.ToImmutableArray();
            Timestamp = timestamp;
        }
        
        public IReadOnlyCollection<Balance> Balances { get; }
        
        public DateTime Timestamp { get; }


        public static SnapshotAggregate CreateOrRestore(
            IEnumerable<Balance> balances,
            DateTime timestamp)
        {
            return new SnapshotAggregate
            (
                balances,
                timestamp
            );
        }
        
        
        [ImmutableObject(true)]
        public sealed class Balance
        {
            private Balance(
                decimal value,
                Guid walletId)
            {
                Value = value;
                WalletId = walletId;
            }
        
            public decimal Value { get; }
        
            public Guid WalletId { get; }


            public static Balance CreateOrRestore(
                decimal balance,
                Guid walletId)
            {
                return new Balance
                (
                    balance,
                    walletId
                );
            }
        }
    }
}
