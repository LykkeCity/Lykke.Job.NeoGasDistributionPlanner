using System;
using Lykke.Job.NeoGasDistributor.Utils;

namespace Lykke.Job.NeoGasDistributor.Tests.Utils
{
    public class FakeDateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow { get; set; }
    }
}
