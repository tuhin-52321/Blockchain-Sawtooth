using Sawtooth.Sdk.Net.RESTApi.Payload.Json;
using SawtoothBrowser.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SawtoothBrowser.ViewModel
{

    public class SawtoothBatch
    {
        public Batch? Batch { get; private set; }
        public BatchStatus? BatchStatus { get; private set; }

        public string?  BatchId { get; private set; }
        public string BatchIdShort => BatchId.Shorten(16);

        public string? Status => BatchStatus?.Status;

        public int? TxnCount => Batch?.Transactions.Count;

        public int? InvalidTxnCount => BatchStatus?.InvalidTransaction.Count;

        public SawtoothBatch(string? batchId, Batch? batch, BatchStatus? status)
        {
            BatchId = batchId;
            Batch = batch;
            BatchStatus = status;
        }

    }
}
