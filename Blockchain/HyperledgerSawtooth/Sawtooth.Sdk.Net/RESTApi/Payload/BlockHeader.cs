using PeterO.Cbor;
using System.Text.Json.Serialization;

namespace Sawtooth.Sdk.Net.RESTApi.Payload
{
    public class BlockHeader
    {
        [JsonPropertyName("block_num")]
        public string? BlockNum { get; set; }

        [JsonPropertyName("previous_block_id")]
        public string? PreviousBlockId { get; set; }

        [JsonPropertyName("signer_public_key")]
        public string? SignerPublicKey { get; set; }

        [JsonPropertyName("batch_ids")]
        public List<string?> BatchIds { get; set; } = new List<string?>();//blank list

        [JsonPropertyName("consensus")]
        public string? Consensus { get; set; }

        [JsonPropertyName("state_root_hash")]
        public string? StateRootHash { get; set; }


        public byte[] ToByteArray()
        {
            CBORObject obj = CBORObject.FromObject(this);

            return obj.EncodeToBytes();
        }


    }
}