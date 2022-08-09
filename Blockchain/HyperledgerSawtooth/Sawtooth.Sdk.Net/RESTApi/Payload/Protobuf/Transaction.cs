using ProtoBuf;

namespace Sawtooth.Sdk.Net.RESTApi.Payload.Protobuf
{
    [ProtoContract]
    public class TransactionHeader
    {
        // Public key for the client who added this transaction to a batch
        [ProtoMember(1, Name = "batcher_public_key")]
        public string? BatcherPublicKey { get; set; }

        // A list of transaction signatures that describe the transactions that
        // must be processed before this transaction can be valid
        [ProtoMember(2, Name = "dependencies")]
        public List<string> Dependencies { get; set; } = new List<string>();

        // The family name correlates to the transaction processor's family name
        // that this transaction can be processed on, for example 'intkey'
        [ProtoMember(3, Name = "family_name")]
        public string? FamilyName { get; set; }

        // The family version correlates to the transaction processor's family
        // version that this transaction can be processed on, for example "1.0"
        [ProtoMember(4, Name = "family_version")]
        public string? FamilyVersion { get; set; }

        // A list of addresses that are given to the context manager and control
        // what addresses the transaction processor is allowed to read from.
        [ProtoMember(5, Name = "inputs")]
        public List<string> Inputs { get; set; } = new List<string>();

        // A random string that provides uniqueness for transactions with
        // otherwise identical fields.
        [ProtoMember(6, Name = "nonce")]
        public string? Nonce { get; set; }

        // A list of addresses that are given to the context manager and control
        // what addresses the transaction processor is allowed to write to.
        [ProtoMember(7, Name = "outputs")]
        public List<string> Outputs { get; set; } = new List<string>();

        //The sha512 hash of the encoded payload
        [ProtoMember(9, Name = "payload_sha512")]
        public string? PayloadSha512 { get; set; }

        // Public key for the client that signed the TransactionHeader
        [ProtoMember(10, Name = "signer_public_key")]
        public string? SignerPublicKey { get; set; }
    }

    [ProtoContract]
    public class Transaction
    {
        // The serialized version of the TransactionHeader
        [ProtoMember(1, Name = "header")]
        public byte[]? Header { get; set; }

        // The signature derived from signing the header
        [ProtoMember(2, Name = "header_signature")]
        public string HeaderSignature { get; set; } = "";

        // The payload is the encoded family specific information of the
        // transaction. Example cbor({'Verb': verb, 'Name': name,'Value': value})
        [ProtoMember(3, Name = "payload")]
        public byte[]? Payload { get; set; }

        public Transaction Clone()
        {
            return Serializer.DeepClone(this);
        }
    }

    // A simple list of transactions that needs to be serialized before
    // it can be transmitted to a batcher.
    [ProtoContract]
    public class TransactionList
    {
        [ProtoMember(1, Name = "transactions")]
        public List<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
