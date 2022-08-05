using System.Text.Json.Serialization;

namespace Sawtooth.Sdk.Net.RESTApi.Payload.Json
{
    public class Paging
    {
        [JsonPropertyName("start")]
        public string? Start { get; set; }

        [JsonPropertyName("limit")]
        public int? Limit { get; set; }

        [JsonPropertyName("next_position")]
        public string? NextPosition { get; set; }

        [JsonPropertyName("next")]
        public string? Next { get; set; }
    }
}
