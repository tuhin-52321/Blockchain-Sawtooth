using ProtoBuf;
using Sawtooth.Sdk.Net.RESTApi.Payload.Protobuf;
using System.Text;

namespace Sawtooth.Sdk.Net.DataHandlers
{
    internal class SawtoothSettingsDataHandler : IDataHandler
    {
        private string? version;

        public SawtoothSettingsDataHandler(string? version)
        {
            this.version = version;
        }

        public string UnwrapPayload(string? payload)
        {
            //TODO: Version specific unwrapping

            if (payload == null) return "<Null Value>";

            //TODO: decode protobuf

            byte[] paylod_raw = Convert.FromBase64String(payload);

            SettingPayload setting_payload;

            using (MemoryStream stream = new MemoryStream(paylod_raw))
            {
                setting_payload = Serializer.Deserialize<SettingPayload>(stream);
            }
            string buf = "[Protobuf Object : SettingPayload]\n"
                 + "Action : " + setting_payload.Action + "\n";



            if (setting_payload.Data != null)
            {
                if (setting_payload.Action == SettingPayload.ActionEnum.PROPOSE)
                {
                    SettingProposal proposal;
                    using (MemoryStream stream = new MemoryStream(setting_payload.Data))
                    {
                        proposal = Serializer.Deserialize<SettingProposal>(stream);
                    }
                    buf += "[Proposal]\n";
                    buf += $"   Setting : {proposal.Setting}\n";
                    buf += $"   Value   : {proposal.Value}\n";
                    buf += $"   Nonce   : {proposal.Nonce}\n";
                }
                
                if (setting_payload.Action == SettingPayload.ActionEnum.VOTE)
                {
                    SettingVote vote;
                    using (MemoryStream stream = new MemoryStream(setting_payload.Data))
                    {
                        vote = Serializer.Deserialize<SettingVote>(stream);
                    }
                    buf += "[Vote]\n";
                    buf += $"   Proposal Id : {vote.ProposalId}\n";
                    buf += $"   Vote        : {vote.Vote}\n";
                }
            }

            return buf;


        }
    }
}