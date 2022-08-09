using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sawtooth.Sdk.Net.Processor
{
    public interface IContext
    {
        /**
  * Make a Get request on a specific context specified by contextId.
  * @param addresses a collection of address Strings
  * @return Map where the keys are addresses, values Bytestring
  * @throws InternalError               something went wrong processing
  *                                     transaction
  * @throws InvalidTransactionException an invalid transaction was encountered
  */
        Dictionary<string, byte[]> GetState(params string[] address) ;

        /**
         * Make a Set request on a specific context specified by contextId.
         * @param addressValuePairs A collection of Map.Entry's
         * @return addressesThatWereSet, A collection of address Strings that were set
         * @throws InternalError               something went wrong processing
         *                                     transaction
         * @throws InvalidTransactionException an invalid transaction was encountered
         */
        List<string> SetState(string address, byte[] state);

        /**
         * Make a Delete request on a specific context specified by contextId.
         * @param addresses a collection of address Strings
         * @return addressesThatWereDeleted, A collection of address Strings that were
         *         deleted
         * @throws InternalError               something went wrong processing
         *                                     transaction
         * @throws InvalidTransactionException an invalid transaction was encountered
         */
        List<string> DeleteState(params string[] address);

        /**
         * Add a blob to the execution result for this transaction.
         * @param data The data to add
         * @throws InternalError something went wrong processing transaction
         */
        void AddReceiptData(byte[] data);

        /**
         * Adds a new event to the execution result for this transaction.
         * @param eventType  This is used to subscribe to events. It should be globally
         *                   unique and describe what, in general, has occurred.
         * @param attributes Additional information about the event that is transparent
         *                   to the validator. Attributes can be used by subscribers to
         *                   filter the type of events they receive.
         * @param data       Additional information about the event that is opaque to
         *                   the validator, or null
         * @throws InternalError something went wrong processing transaction
         */
        void AddEvent(string eventType, (string Name, string Value) attributes, byte[] data);
    }
}
