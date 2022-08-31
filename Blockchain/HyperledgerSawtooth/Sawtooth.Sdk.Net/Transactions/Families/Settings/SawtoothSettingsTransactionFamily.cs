
using Sawtooth.Sdk.Net.Utils;
using System.Text;
using static Setting.Types;
using static SettingCandidate.Types;

namespace Sawtooth.Sdk.Net.Transactions
{
    public class SawtoothSettingsTransactionFamily : TransactionFamily<SawtoothSettingsState, SawtoothSettingsTransaction>
    {

        public SawtoothSettingsTransactionFamily() : base("sawtooth_settings", "1.0")
        {
        }
        public override string AddressPrefix => "000000";

        public override string AddressSuffix(string context)
        {
            string[] comps = context.Split(new char[] { '.' }, 4);

            string part1 = comps[0];
            string part2 = comps.Length > 1 ? comps[1] : "";
            string part3 = comps.Length > 2 ? comps[2] : "";
            string part4 = comps.Length > 3 ? comps[3] : "";

            return    Encoding.UTF8.GetBytes(part1).ToSha256().ToHexString().First(16)
                    + Encoding.UTF8.GetBytes(part2).ToSha256().ToHexString().First(16)
                    + Encoding.UTF8.GetBytes(part3).ToSha256().ToHexString().First(16)
                    + Encoding.UTF8.GetBytes(part4).ToSha256().ToHexString().First(16);

        }
    }



    public class SawtoothSettingsState : ProtobufPayload<Entry>, IState
    {
        public string DisplayString {
            get
            {
                string buffer = "[Protobuf Object: Setting]\n";
                if (Payload != null)
                {
                    buffer += "{\n";
                    if ("sawtooth.settings.vote.proposals".Equals(Payload.Key))
                    {
                        buffer += $"   {Payload.Key} = {{ \n";
                        if (Payload.Value != null)
                        {
                            byte[] setting_candidate_encoded = Convert.FromBase64String(Payload.Value);
                            SettingCandidate setting_candidate = setting_candidate_encoded.ToProtobufClass<SettingCandidate>();

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
                        buffer += $"                 }} \n";

                    }
                    else
                    {
                        buffer += $"   {Payload.Key} = {Payload.Value} \n";
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



        public string AddressContext => Payload?.Key != null ? Payload.Key: "no.key.set";

    }

    public class SawtoothSettingsTransaction : ProtobufPayload<SettingsPayload>, ITransaction
    {
        private string? proposal_key;

        public string DisplayString
        {
            get
            {
                string buf = "[Protobuf Object : SettingPayload]\n";

                if (Payload != null)
                {
                    buf += "Action : " + Payload.Action + "\n";

                    if (Payload.Data != null)
                    {
                        if (Payload.Action == SettingsPayload.Types.Action.Propose)
                        {
                            SettingProposal proposal = Payload.Data.ToProtobufClass<SettingProposal>();

                            buf += "[Proposal]\n";
                            buf += $"   Setting : {proposal.Setting}\n";
                            buf += $"   Value   : {proposal.Value}\n";
                            buf += $"   Nonce   : {proposal.Nonce}\n";

                            proposal_key = proposal.Setting;
                        }

                        if (Payload.Action == SettingsPayload.Types.Action.Vote)
                        {
                            SettingVote vote = Payload.Data.ToProtobufClass<SettingVote>();
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


        public string? AddressContext => proposal_key;

    }

}