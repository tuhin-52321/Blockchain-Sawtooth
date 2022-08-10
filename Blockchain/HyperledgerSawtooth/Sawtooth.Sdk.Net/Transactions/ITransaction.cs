using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sawtooth.Sdk.Net.Transactions
{
    public interface ITransaction
    {
        public string UnwrapPayload(byte[] state_payload);

        public byte[] WrapPayload();

        public string DisplayString { get; }
    }
}
