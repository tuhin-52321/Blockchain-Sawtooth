using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sawtooth.Sdk.Net.Transactions
{
    public abstract class SerializablePayload
    {
        public abstract void Unwrap(byte[] payload);
        public abstract byte[] Wrap();

        public string WrapToBase64Encoded()
        {
            return Convert.ToBase64String(Wrap());
        }

        public SerializablePayload()
        {
        }

        public static T CreateFromPayloadData<T>(byte[] payload) where T : SerializablePayload, new()
        {
            T payloadClass = new();
            payloadClass.Unwrap(payload);

            return payloadClass;

        }

        public static T CreateFromPayloadData<T>(string base64EncodedPayload) where T : SerializablePayload, new()
        {
            byte[] payload = Convert.FromBase64String(base64EncodedPayload);

            return CreateFromPayloadData<T>(payload);

        }
    }
}
