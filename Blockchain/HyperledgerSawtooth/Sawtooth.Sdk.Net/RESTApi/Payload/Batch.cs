using System.Text.Json.Serialization;

namespace Sawtooth.Sdk.Net.RESTApi.Payload
{
    public class Batch
    {
        [JsonPropertyName("header")]
        public BatchHeader? Header { get; set; }

        [JsonPropertyName("header_signature")]
        public string? HeaderSignature { get; set; }

        [JsonPropertyName("transactions")]
        public List<Transaction?> Transactions { get; set; } = new List<Transaction?>();

    }
}
