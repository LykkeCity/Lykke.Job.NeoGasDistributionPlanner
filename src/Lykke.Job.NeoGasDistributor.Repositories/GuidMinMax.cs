using System;

namespace Lykke.Job.NeoGasDistributor.Repositories
{
    internal static class GuidMinMax
    {
        public static readonly Guid Min = Guid.Empty;
        
        public static readonly Guid Max = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff");
    }
}
