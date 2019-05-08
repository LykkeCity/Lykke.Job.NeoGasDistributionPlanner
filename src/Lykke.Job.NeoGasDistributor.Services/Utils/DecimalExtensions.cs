using System;

namespace Lykke.Job.NeoGasDistributor.Services.Utils
{
    internal static class DecimalExtensions
    {
        public static decimal RoundDown(
            this decimal value,
            int scale)
        {
            if (scale < 0)
            {
                throw new ArgumentException("Should be greater or equal to zero.", nameof(scale));
            }
            
            var power = (decimal) Math.Pow(10, scale);
            
            return Math.Floor(value * power) / power;
        }
    }
}
