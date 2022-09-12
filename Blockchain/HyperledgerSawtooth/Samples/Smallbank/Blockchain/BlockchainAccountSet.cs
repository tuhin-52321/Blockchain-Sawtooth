using Google.Protobuf.Collections;
using Sawtooth.Sdk.Net.Client;
using Sawtooth.Sdk.Net.Transactions.Families.Smallbank;
using Sawtooth.Sdk.Net.Utils;
using Smallbank.Models;
using System.Security.Policy;

namespace Smallbank.Blockchain
{
    public class BlockchainAccountSet
    {
        private ValidatorClient client;

        private SmallbankTransactionFamily txnFamily;

        private Signer signer;

        private EncoderSettings settings;

        private Encoder encoder;

        public BlockchainAccountSet(string url)
        {
            txnFamily = new SmallbankTransactionFamily();

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

            CreateClient(url);
        }

        private void CreateClient(string url)
        {
            if (client != null) client.Dispose();
            client = ValidatorClient.Create(url, () => CreateClient(url));
        }

        internal void Close()
        {
            client.Dispose();
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
                var batchIds = await client.PostBatchListAsync(encoder.EncodeSingleTransaction(txnFamily.WrapTxnPayload(txn)));

                return await CheckStatus(batchIds);
            }
            catch (Exception e)
            {
                return e.Message;
            }

        }

        public async Task<string?> DepositCheck(VMTransaction depositCheck)
        {
            try
            {
                //1. Create transaction payload

                var payload = SmallbankTransaction.CreateDepositCheckTransaction(depositCheck.CustomerId, depositCheck.CheckAmount);

                //2. Post the Batch

                string? message = await CallSmallBankTxn(payload);


                //3. return message
                return message;

            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public async Task<string?> DepositCash(VMTransaction depositCash)
        {
            try
            {
                //1. Create transaction payload

                var payload = SmallbankTransaction.CreateDepositCashTransaction(depositCash.CustomerId, depositCash.CashAmount);

                //2. Post the Batch

                string? message = await CallSmallBankTxn(payload);


                //3. return message
                return message;

            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
        
        public async Task<string?> SendMoney(VMTransaction send)
        {
            try
            {
                //1. Create transaction payload

                var payload = SmallbankTransaction.CreateSendPaymentTransaction(send.CustomerId, send.DestCustomerId, send.CheckAmount);

                //2. Post the Batch

                string? message = await CallSmallBankTxn(payload);


                //3. return message
                return message;

            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public async Task<string?> Amalgamate(VMTransaction send)
        {
            try
            {
                //1. Create transaction payload

                var payload = SmallbankTransaction.CreateAmalgamateTransaction(send.CustomerId, send.DestCustomerId);

                //2. Post the Batch

                string? message = await CallSmallBankTxn(payload);


                //3. return message
                return message;

            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public async Task<string?> WriteCheck(VMTransaction writeCheck)
        {
            try
            {
                //1. Create transaction payload

                var payload = SmallbankTransaction.CreateWriteCheckTransaction(writeCheck.CustomerId, writeCheck.CheckAmount);

                //2. Post the Batch

                string? message = await CallSmallBankTxn(payload);


                //3. return message
                return message;

            }
            catch (Exception e)
            {
                return e.Message;
            }
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

        private async Task<string?> CheckStatus(RepeatedField<string> batchIds)
        {
            int tries = 30; //Wil try 30 times => 30x1 = 30 seconds max
            while (tries > 0)
            {
                var statuses = await client.GetBatchStatusesAsync(batchIds);

                if (statuses != null)
                {
                    foreach (var status in statuses)
                    {
                        if (status.BatchId != null)
                        {
                            if (status.InvalidTransactions.Count > 0)
                            {
                                return status.InvalidTransactions[0]?.Message;
                            }
                            if (status.Status == ClientBatchStatus.Types.Status.Pending)
                            {
                                Thread.Sleep(1000);
                                tries--;
                                continue;
                            }
                            if (status.Status == ClientBatchStatus.Types.Status.Committed)
                            {
                                return "Transaction committed.";
                            }
                            else
                            {
                                return "Transaction failed: status returned : " + status.Status;
                            }
                        }
                    }
                }
                else
                {
                    return "Transaction failed: no status found!";
                }
            }

            return "Transaction status check timed out.";
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
            var full = await client.GetAllStatesWithFilterAsync(txnFamily.AddressPrefix);
            foreach (var data in full.List)
            {
                if (data?.Data != null)
                {
                    SmallbankState smallbank = txnFamily.UnwrapStatePayload(data.Data.ToByteArray());
                    if (smallbank.Payload != null)
                    {
                        Account account = new Account
                        {
                            CustomerId = smallbank.Payload.CustomerId,
                            CustomerName = smallbank.Payload.CustomerName == null ? "<No Name>" : smallbank.Payload.CustomerName,
                            SavingsBalance = smallbank.Payload.SavingsBalance,
                            CheckingBalance = smallbank.Payload.CheckingBalance
                        };
                        accounts.Add(account);
                    }
                }
            }
            return accounts;
        }
    }
}
