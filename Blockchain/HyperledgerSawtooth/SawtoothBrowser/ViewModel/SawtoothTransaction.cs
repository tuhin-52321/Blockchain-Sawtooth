using Sawtooth.Sdk.Net.RESTApi.Payload.Json;
using SawtoothBrowser.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SawtoothBrowser.ViewModel
{

    public class SawtoothTransaction
    {
        public Transaction? Transaction { get; private set; }
        public BatchStatus? BatchStatus { get; private set; }

        public string?  TxnId { get; private set; }
        public string TxnIdShort => TxnId.Shorten(16);

        public string? Family => Transaction?.Header?.FamilyName;

        public string? Version => Transaction?.Header?.FamilyVersion;

        public string IsValid => IsTxnIdPresentIn(BatchStatus?.InvalidTransaction)?"No":"Yes";

        private bool IsTxnIdPresentIn(List<InvalidTransaction?>? invalidTransaction)
        {
            if (invalidTransaction == null) return false;
            foreach(var invalidTxn in invalidTransaction)
            {
                if(invalidTxn != null && invalidTxn.Id == TxnId)
                {
                    return true;
                }
            }
            return false;
        }

        public SawtoothTransaction(string? txnId, Transaction? txn, BatchStatus? status)
        {
            TxnId = txnId;
            Transaction = txn;
            BatchStatus = status;
        }

    }
}
