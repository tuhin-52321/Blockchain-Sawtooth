using PeterO.Cbor;
using ProtoBuf;
using Sawtooth.Sdk.Net.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sawtooth.Sdk.Net.Transactions
{
    public abstract class ProtobufPayload<T> : SerializablePayload
    {

        public T? Payload { get; set; } = default;

        public override void Unwrap(byte[] payload)
        {
            using (MemoryStream stream = new MemoryStream(payload))
            {
                Payload = Serializer.Deserialize<T>(stream);
            }
        }

        public override byte[] Wrap()
        {
            return Payload.ToProtobufByteArray();
        }
    }
}
