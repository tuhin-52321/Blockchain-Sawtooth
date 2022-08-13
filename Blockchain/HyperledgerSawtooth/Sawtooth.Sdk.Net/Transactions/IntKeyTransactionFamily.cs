using PeterO.Cbor;
using System.Text;
using Sawtooth.Sdk.Net.Utils;

namespace Sawtooth.Sdk.Net.Transactions
{
    public class IntKeyTransactionFamily : TransactionFamily
    {

        public IntKeyTransactionFamily(string version): base("intkey", version)
        {
            if(version == "1.0")
            {
                SetHandlers(new IntKeyState(), new IntKeyTransaction());
            }
        }

    }

    public class IntKeyAddress : IAddress
    {
        public string Prefix => Encoding.UTF8.GetBytes("intkey").ToSha512().ToHexString().First(6);

        public string ComposeAddress(string context)
        {
            return Prefix + Encoding.UTF8.GetBytes(context).ToSha512().ToHexString().Last(64);

        }

    }

    public class IntKeyState : State
    {
        public IntKeyState() : base(new IntKeyAddress())
        {
        }

        public string Name { get; private set; } = "";
        public int Value { get; private set; } = 0;

        public override string DisplayString => "[CBOR Object: Map]\n"
                 + $"    Name : {Name} \n"
                 + $"    Value : {Value} \n";


        public override void UnwrapState(string? state_payload)
        {
            if (state_payload == null) return;

            byte[] paylod_raw = Convert.FromBase64String(state_payload);

            CBORObject cbor = CBORObject.DecodeFromBytes(paylod_raw);

            var keys = cbor.Keys.GetEnumerator();
            if (keys.MoveNext())
            {
                Name = keys.Current.ToObject<string>();
                Value = cbor[Name].ToObject<int>();
            }
        }

        public override void WrapState(out string? address, out string? state_payload)
        {
            address = Address.ComposeAddress(Name);
            state_payload = Convert.ToBase64String(CBORObject.NewMap().Add(Name, Value).EncodeToBytes());
        }
    }

    public class IntKeyTransaction : ITransaction
    {
        public string? Name { get; set; }
        public string? Verb { get; set; }
        public int? Value { get; set; }

        public string DisplayString =>
             "[CBOR Object: Map]\n"
                 + $"    Name : {Name} \n"
                 + $"    Verb : {Verb} \n"
                 + $"    Value: {Value} \n";

        public string UnwrapPayload(byte[] payload)
        {
            if (payload == null) return "<Null Payload>";

            CBORObject cbor = CBORObject.DecodeFromBytes(payload);

            Name = cbor["Name"].ToObject<string>();
            Verb = cbor["Verb"].ToObject<string>();

            if("set".Equals(Verb))
                Value = cbor["Value"].ToObject<int>();

            return DisplayString;


        }

        public byte[] WrapPayload()
        {
            if (Name == null) throw new IOException("Please set 'Name' before wraping the object.");
            if (Verb == null) throw new IOException("Please set 'Verb' before wraping the object.");
            if (Value == null) throw new IOException("Please set 'Value' before wraping the object.");

            CBORObject cbor = CBORObject.NewMap().Add("Name", Name).Add("Verb", Verb).Add("Value", Value);

            return cbor.EncodeToBytes();
        }

        public string? AddressContext => Name;

    }
}