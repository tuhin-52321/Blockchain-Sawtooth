using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sawtooth.Sdk.Net.Transactions
{
    public abstract class State
    {
        public IAddress Address { get; private set; }

        public State(IAddress address)
        {
            Address = address;
        }

        public abstract void UnwrapState(string? state_payload);

        public abstract void WrapState(out string? address, out string? state_payload);

        public abstract string DisplayString { get; }
    }
}
