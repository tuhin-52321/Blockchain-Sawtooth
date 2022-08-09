using System.Text.Json.Serialization;

namespace Sawtooth.Sdk.Net.RESTApi.Payload.Json
{
    public class WSSubscribe
    {
        [JsonPropertyName("action")]
        public string? Action => "subscribe";

        [JsonPropertyName("address_prefixes")]
        public List<string> AddressPrefixes { get; set; } = new List<string>();

    }
}
