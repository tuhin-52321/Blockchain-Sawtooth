using PeterO.Cbor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sawtooth.Sdk.Net.Transactions
{
    public abstract class CBORPayload : SerializablePayload
    {

        public abstract void Deserialize(CBORObject cbor);
        public abstract CBORObject Serialize();

        public override void Unwrap(byte[] payload)
        {
            Deserialize(CBORObject.DecodeFromBytes(payload));
        }

        public override byte[] Wrap()
        {
            return Serialize().EncodeToBytes();
        }
    }
}
