using System.Text;

namespace Sawtooth.Sdk.Net.Transactions
{
    public abstract class CSVStringPayload : SerializablePayload
    {


        public abstract void Deserialize(string[] values);
        public abstract string Serialize();

        public override void Unwrap(byte[] payload)
        {
            string data = Encoding.UTF8.GetString(payload);

            Deserialize(data.Split(","));
        }

        public override byte[] Wrap()
        {
            return Encoding.UTF8.GetBytes(Serialize());
        }
    }
}
