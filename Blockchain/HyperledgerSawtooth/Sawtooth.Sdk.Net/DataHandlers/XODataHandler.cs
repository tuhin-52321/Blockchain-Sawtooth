using System.Text;

namespace Sawtooth.Sdk.Net.DataHandlers
{
    internal class XODataHandler : IDataHandler
    {
        private string? version;

        public XODataHandler(string? version)
        {
            this.version = version;
        }

        public string UnwrapPayload(string? payload)
        {
            //TODO: Version specific unwrapping

            if (payload == null) return "<Null Value>";

            byte[] paylod_raw = Convert.FromBase64String(payload);

            string data = Encoding.UTF8.GetString(paylod_raw);

            string[] values = data.Split(",");

            return $"Game Name   : {values[0]} \n"
                 + $"Action      : {values[1]} \n"
                 + $"Space       : {values[2]} \n"; 
        }
    }
}