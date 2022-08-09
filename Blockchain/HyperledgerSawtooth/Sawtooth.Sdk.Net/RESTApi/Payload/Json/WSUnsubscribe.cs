using System.Text.Json.Serialization;

namespace Sawtooth.Sdk.Net.RESTApi.Payload.Json
{
    public class WSUnSubscribe
    {
        [JsonPropertyName("action")]
        public string? Action => "unsubscribe";
    }
}
