using System.Collections.Generic;
using Lykke.Job.NeoGasDistributor.Domain;

namespace Lykke.Job.NeoGasDistributor
{
    public class BalanceUpdateAggregateComparer : IEqualityComparer<BalanceUpdateAggregate>
    {
        public static readonly BalanceUpdateAggregateComparer Instance = new BalanceUpdateAggregateComparer();

        public bool Equals(BalanceUpdateAggregate x, BalanceUpdateAggregate y)
        {
            return x.WalletId == y.WalletId && x.EventTimestamp == y.EventTimestamp && x.NewBalance == y.NewBalance;
        }

        public int GetHashCode(BalanceUpdateAggregate obj)
        {
            unchecked
            {
                var hashCode = obj.NewBalance.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.EventTimestamp.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.WalletId.GetHashCode();

                return hashCode;
            }
        }
    }
}
