using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sawtooth.Sdk.Net.Transactions
{
    public abstract class TransactionFamily
    {
        public State State { get; private set; }
        public ITransaction Transaction { get; private set; }

        public string Name { get; private set; }
       

        public TransactionFamily(string name)
        {
            this.Name = name;
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
