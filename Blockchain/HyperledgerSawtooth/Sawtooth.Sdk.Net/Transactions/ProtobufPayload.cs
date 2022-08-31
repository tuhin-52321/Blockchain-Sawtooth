using Google.Protobuf;
using PeterO.Cbor;
using Sawtooth.Sdk.Net.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Message.Types;

namespace Sawtooth.Sdk.Net.Transactions
{
    public abstract class ProtobufPayload<T> : SerializablePayload where T:IMessage<T>, new()
    {
        public T Payload { get; set; } = new T();

        public override void Unwrap(byte[] payload)
        {
            Payload.MergeFrom(payload);
        }

        public override byte[] Wrap()
        {
            return Payload.ToByteArray();
        }
    }
}
