using Sawtooth.Sdk.Net.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sawtooth.Sdk.Net.Transactions
{
    public abstract class TransactionFamily
    {
        private State State { get; set; }
        private ITransaction Transaction { get; set; }

        public string Name { get; private set; }
        public string Version { get; private set; }

        public string AddressPrefix => State.Address.Prefix;

        public string? Address(string? context)
        {
           return (context != null) ? State.Address.ComposeAddress(context) : null;
        }

        public string UnwrapPayload(string payload) => Transaction.UnwrapPayload(payload.FromBase64String());

        public TransactionFamily(string name, string version)
        {
            this.Name = name;
            this.Version = version;
            this.State = new DefaultState();
            this.Transaction = new DefaultTransaction();
        }

        public void SetHandlers(State state, ITransaction transaction)
        {
            this.State = state;
            this.Transaction = transaction;
        }
    }
}
