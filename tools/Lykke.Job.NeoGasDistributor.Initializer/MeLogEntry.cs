using System.Collections.Generic;
using Newtonsoft.Json;

namespace Lykke.Job.NeoGasDistributor
{
    public class MeLogEntry
    {
        [JsonProperty("balanceUpdates")]
        public List<MeLogBalanceUpdate> BalanceUpdates { get; set; }

        [JsonProperty("header")]
        public MeLogEntryHeader Header { get; set; }
    }
}
