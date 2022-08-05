using System.Text.Json.Serialization;

namespace Sawtooth.Sdk.Net.RESTApi.Payload.Json
{
    public class Status
    {
        [JsonPropertyName("endpoint")]
        public string? Endpoint { get; set; }

        [JsonPropertyName("peers")]
        public List<string?> Peers { get; set; } = new List<string?>();

    }
}
