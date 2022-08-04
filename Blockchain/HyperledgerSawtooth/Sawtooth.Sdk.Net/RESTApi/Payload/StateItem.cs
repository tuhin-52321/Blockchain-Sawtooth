using System.Text.Json.Serialization;

namespace Sawtooth.Sdk.Net.RESTApi.Payload
{
    public class StateItem
    {
        [JsonPropertyName("address")]
        public string? Address { get; set; }

        [JsonPropertyName("data")]
        public string? Data { get; set; }

    }
}
