using System.Text.Json.Serialization;

namespace Sawtooth.Sdk.Net.RESTApi.Payload.Json
{
    public class State
    {
        [JsonPropertyName("data")]
        public string? Data { get; set; }

        [JsonPropertyName("head")]
        public string? Head { get; set; }

        [JsonPropertyName("link")]
        public string? Link { get; set; }



    }
}
