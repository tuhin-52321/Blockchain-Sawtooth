using System.Text.Json.Serialization;

namespace Sawtooth.Sdk.Net.RESTApi.Payload
{
    public class StateChange 
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("address")]
        public string? Address { get; set; }


        [JsonPropertyName("value")]
        public string? Value { get; set; }

    }
}
