using ProtoBuf;
using Sawtooth.Sdk.Net.RESTApi.Payload.Protobuf;
using Sawtooth.Sdk.Net.Utils;
using System.Text;

namespace Sawtooth.Sdk.Net.Transactions
{
    public class SawtoothSettingsTransactionFamily : TransactionFamily
    {

        public SawtoothSettingsTransactionFamily(string version) : base("sawtooth_settings", version)
        {
            if (version == "1.0")
            {
                SetHandlers(new SawtoothSettingsState(), new SawtoothSettingsTransaction());
            }
        }

    }

    public class SawtoothSettingsAddress : IAddress
    {
        public string Prefix => "000000";

        public string ComposeAddress(string context)
        {
            string[] comps = context.Split(new char[] { '.' }, 4);

            string part1 = comps[0];
            string part2 = comps.Length > 1 ? comps[1] : "";
            string part3 = comps.Length > 2 ? comps[2] : "";
            string part4 = comps.Length > 3 ? comps[3] : "";

            return Prefix
                    + Encoding.UTF8.GetBytes(part1).ToSha256().ToHexString().First(16)
                    + Encoding.UTF8.GetBytes(part2).ToSha256().ToHexString().First(16)
                    + Encoding.UTF8.GetBytes(part3).ToSha256().ToHexString().First(16)
                    + Encoding.UTF8.GetBytes(part4).ToSha256().ToHexString().First(16);

        }

    }

    public class SawtoothSettingsState : State
    {
        public SawtoothSettingsState() : base(new SawtoothSettingsAddress())
        {
        }

        public Entry? Entry { get; private set; }

        public override string DisplayString {
            get
            {
                string buffer = "[Protobuf Object: Setting]\n";
                if (Entry != null)
                {
                    buffer += "{\n";
                    if ("sawtooth.settings.vote.proposals".Equals(Entry.Key))
                    {
                        buffer += $"   {Entry.Key} = {{ \n";
                        if (Entry.Value != null)
                        {
                            byte[] setting_candidate_encoded = Convert.FromBase64String(Entry.Value);
                            using (MemoryStream stream = new MemoryStream(setting_candidate_encoded))
                            {
                                SettingCandidate setting_candidate = Serializer.Deserialize<SettingCandidate>(stream);

                                buffer += $"                    ProposalId = {setting_candidate.ProposalId} \n";
                                buffer += $"                    Votes = [ \n";
                                foreach (VoteRecord vote in setting_candidate.Votes)
                                {
                                    buffer += $"                            {{\n";
                                    buffer += $"                               PublicKey = {vote.PublicKey}\n";
                                    buffer += $"                               Vote      = {vote.Vote}\n";
                                    buffer += $"                            }}\n";
                                }
                                buffer += $"                    Votes = ] \n";

                            }
                        }
                        buffer += $"                 }} \n";

                    }
                    else
                    {
                        buffer += $"   {Entry.Key} = {Entry.Value} \n";
                    }
                    buffer += "}\n";
                }
                else
                {
                    buffer += "<Null Value>";
                }

                return buffer;
            }
        }

        public override void UnwrapState(string? state_payload)
        {
            if (state_payload == null) return;

            byte[] paylod_raw = Convert.FromBase64String(state_payload);
            using (MemoryStream stream = new MemoryStream(paylod_raw))
            {
                Entry = Serializer.Deserialize<Entry>(stream);
            }
        }

        public override void WrapState(out string? address, out string? state_payload)
        {
            address = null;
            state_payload = null;
            if (Entry != null)
            {
                if(Entry.Key != null) address = Address.ComposeAddress(Entry.Key);
                state_payload = Convert.ToBase64String(Entry.ToProtobufByteArray());
            }
        }
    }

    public class SawtoothSettingsTransaction : ITransaction
    {
        public SettingPayload? SettingPayload { get; private set; }

        private string? proposal_key;

        public string DisplayString
        {
            get
            {
                string buf = "[Protobuf Object : SettingPayload]\n";

                if (SettingPayload != null)
                {
                    buf += "Action : " + SettingPayload.Action + "\n";

                    if (SettingPayload.Data != null)
                    {
                        if (SettingPayload.Action == SettingPayload.ActionEnum.PROPOSE)
                        {
                            SettingProposal proposal;
                            using (MemoryStream stream = new MemoryStream(SettingPayload.Data))
                            {
                                proposal = Serializer.Deserialize<SettingProposal>(stream);
                            }
                            buf += "[Proposal]\n";
                            buf += $"   Setting : {proposal.Setting}\n";
                            buf += $"   Value   : {proposal.Value}\n";
                            buf += $"   Nonce   : {proposal.Nonce}\n";

                            proposal_key = proposal.Setting;
                        }

                        if (SettingPayload.Action == SettingPayload.ActionEnum.VOTE)
                        {
                            SettingVote vote;
                            using (MemoryStream stream = new MemoryStream(SettingPayload.Data))
                            {
                                vote = Serializer.Deserialize<SettingVote>(stream);
                            }
                            buf += "[Vote]\n";
                            buf += $"   Proposal Id : {vote.ProposalId}\n";
                            buf += $"   Vote        : {vote.Vote}\n";
                        }
                    }
                }
                else
                {
                    buf += "<Null Value>";
                }
                return buf;
            }
        }

        public string UnwrapPayload(byte[] payload)
        {
            if (payload == null) return "<Null Payload>";

            using (MemoryStream stream = new MemoryStream(payload))
            {
                SettingPayload = Serializer.Deserialize<SettingPayload>(stream);
            }

            return DisplayString;

        }

        public byte[] WrapPayload()
        {
            if (SettingPayload == null) throw new IOException("Please set 'SettingPayload' before wraping the object.");

            return SettingPayload.ToProtobufByteArray();
        }

        public string? AddressContext => proposal_key;

    }

}