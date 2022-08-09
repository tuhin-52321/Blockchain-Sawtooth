using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sawtooth.Sdk.Net.Transactions
{
    public interface ITransaction
    {
        public string? UnwrapPayload(string? state_payload);

        public string? WrapPayload();

        public string DisplayString { get; }
    }
}
