using PeterO.Cbor;
using System.Text.Json.Serialization;

namespace Sawtooth.Sdk.Net.RESTApi.Payload
{
    public class TransactionReceipt
    {
        [JsonPropertyName("transaction_id")]
        public string? TransactionId { get; set; }

        [JsonPropertyName("state_changes")]
        public List<StateChange?> StateChanges { get; set; } = new List<StateChange?>();

        [JsonPropertyName("events")]
        public List<Event?> InvalidTransaction { get; set; } = new List<Event?>();

        [JsonPropertyName("data")]
        public List<string?> Data { get; set; } = new List<string?>();

        public byte[] ToByteArray()
        {
            CBORObject obj = CBORObject.FromObject(this);

            return obj.EncodeToBytes();
        }


    }

}