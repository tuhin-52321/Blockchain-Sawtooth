using PeterO.Cbor;
using System.Text;
using Sawtooth.Sdk.Net.Utils;

namespace Sawtooth.Sdk.Net.Transactions.Families.IntKey
{
    public class IntKeyTransactionFamily : TransactionFamily<IntKeyState, IntKeyTransaction>
    {

        public IntKeyTransactionFamily() : base("intkey", "1.0")
        {
        }
        public override string AddressPrefix => Encoding.UTF8.GetBytes("intkey").ToSha512().ToHexString().First(6);

        public override string AddressSuffix(string context)
        {

            return Encoding.UTF8.GetBytes(context).ToSha512().ToHexString().Last(64);

        }

    }


    public class IntKeyState : CBORPayload, IState
    {
        public string Name { get; private set; } = "";
        public int Value { get; private set; } = 0;

        public string DisplayString => "[CBOR Object: Map]\n"
                 + $"    Name : {Name} \n"
                 + $"    Value : {Value} \n";


        public string AddressContext => Name;


        public override void Deserialize(CBORObject cbor)
        {
            var keys = cbor.Keys.GetEnumerator();
            if (keys.MoveNext())
            {
                Name = keys.Current.ToObject<string>();
                Value = cbor[Name].ToObject<int>();
            }
        }

        public override CBORObject Serialize()
        {
            return CBORObject.NewMap().Add(Name, Value);
        }
    }

    public class IntKeyTransaction : CBORPayload, ITransaction
    {

        public string? Name { get; set; }
        public string? Verb { get; set; }
        public int? Value { get; set; }

        public string DisplayString =>
             "[CBOR Object: Map]\n"
                 + $"    Name : {Name} \n"
                 + $"    Verb : {Verb} \n"
                 + $"    Value: {Value} \n";

        public override void Deserialize(CBORObject cbor)
        {
            Name = cbor["Name"].ToObject<string>();
            Verb = cbor["Verb"].ToObject<string>();

            if ("set".Equals(Verb))
                Value = cbor["Value"].ToObject<int>();
        }

        public override CBORObject Serialize()
        {
            if (Name == null) throw new IOException("Please set 'Name' before wraping the object.");
            if (Verb == null) throw new IOException("Please set 'Verb' before wraping the object.");
            if (Value == null) throw new IOException("Please set 'Value' before wraping the object.");

            CBORObject cbor = CBORObject.NewMap().Add("Name", Name).Add("Verb", Verb).Add("Value", Value);

            return cbor;
        }

        public string AddressContext => Name != null ? Name : "<NotSet>";

    }
}