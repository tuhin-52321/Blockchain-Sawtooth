using System.Text.Json.Serialization;

namespace Sawtooth.Sdk.Net.RESTApi.Payload.Json
{
    public class BatchHeader
    {
        [JsonPropertyName("signer_public_key")]
        public string? SignerPublicKey { get; set; }

        [JsonPropertyName("transaction_ids")]
        public List<string?> TransactionIds { get; set; } = new List<string?>();//blank list

    }
}