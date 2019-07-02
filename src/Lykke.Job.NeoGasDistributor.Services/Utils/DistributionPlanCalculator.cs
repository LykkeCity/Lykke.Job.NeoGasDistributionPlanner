using System;
using System.Collections.Generic;
using System.Linq;
using Lykke.Job.NeoGasDistributor.Domain;

namespace Lykke.Job.NeoGasDistributor.Services.Utils
{
    public static class DistributionPlanCalculator
    {
        private static IEnumerable<DistributionPlanAggregate.Amount> Empty
            => Enumerable.Empty<DistributionPlanAggregate.Amount>();
        
        
        public static IEnumerable<DistributionPlanAggregate.Amount> CalculateAmounts(
            IEnumerable<SnapshotAggregate> snapshots,
            IEnumerable<ClaimedGasAmountAggregate> claimedGasAmounts,
            int scale)
        {
            var totalGasAmount = claimedGasAmounts
                .Sum(x => x.Amount);

            if (totalGasAmount < 0)
            {
                throw new InvalidOperationException($"Total amount of claimed gas is lower than zero [{totalGasAmount}].");
            }
            
            if (totalGasAmount == 0)
            {
                return Empty;
            }
            
            var neoAmounts = snapshots
                .SelectMany(x => x.Balances)
                .GroupBy(x => x.WalletId)
                .Select(x => new
                {
                    WalletId = x.Key,
                    NeoAmount = x.Sum(y => y.Value)
                })
                .ToList();

            var totalNeoAmount = neoAmounts
                .Sum(x => x.NeoAmount);

            if (totalNeoAmount < 0)
            {
                throw new InvalidOperationException($"Total NEO amount is lower than zero [{totalNeoAmount}].");
            }

            if (totalNeoAmount == 0)
            {
                return Empty;
            }
            
            return neoAmounts
                .Select(x => new
                {
                    x.WalletId,
                    Ratio = (x.NeoAmount / totalNeoAmount).RoundDown(8)
                })
                .Select(x => DistributionPlanAggregate.Amount.Create
                (
                    walletId: x.WalletId,
                    value: (totalGasAmount * x.Ratio).RoundDown(scale)
                ))
                .Where(x => x.Value > 0);
        }
    }
}
