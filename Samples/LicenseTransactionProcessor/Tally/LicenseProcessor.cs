using Sawtooth.Sdk.Net.Processor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LicenseTransactionProcessor.Tally
{
    public class LicenseProcessor : TransactionProcessor
    {
        public LicenseProcessor(string address) : base(address)
        {
            AddHandler(new LicenseHandler());
        }
    }
}
