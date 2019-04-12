using System;

namespace Lykke.Job.NeoGasDistributor.Utils
{
    public interface IDateTimeProvider
    {
        DateTime UtcNow { get; }
    }
}
