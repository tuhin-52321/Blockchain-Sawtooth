using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sawtooth.Sdk.Net.Transactions
{
    public interface IAddress
    {
        string ComposeAddress(string context);

        public string Prefix { get; }
    }
}
