namespace Sawtooth.Sdk.Net.DataHandlers
{
    public class DefaultDataHandler : IDataHandler
    {
        public string UnwrapPayload(string? payload)
        {
            if (payload == null) return "<Null Value>";
            return "Raw data: " + payload;
        }
    }
}