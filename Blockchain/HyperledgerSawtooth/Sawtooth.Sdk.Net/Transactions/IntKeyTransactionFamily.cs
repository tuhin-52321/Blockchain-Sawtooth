using PeterO.Cbor;
using System.Text;
using Sawtooth.Sdk.Net.Utils;

namespace Sawtooth.Sdk.Net.Transactions
{
    public class IntKeyTransactionFamily : TransactionFamily
    {

        public IntKeyTransactionFamily(string? version): base("intkey")
        {
            if(version == "1.0")
            {
                SetHandlers(new IntKeyState(), new IntKeyTransaction());
            }
        }

    }

    public class IntKeyAddress : IAddress
    {
        public string Prefix => Encoding.UTF8.GetBytes("intkey").ToSha256().ToHexString().First(6);

        public string ComposeAddress(string context)
        {
            return Prefix + Encoding.UTF8.GetBytes(context).ToSha256().ToHexString().Last(64);

        }

    }

    public class IntKeyState : State
    {
        public IntKeyState() : base(new IntKeyAddress())
        {
        }

        public string? Name {get; private set;}
        public int? Value { get; private set; }

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
            address = null;
            state_payload = null;
            if (Name != null)
            {
                address = Address.ComposeAddress(Name);
                if (Value != null)
                {
                    state_payload = Convert.ToBase64String(CBORObject.NewMap().Add(Name, Value).EncodeToBytes());
                }

            }
        }
    }

    public class IntKeyTransaction : ITransaction
    {
        public string? Name { get; private set; }
        public string? Verb { get; private set; }
        public int? Value { get; private set; }

        public string DisplayString =>
             "[CBOR Object: Map]\n"
                 + $"    Name : {Name} \n"
                 + $"    Verb : {Verb} \n"
                 + $"    Value: {Value} \n";

        public string? UnwrapPayload(string? state_payload)
        {
            if (state_payload == null) return null;

            byte[] paylod_raw = Convert.FromBase64String(state_payload);

            CBORObject cbor = CBORObject.DecodeFromBytes(paylod_raw);

            Name = cbor["Name"].ToObject<string>();
            Verb = cbor["Verb"].ToObject<string>();
            Value = cbor["Value"].ToObject<int>();

            return DisplayString;


        }

        public string? WrapPayload()
        {
            if (Name == null || Verb == null || Value == null) return null;

            CBORObject cbor = CBORObject.NewMap().Add("Name", Name).Add("Verb", Verb).Add("Value", Value);

            return Convert.ToBase64String(cbor.EncodeToBytes());
        }
    }
}