using System;
using Newtonsoft.Json;

namespace Lykke.Job.NeoGasDistributor
{
    public class MeLogEntryHeader
    {
        [JsonProperty("messageType")]
        public string MessageType { get; set; }

        [JsonProperty("sequenceNumber")]
        public long SequenceNumber { get; set; }

        [JsonProperty("messageId")]
        public Guid MessageId { get; set; }

        [JsonProperty("requestId")]
        public Guid RequestId { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("timestamp")]
        public DateTimeOffset Timestamp { get; set; }

        [JsonProperty("eventType")]
        public string EventType { get; set; }
    }
}