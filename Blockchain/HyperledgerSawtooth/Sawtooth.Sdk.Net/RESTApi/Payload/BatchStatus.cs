using PeterO.Cbor;
using System.Text.Json.Serialization;

namespace Sawtooth.Sdk.Net.RESTApi.Payload
{

    public class BatchStatus
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName( "status")]
        public string? Status { get; set; }

        [JsonPropertyName("invalid_transactions")]
        public List<InvalidTransaction?> InvalidTransaction { get; set; } = new List<InvalidTransaction?>();

        public byte[] ToByteArray()
        {
            CBORObject obj = CBORObject.FromObject(this);

            return obj.EncodeToBytes();
        }


    }


}