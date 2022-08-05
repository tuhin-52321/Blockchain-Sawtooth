using PeterO.Cbor;

namespace Sawtooth.Sdk.Net.DataHandlers
{
    internal class IntKeyDataHandler : IDataHandler
    {
        private string? version;

        public IntKeyDataHandler(string? version)
        {
            this.version = version;
        }

        public string UnwrapPayload(string? payload)
        {
            //TODO: Version specific unwrapping

            if (payload == null) return "<Null Value>";

            byte[] paylod_raw = Convert.FromBase64String(payload);

            CBORObject cbor = CBORObject.DecodeFromBytes(paylod_raw);

            string name = cbor["Name"].ToObject<string>();
            string verb = cbor["Verb"].ToObject<string>();
            int value = cbor["Value"].ToObject<int>();

            return "[CBOR Object: Map]\n" 
                 + $"    Name : {name} \n"
                 + $"    Verb : {verb} \n"
                 + $"    Value: {value} \n";

        }
    }
}