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

        protected SerializablePayload()
        {
        }

        public static T CreateEmpty<T>() where T : SerializablePayload
        {
            T? payloadClass = Activator.CreateInstance(typeof(T)) as T;

            if (payloadClass == null) throw new TypeLoadException($"Could create instance of type '{typeof(T).FullName}'");

            return payloadClass;

        }
        public static T CreateFromPayloadData<T>(byte[] payload) where T : SerializablePayload
        {
            T payloadClass = CreateEmpty<T>();
            
            payloadClass.Unwrap(payload);

            return payloadClass;

        }

        public static T CreateFromPayloadData<T>(string base64EncodedPayload) where T : SerializablePayload
        {
            byte[] payload = Convert.FromBase64String(base64EncodedPayload);

            return CreateFromPayloadData<T>(payload);

        }

    }
}
