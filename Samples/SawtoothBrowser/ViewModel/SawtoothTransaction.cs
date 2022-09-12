using Google.Protobuf.Collections;
using SawtoothBrowser.Utils;
using Google.Protobuf;
using static ClientBatchStatus.Types;

namespace SawtoothBrowser.ViewModel
{

    public class SawtoothTransaction
    {
        public Transaction Transaction { get; private set; }
        public TransactionHeader Header { get; private set; }
        public ClientBatchStatus BatchStatus { get; private set; }
        public string TxnId { get; private set; }
        public string TxnIdShort => TxnId.Shorten(16);

        public string Family => Header.FamilyName;

        public string Version => Header.FamilyVersion;

        public string IsValid => IsTxnIdPresentIn(BatchStatus.InvalidTransactions)?"No":"Yes";

        private bool IsTxnIdPresentIn(RepeatedField<InvalidTransaction> invalidTransaction)
        {
            if (invalidTransaction == null) return false;
            foreach(var invalidTxn in invalidTransaction)
            {
                if(invalidTxn != null && invalidTxn.TransactionId == TxnId)
                {
                    return true;
                }
            }
            return false;
        }

        public SawtoothTransaction(string txnId, Transaction txn, ClientBatchStatus status)
        {
            TxnId = txnId;
            Transaction = txn;
            BatchStatus = status;
            Header = new TransactionHeader();
            Header.MergeFrom(txn.Header);
        }

    }
}
