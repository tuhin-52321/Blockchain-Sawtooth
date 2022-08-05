using System.Text.Json.Serialization;

namespace Sawtooth.Sdk.Net.RESTApi.Payload.Json
{

    public class BatchStatus
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("invalid_transactions")]
        public List<InvalidTransaction?> InvalidTransaction { get; set; } = new List<InvalidTransaction?>();

    }


}