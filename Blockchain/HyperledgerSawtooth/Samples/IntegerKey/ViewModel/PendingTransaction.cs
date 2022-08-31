using Sawtooth.Sdk.Net.RESTApi.Payload.Json;
using Sawtooth.Sdk.Net.Transactions.Families.IntKey;
using Sawtooth.Sdk.Net.Utils;

namespace IntegerKeys.ViewModel
{

    public class PendingTransaction
    {

        public string TxnId { get; private set; }

        public string ShortId => TxnId.First(8);
        public string? Name => Transaction.Name;
        public string? Verb => Transaction.Verb;
        public int? Value => Transaction.Value;
        public string Status { get; set; }

        public string? Message { get; set; }

        public IntKeyTransaction Transaction;

        public PendingTransaction(string id, IntKeyTransaction transaction, string status)
        {
            TxnId = id;
            Status = status;
            Transaction = transaction;
        }
    }
}
