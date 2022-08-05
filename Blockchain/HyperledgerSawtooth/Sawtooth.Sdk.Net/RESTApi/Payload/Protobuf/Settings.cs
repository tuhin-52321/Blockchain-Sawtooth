using ProtoBuf;
using System.ComponentModel;

namespace Sawtooth.Sdk.Net.RESTApi.Payload.Protobuf
{
    [ProtoContract]
    public class Entry
    {
        [ProtoMember(1, Name = "key")]
        public string? Key { get; set; }

        [ProtoMember(2, Name = "value")]
        public string? Value { get; set; }

    }

    [ProtoContract]
    public class Setting
    {
        [ProtoMember(1, Name = "entries")]
        public List<Entry> Entries { get; set; } = new List<Entry>();

    }

    [ProtoContract]
    public class VoteRecord
    {
        [ProtoMember(1, Name = "public_key")]
        public string? PublicKey { get; set; }

        [ProtoMember(2, Name = "vote")]
        public SettingVote.VoteEnum Vote { get; set; }

    }

    [ProtoContract]
    public class SettingCandidate
    {
        [ProtoMember(1, Name = "proposal_id")]
        public string? ProposalId { get; set; }

        [ProtoMember(2, Name = "proposal")]
        SettingProposal? Proposal { get; set; }

        [ProtoMember(3, Name = "votes")]
        public List<VoteRecord> Votes { get; set; } = new List<VoteRecord>();

    }

    [ProtoContract]
    public class SettingCandidates
    {
        [ProtoMember(1, Name = "candidates")]
        public List<SettingCandidate> Candidates { get; set; } = new List<SettingCandidate>();

    }

    [ProtoContract]
    public class SettingPayload
    {
        public enum ActionEnum { PROPOSE = 1, VOTE = 0 }

        [ProtoMember(1, Name = "action")]
        public ActionEnum Action { get; set; }

        [ProtoMember(2, Name = "data")]
        public byte[]? Data { get; set; }
    }

    [ProtoContract]
    public class SettingProposal
    {

        [ProtoMember(1, Name = "setting")]
        public string? Setting { get; set; }

        [ProtoMember(2, Name = "value")]
        public string? Value { get; set; }

        [ProtoMember(3, Name = "nonce")]
        public string? Nonce { get; set; }

    }

    [ProtoContract]
    public class SettingVote
    {
        public enum VoteEnum { ACCEPT = 0, REJECT = 1 }

        [ProtoMember(1, Name = "proposal_id")]
        public string? ProposalId { get; set; }

        [ProtoMember(2, Name = "vote")]
        public VoteEnum Vote { get; set; }
    }



}
