﻿using Sawtooth.Sdk.Net.Client;
using Sawtooth.Sdk.Net.RESTApi.Client;
using Sawtooth.Sdk.Net.RESTApi.Payload;
using Sawtooth.Sdk.Net.Transactions;
using Sawtooth.Sdk.Net.Utils;
using Smallbank.Models;

namespace Smallbank.Blockchain
{
    public class BlockchainAccountSet
    {
        private SawtoothClient client;

        private TransactionFamily txnFamily;

        private Signer signer;

        private EncoderSettings settings;

        private Encoder encoder;

        public BlockchainAccountSet(string url)
        {
            txnFamily = TransactionFamilyFactory.GetTransactionFamily("smallbank", "1.0");

            signer = new Signer();

            settings = new EncoderSettings()
            {
                BatcherPublicKey = signer.GetPublicKey().ToHexString(),
                SignerPublickey = signer.GetPublicKey().ToHexString(),
                FamilyName = txnFamily.Name,
                FamilyVersion = txnFamily.Version
            };
            settings.Inputs.Add(txnFamily.AddressPrefix);
            settings.Outputs.Add(txnFamily.AddressPrefix);

            encoder = new Encoder(settings, signer.GetPrivateKey());

            client = new SawtoothClient(url);
        }

        public async Task<IList<Account>> ToListAsync()
        {
            return await FetchAllAsync();
        }



        public async Task<uint> GetLastIdAsync()
        {
            List<Account> accounts = await FetchAllAsync();
            if(accounts.Count > 0)
            {
                return accounts.Last().CustomerId;
            }
            return 0;
        }

        private async Task<string?> CallSmallBankTxn(SmallbankTransaction txn)
        {

            try
            {
                var response = await client.PostBatchListAsync(encoder.EncodeSingleTransaction(txnFamily.WrapPayload(txn)));

                if (response != null && response.Link != null)
                {
                    return await CheckStatus( response.Link);

                }
            }
            catch (Exception e)
            {
                return e.Message;
            }

            return null;
        }
        public async Task<string?> Add(Account acc)
        {
            try
            {
                //1. Create transaction payload

                var payload = SmallbankTransaction.CreateAccountTransaction(await GetLastIdAsync() + 1, acc.CustomerName, acc.SavingsBalance, acc.CheckingBalance);

                //2. Post the Batch

                string? message = await CallSmallBankTxn(payload);


                //3. return message
                return message;

            }catch(Exception e)
            {
                return e.Message;
            }
        }

        private async Task<string?> CheckStatus(string link)
        {
            int tries = 30; //Wil try 30 times => 30x1 = 30 seconds max
            while (tries > 0)
            {
                var statuses = await client.GetBatchStatusUsingLinkAsync(link);

                if (statuses != null)
                {
                    foreach (var status in statuses)
                    {
                        if (status.Id != null && status.Status != null)
                        {
                            if (status.InvalidTransaction.Count > 0)
                            {
                                return status.InvalidTransaction[0]?.Message;
                            }
                            if (status.Status == "PENDING")
                            {
                                Thread.Sleep(1000);
                                tries--;
                                continue;
                            }
                            if (status.Status == "COMMITTED")
                            {
                                return "Account Created.";
                            }
                            else
                            {
                                return "Account creation failes: status returned : " + status.Status;
                            }
                        }
                    }
                }
                else
                {
                    return "Account creation failed: no status found!";
                }
            }

            return "Account creation status check timed out.";
        }

        public async Task<bool> Any(Func<Account, bool> value)
        {
            List<Account> accounts = await FetchAllAsync();
            return accounts.Any(value);
        }

        public async Task<Account?> FirstOrDefaultAsync(Func<Account, bool> value)
        {
            List<Account> accounts = await FetchAllAsync();
            return accounts.FirstOrDefault(value);
        }

        public async Task<List<Account>> FetchAllAsync()
        {
            List<Account> accounts = new List<Account>(); ;
            var full = await client.GetStatesWithFilterAsync(txnFamily.AddressPrefix);
            foreach (var data in full.List)
            {
                SmallbankState smallbank = new SmallbankState();
                smallbank.UnwrapState(data?.Data);
                if (smallbank.Account != null)
                {
                    Account account = new Account
                    {
                        CustomerId = smallbank.Account.CustomerId,
                        CustomerName = smallbank.Account.CustomerName==null?"<No Name>": smallbank.Account.CustomerName,
                        SavingsBalance = smallbank.Account.SavingsBalance,
                        CheckingBalance = smallbank.Account.CheckingBalance
                    };
                    accounts.Add(account);
                }
            }
            return accounts;
        }
    }
}
