using System.Text.Json.Serialization;
using PeterO.Cbor;

namespace Sawtooth.Sdk.Net.RESTApi.Payload.Json
{
    public class TransactionHeader
    {
        [JsonPropertyName("batcher_public_key")]
        public string? BatcherPublicKey { get; set; }

        [JsonPropertyName("dependencies")]
        public List<string>? Dependencies { get; set; }

        [JsonPropertyName("family_name")]
        public string? FamilyName { get; set; }

        [JsonPropertyName("family_version")]
        public string? FamilyVersion { get; set; }

        [JsonPropertyName("inputs")]
        public List<string> Inputs { get; set; } = new List<string>();//default blnak list

        [JsonPropertyName("nonce")]
        public string? Nonce { get; set; }

        [JsonPropertyName("outputs")]
        public List<string> Outputs { get; set; } = new List<string>();//default blnak list 

        [JsonPropertyName("payload_sha512")]
        public string? PayloadSha512 { get; set; }

        [JsonPropertyName("signer_public_key")]
        public string? SignerPublicKey { get; set; }

        public byte[] ToByteArray()
        {
            CBORObject obj = CBORObject.FromObject(this);

            return obj.EncodeToBytes();
        }

    }
}