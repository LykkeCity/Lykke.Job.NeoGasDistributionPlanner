using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;

namespace Lykke.Job.NeoGasDistributor.Domain
{
    [ImmutableObject(true)]
    public sealed class DistributionPlanAggregate
    {
        private DistributionPlanAggregate(
            IEnumerable<Amount> amounts,
            DateTime timestamp)
        
            : this (amounts, Guid.NewGuid(), timestamp)
        {
            
        }
        
        private DistributionPlanAggregate(
            IEnumerable<Amount> amounts,
            Guid id,
            DateTime timestamp)
        {
            Amounts = amounts.ToImmutableArray();
            Id = id;
            Timestamp = timestamp;
        }

        
        public IReadOnlyCollection<Amount> Amounts { get; }

        public Guid Id { get; }
        
        public DateTime Timestamp { get; }


        public static DistributionPlanAggregate Create(
            DateTime timestamp,
            IEnumerable<Amount> amounts)
        {
            return new DistributionPlanAggregate
            (
                amounts,
                timestamp
            );
        }

        public static DistributionPlanAggregate Restore(
            Guid id,
            DateTime timestamp,
            IEnumerable<Amount> amounts)
        {
            return new DistributionPlanAggregate
            (
                amounts,
                id,
                timestamp
            );
        }
        
        [ImmutableObject(true)]
        public sealed class Amount
        {
            private Amount(
                decimal value,
                Guid walletId)
        
                : this(Guid.NewGuid(), value, walletId)
            {
            
            }
        
            private Amount(
                Guid amountId,
                decimal value,
                Guid walletId)
            {
                Id = amountId;
                Value = value;
                WalletId = walletId;
            }

            public Guid Id { get; }
            
            public decimal Value { get; }

            public Guid WalletId { get; }


            public static Amount Create(
                Guid walletId,
                decimal value)
            {
                return new Amount(value, walletId);
            }
        
            public static Amount Restore(
                Guid walletId,
                Guid amountId,
                decimal value)
            {
                return new Amount(amountId, value, walletId);
            }
        }
    }
}
