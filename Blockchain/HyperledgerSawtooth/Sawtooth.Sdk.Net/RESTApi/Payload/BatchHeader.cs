using PeterO.Cbor;
using System.Text.Json.Serialization;

namespace Sawtooth.Sdk.Net.RESTApi.Payload
{
    public class BatchHeader
    {
        [JsonPropertyName("signer_public_key")]
        public string? SignerPublicKey { get; set; }

        [JsonPropertyName( "transaction_ids")]
        public List<string?> TransactionIds { get; set; } = new List<string?>();//blank list

        public byte[] ToByteArray()
        {
            CBORObject obj = CBORObject.FromObject(this);

            return obj.EncodeToBytes();
        }


    }
}