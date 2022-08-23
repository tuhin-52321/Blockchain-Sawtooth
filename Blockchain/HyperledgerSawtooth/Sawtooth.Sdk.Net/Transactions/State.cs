using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sawtooth.Sdk.Net.Transactions
{
    public interface IState : IDisplayable
    {
        public IAddress Address { get; }

        public string? ComposedAddress { get; }

    }
}
