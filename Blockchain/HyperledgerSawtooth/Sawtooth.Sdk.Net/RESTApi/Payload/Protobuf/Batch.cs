using ProtoBuf;

namespace Sawtooth.Sdk.Net.RESTApi.Payload.Protobuf
{
    [ProtoContract]
    public class BatchHeader
    {
        // Public key for the client that signed the BatchHeader
        [ProtoMember(1, Name = "signer_public_key")]
        public string? SignerPublicKey { get; set; }

        // List of transaction.header_signatures that match the order of
        // transactions required for the batch
        [ProtoMember(2, Name = "transaction_ids")]
        public List<string> TransactionIds { get; set; } = new List<string>();
    }

    [ProtoContract]
    public class Batch
    {
        // The serialized version of the BatchHeader
        [ProtoMember(1, Name = "header")]
        public byte[]? Header { get; set; }

        // The signature derived from signing the header
        [ProtoMember(2, Name = "header_signature")]
        public string? HeaderSignature { get; set; }

        // A list of the transactions that match the list of
        // transaction_ids listed in the batch header
        [ProtoMember(3, Name = "transactions")]
        public List<Transaction> Transactions { get; set; } = new List<Transaction>();

        // A debugging flag which indicates this batch should be traced through the
        // system, resulting in a higher level of debugging output.
        [ProtoMember(4, Name = "trace")]
        public bool? Trace { get; set; }
    }

    [ProtoContract]
    public class BatchList
    {
        [ProtoMember(1, Name = "batches")]
        public List<Batch> Batches { get; set; } = new List<Batch>();
    }
}
