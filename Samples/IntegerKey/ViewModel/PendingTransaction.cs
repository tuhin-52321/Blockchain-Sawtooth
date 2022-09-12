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
        public uint? Value => Transaction.Value;
        public ClientBatchStatus.Types.Status Status { get; set; }

        public string? Message { get; set; }

        public IntKeyTransaction Transaction;

        public PendingTransaction(string id, IntKeyTransaction transaction, ClientBatchStatus.Types.Status status)
        {
            TxnId = id;
            Status = status;
            Transaction = transaction;
        }
    }
}
