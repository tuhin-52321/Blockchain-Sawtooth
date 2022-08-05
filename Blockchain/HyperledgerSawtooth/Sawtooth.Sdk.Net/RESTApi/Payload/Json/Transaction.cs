using PeterO.Cbor;
using System.Text.Json.Serialization;

namespace Sawtooth.Sdk.Net.RESTApi.Payload.Json
{
    public class Transaction
    {
        [JsonPropertyName("header")]
        public TransactionHeader? Header { get; set; }

        [JsonPropertyName("header_signature")]
        public string? HeaderSignature { get; set; }

        [JsonPropertyName("payload")]
        public string? Payload { get; set; }

        public Transaction? Clone()
        {
            CBORObject obj = CBORObject.FromObject(this);

            return obj.ToObject(GetType()) as Transaction;
        }

        public byte[] ToByteArray()
        {
            CBORObject obj = CBORObject.FromObject(this);

            return obj.EncodeToBytes();
        }
    }
}