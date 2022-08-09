using Sawtooth.Sdk.Net.RESTApi.Payload.Json;


namespace IntegerKeys.ViewModel
{

    public class CommittedKeys
    {
        public IntKeyState State { get; private set; }

        public string? Name => State.Name;
        public string? Value => State.Value;

        public CommittedKeys(IntKeyState state)
        {
            State = state;
        }

    }
}
