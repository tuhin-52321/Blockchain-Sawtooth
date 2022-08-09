using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sawtooth.Sdk.Net.Processor
{
    public interface ITransactionHandler
    {
        /**
   * Returns the transaction family's name.
   * @return the transaction family's name
   */
        public string TransactionFamilyName { get; }

        /**
         * Returns the transaction family's version.
         * @return the transaction family's version
         */
        public string Version { get; }

        /**
         * Returns the namespaces for this transaction handler.
         * @return the namespaces for this transaction handler
         */
        List<string> NameSpaces { get; }


    }
}
