using System.Text.Json.Serialization;

namespace Sawtooth.Sdk.Net.RESTApi.Payload.Json
{
    public class Block
    {
        [JsonPropertyName("header")]
        public BlockHeader? Header { get; set; }

        [JsonPropertyName("header_signature")]
        public string? HeaderSignature { get; set; }

        [JsonPropertyName("batches")]
        public List<Batch?> Batches { get; set; } = new List<Batch?>();


    }
}
