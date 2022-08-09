using System.Text.Json.Serialization;

namespace Sawtooth.Sdk.Net.RESTApi.Payload.Json
{
    public class WSEvent
    {
        [JsonPropertyName("block_num")]
        public string? BlockNum { get; set; }

        [JsonPropertyName("block_id")]
        public string? BlockId { get; set; }

        [JsonPropertyName("previous_block_id")]
        public string? PreviousBlockId { get; set; }

        [JsonPropertyName("state_changes")]
        public List<GlobalState> StateChanges { get; set; } = new List<GlobalState>();

        [JsonPropertyName("warning")]
        public string? Warning { get; set; }

        [JsonPropertyName("error")]
        public string? Error { get; set; }

    }

    public class GlobalState
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("value")]
        public string? Value { get; set; }

        [JsonPropertyName("address")]
        public string? Address { get; set; }
    }
}
