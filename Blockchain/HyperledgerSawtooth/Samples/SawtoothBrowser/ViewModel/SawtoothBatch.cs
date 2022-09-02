using SawtoothBrowser.Utils;
using Google.Protobuf;

namespace SawtoothBrowser.ViewModel
{

    public class SawtoothBatch
    {
        public Batch Batch { get; private set; }
        public BatchHeader Header { get; private set; }
        public ClientBatchStatus BatchStatus { get; private set; }

        public string  BatchId { get; private set; }
        public string BatchIdShort => BatchId.Shorten(16);

        public string Status => BatchStatus.Status.ToString();

        public int TxnCount => Batch.Transactions.Count;

        public int InvalidTxnCount => BatchStatus.InvalidTransactions.Count;

        public SawtoothBatch(string batchId, Batch batch, ClientBatchStatus status)
        {
            BatchId = batchId;
            Batch = batch;
            BatchStatus = status;
            Header = new BatchHeader();
            Header.MergeFrom(Batch.Header);
        }

    }
}
