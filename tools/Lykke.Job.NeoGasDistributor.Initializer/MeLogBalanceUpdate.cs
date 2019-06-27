using System;
using Newtonsoft.Json;

namespace Lykke.Job.NeoGasDistributor
{
    public class MeLogBalanceUpdate
    {
        [JsonProperty("walletId")]
        public Guid WalletId { get; set; }

        [JsonProperty("assetId")]
        public string AssetId { get; set; }

        [JsonProperty("oldBalance")]
        public string OldBalance { get; set; }

        [JsonProperty("newBalance")]
        [JsonConverter(typeof(ParseDecimalConverter))]
        public decimal NewBalance { get; set; }

        [JsonProperty("oldReserved")]
        public string OldReserved { get; set; }

        [JsonProperty("newReserved")]
        public string NewReserved { get; set; }
    }
}
