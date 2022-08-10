namespace Sawtooth.Sdk.Net.Transactions
{
    public class DefaultTransactionFamily : TransactionFamily
    {
        public DefaultTransactionFamily() : base("default", "1.0")
        {
        }

    }

    public class DefaultAddress : IAddress
    {

        public string Prefix => throw new NotImplementedException();

        public string ComposeAddress(string context)
        {
            throw new NotImplementedException();
        }
    }

    public class DefaultState : State
    {
        private string? raw_data;

        public DefaultState() : base(new DefaultAddress())
        {

        }

        public override void UnwrapState(string? state_payload)
        {
            if (state_payload == null) raw_data = null;
            raw_data = state_payload;
        }

        public override void WrapState(out string? address, out string? state_payload)
        {
            address = Address.ComposeAddress("default");
            state_payload = raw_data;
        }

        public override string DisplayString =>  "[Raw data: ]\n" + (raw_data!=null?raw_data:"<Null Value>");

    }

    public class DefaultTransaction : ITransaction
    {
        private string? raw_data;

        public string UnwrapPayload(byte[] payload)
        {
            if (payload == null) raw_data = null;
            else
                raw_data = Convert.ToBase64String(payload);

            return DisplayString;
        }
        public byte[] WrapPayload()
        {
            if (raw_data == null) throw new IOException("Please set raw data first.");
            return Convert.FromBase64String(raw_data);
        }

        public string DisplayString => "[Raw data: ]\n" + (raw_data != null ? raw_data : "<Null Value>");
    }
}