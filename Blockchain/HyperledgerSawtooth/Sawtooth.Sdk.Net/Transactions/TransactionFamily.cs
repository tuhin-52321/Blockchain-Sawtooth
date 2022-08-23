using Sawtooth.Sdk.Net.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sawtooth.Sdk.Net.Transactions
{
    public abstract class TransactionFamily<STATE,TXN> where STATE:SerializablePayload,new() where TXN:SerializablePayload,new()
    {
        public string Name { get; private set; }
        public string Version { get; private set; }

        public abstract string AddressPrefix { get; }
        public abstract string AddressSuffix(string context);

        public string Address(string context)
        {
            return AddressPrefix + AddressSuffix(context);       
        }

        public TXN UnwrapTxnPayload(byte[] payload) => SerializablePayload.CreateFromPayloadData<TXN>(payload);
        public STATE UnwrapStatePayload(byte[] payload) => SerializablePayload.CreateFromPayloadData<STATE>(payload);

        public TXN UnwrapTxnPayload(string base64EncodedString) => SerializablePayload.CreateFromPayloadData<TXN>(base64EncodedString);
        public STATE UnwrapStatePayload(string base64EncodedString) => SerializablePayload.CreateFromPayloadData<STATE>(base64EncodedString);

        public byte[] WrapTxnPayload(TXN payload) => payload.Wrap();
        public byte[] WrapStatePayload(STATE payload) => payload.Wrap();

        public string WrapTxnPayloadToBase64String(TXN payload) => WrapTxnPayload(payload).ToBase64String();
        public string WrapStatePayloadToBase64String(STATE payload) => WrapStatePayload(payload).ToBase64String();
        

        public TransactionFamily(string name, string version)
        {
            Name = name;
            Version = version;
        }

    }
}
