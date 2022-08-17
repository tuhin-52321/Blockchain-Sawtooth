﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sawtooth.Sdk.Net.Transactions
{
    public static class TransactionFamilyFactory
    {
        public static TransactionFamily GetTransactionFamily(string? family, string? version)
        {
            if (family != null && version != null)
            {
                if (family.Equals("intkey"))
                {
                    return new IntKeyTransactionFamily(version);
                }
                if (family.Equals("sawtooth_settings"))
                {
                    return new SawtoothSettingsTransactionFamily(version);
                }
                if (family.Equals("xo"))
                {
                    return new XOTransactionFamily(version);
                }
                if (family.Equals("smallbank"))
                {
                    return new SmallbankTransactionFamily(version);
                }

            }
            return new DefaultTransactionFamily();
        }
    }
}
