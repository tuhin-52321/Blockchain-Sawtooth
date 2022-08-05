using System.Text.Json.Serialization;

namespace Sawtooth.Sdk.Net.RESTApi.Payload.Json
{
    public class Event
    {
        [JsonPropertyName("event_type")]
        public string? EvetnType { get; set; }

        [JsonPropertyName("attributes")]
        public List<EventAttribute?> Attributes { get; set; } = new List<EventAttribute?>();


        [JsonPropertyName("data")]
        public string? Data { get; set; }

    }

    public class EventAttribute
    {
        [JsonPropertyName("key")]
        public string? Key { get; set; }

        [JsonPropertyName("value")]
        public string? Value { get; set; }

    }
}
