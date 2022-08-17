using Sawtooth.Sdk.Net.Client;
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

        private List<Account> uncommitted_states = new List<Account>();

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

        public void Add(Account state)
        {
            uncommitted_states.Add(state);
        }

        internal async Task<Account?> FindAsync(uint? id)
        {
            List<Account> accounts = await FetchAllAsync();
            return accounts.Find(i => i.ID == id);
        }

        public async Task SaveChangesAsync()
        {
            //1. Create Batch of transactions using uncommitted_states
            List<Sawtooth.Sdk.Net.RESTApi.Payload.Protobuf.Transaction> txns = new List<Sawtooth.Sdk.Net.RESTApi.Payload.Protobuf.Transaction>();
            foreach (Account acc in uncommitted_states)
            {
                Sawtooth.Sdk.Net.RESTApi.Payload.Protobuf.Account account = new Sawtooth.Sdk.Net.RESTApi.Payload.Protobuf.Account();
                account.CustomerId = acc.ID;
                account.CustomerName = acc.Name;
                account.SavingsBalance = acc.SavingsBalance;
                account.CheckingBalance = acc.CheckingBalance;
                txns.Add(encoder.CreateTransaction(account.ToProtobufByteArray()));
            }
            if (txns.Count > 0)
            {
                var batch = encoder.CreateBatch(txns);

                //2. Post the Batch
                await client.PostBatchListAsync(encoder.Encode(batch));
            }


            //3. Clear the uncommitted_states
            uncommitted_states.Clear();

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
                        ID = smallbank.Account.CustomerId,
                        Name = smallbank.Account.CustomerName==null?"<No Name>": smallbank.Account.CustomerName,
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
