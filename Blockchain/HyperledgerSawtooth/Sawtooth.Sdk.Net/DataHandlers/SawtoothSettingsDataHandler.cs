using PeterO.Cbor;
using ProtoBuf;

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

            return "protobuf base64 encoded string: " + payload;

        }
    }
}