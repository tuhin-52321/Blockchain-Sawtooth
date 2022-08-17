using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Smallbank.Blockchain;
using Smallbank.Models;

namespace Smallbank.Data
{
    public class SmallbankContext 
    {
        public SmallbankContext (string url) 
        {
            Account = new BlockchainAccountSet(url);
        }

        public BlockchainAccountSet Account;
    }
}
