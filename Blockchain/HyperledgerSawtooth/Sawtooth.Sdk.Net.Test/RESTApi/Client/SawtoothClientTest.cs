using PeterO.Cbor;
using Sawtooth.Sdk.Net.Client;
using Sawtooth.Sdk.Net.RESTApi.Payload;
using Sawtooth.Sdk.Net.Transactions;
using Sawtooth.Sdk.Net.Utils;
using System.Text.Json;

namespace Sawtooth.Sdk.Net.RESTApi.Client.Tests
{
    [TestClass()]
    public class SawtoothClientTest
    {
        SawtoothClient? client;

        [TestInitialize()]
        public void StartUp()
        {
            client = new SawtoothClient("http://localhost:8008");
        }

        [TestCleanup()]
        public void CleanUp()
        {
            client = null;
        }

        private string ToJson<T>(T? json)
        {
            if (json != null)
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                return JsonSerializer.Serialize(json, options);
            }

            return "<Null>";
        }

        private async Task GetListAllTestAsync<T>(Func<Task<FullList<T>>> method)
        {
            foreach (T? batch in (await method()).List)
            {
                Console.WriteLine(ToJson<T>(batch));
            }
        }

        private async Task GetListFirstPageTestAsync<T>(Func<string?,Task<PageOf<T>>> method)
        {
            foreach (T? batch in (await method(null)).List)
            {
                Console.WriteLine(ToJson<T>(batch));
            }
        }

        private async Task GetListAPageTestAsync<T>(Func<string?, Task<List<T>>> method, string start)
        {
            foreach (T batch in await method(start))
            {
                Console.WriteLine(ToJson<T>(batch));
            }
        }

        private async Task GetSingleTestWithParamAsync<T>(Func<string,Task<T>> method, string param)
        {
            T response = await method(param);
            Console.WriteLine(ToJson<T>(response));
        }

        private async Task GetSingleTestWithMultiParamAsync<T>(Func<string[], Task<T>> method, params string[] param)
        {
            T response = await method(param);
            Console.WriteLine(ToJson<T>(response));
        }

        private async Task GetSingleTestAsync<T>(Func<Task<T>> method)
        {
            T response = await method();
            Console.WriteLine(ToJson<T>(response));
        }

        [TestMethod("List all batches")]
        public async Task GetAllBatchesTestAsync()
        {
            Assert.IsNotNull(client);

            await GetListAllTestAsync(client.GetBatchesAsync);
        }

        [TestMethod("List first page batches")]
        public async Task GetFirstPageOfBatchesTestAsync()
        {
            Assert.IsNotNull(client);

            await GetListFirstPageTestAsync(client.GetBatchesAsync);
        }

        [TestMethod("List all blocks")]
        public async Task GetAllBlocksTestAsync()
        {
            Assert.IsNotNull(client);

            await GetListAllTestAsync(client.GetBlocksAsync);
        }

        [TestMethod("List first page blocks")]
        public async Task GetFirstPageOfBlocksTestAsync()
        {
            Assert.IsNotNull(client);

            await GetListFirstPageTestAsync(client.GetBlocksAsync);
        }



        [TestMethod("List all transactions")]
        public async Task GetAllTransactionsTestAsync()
        {
            Assert.IsNotNull(client);

            await GetListAllTestAsync(client.GetTransactionsAsync);
        }

        [TestMethod("List first page transctions")]
        public async Task GetFirstPageOfTransactionsTestAsync()
        {
            Assert.IsNotNull(client);

            await GetListFirstPageTestAsync(client.GetTransactionsAsync);
        }


        [TestMethod("List all states")]
        public async Task GetStatesTestAsync()
        {
            Assert.IsNotNull(client);

            await GetListAllTestAsync(client.GetStatesAsync);
        }

        [TestMethod("List first page of states")]
        public async Task GetFirstPageOfStatesTestAsync()
        {
            Assert.IsNotNull(client);

            await GetListFirstPageTestAsync(client.GetStatesAsync);
        }


        [TestMethod("Get status")]
        public async Task GetStatusTestAsync()
        {
            Assert.IsNotNull(client);

            await GetSingleTestAsync(client.GetStatusAsync);
        }

        [TestMethod("Get peers")]
        public async Task GetPeersTestAsync()
        {
            Assert.IsNotNull(client);

            await GetSingleTestAsync(client.GetPeersAsync);
        }

        [TestMethod("Get a particular block")]
        [DataRow("595155177083ba08566818df9514dcf019f2d7c443b3f1b269ea95ab734160ad532af0942db0db13068ec27d1d15bc3a3a189052a0d2f7f3591987b3af415038")]
        public async Task GetBlockTestAsync(string block_id)
        {
            Assert.IsNotNull(client);

            await GetSingleTestWithParamAsync(client.GetBlockAsync, block_id);
        }

        [TestMethod("Get a particular batch")]
        [DataRow("dd43e16e404ff84fdf628eb3f4b3ab8a6dbaaf0c9c10d8f25a3fcaa72c668ff0274ea6f016bfb36c80c71ca931059b9f25379dc76881c28f80d5c58e4a7d63d4")]
        public async Task GetBatchTestAsync(string batch_id)
        {
            Assert.IsNotNull(client);

            await GetSingleTestWithParamAsync(client.GetBatchAsync, batch_id);
        }

        [TestMethod("Get a particular batch")]
        [DataRow("dd43e16e404ff84fdf628eb3f4b3ab8a6dbaaf0c9c10d8f25a3fcaa72c668ff0274ea6f016bfb36c80c71ca931059b9f25379dc76881c28f80d5c58e4a7d63d3")]
        public async Task GetBatchTestNoDataFoundAsync(string batch_id)
        {
            Assert.IsNotNull(client);
            try
            {
                await GetSingleTestWithParamAsync(client.GetBatchAsync, batch_id);

                Assert.Fail("Should get no data found");
            }catch(HttpException e)
            {
                Console.WriteLine(e.Title + ": " + e.Message);
                Assert.IsTrue(e.Code == 71, "Exception did not return no data found - returned = " + e.Code);
            }
        }


        [TestMethod("Get committed statuses for a set of batches")]
        [DataRow("dd43e16e404ff84fdf628eb3f4b3ab8a6dbaaf0c9c10d8f25a3fcaa72c668ff0274ea6f016bfb36c80c71ca931059b9f25379dc76881c28f80d5c58e4a7d63d4")]
        public async Task GetBatchStatusTestAsync(string batch_id)
        {
            Assert.IsNotNull(client);

            await GetSingleTestWithMultiParamAsync(client.GetBatchStatusesAsync, batch_id);
        }

        [TestMethod("Get the receipts for a set of transactions")]
        [DataRow("c1ae814d87cb5f40a54573e4c894a43e65da0763ca47cc9602cb94785bf130c65d4f87e8c4fea0421fc561d362f2babed634c96e8e34ca38bfe2c9afe21eaa6d")]
        public async Task GetTrasnactionsReceiptsTestAsync(string transaction_id)
        {
            Assert.IsNotNull(client);

            await GetSingleTestWithMultiParamAsync(client.GetTransactionReceiptsAsync, transaction_id);
        }

        [TestMethod("Get a particular state of an address")]
        [DataRow("000000a87cb5eafdcca6a8c983c585ac3c40d9b1eb2ec8ac9f31ff5ca4f3850ccc331a")]
        public async Task GetStateTestAsync(string address)
        {
            Assert.IsNotNull(client);

            await GetSingleTestWithParamAsync(client.GetStateAsync, address);
        }

        [TestMethod("Get a particular transaction")]
        [DataRow("3bb3461959ff60369456fec86e04636be0c3b9c4f0221465a35b383a9d899d89540744e1fdf87bbeaa30dcfba949ea9c8a7cca8f04597ffb5d1ce4f98562aa6a")]
        public async Task GetTransactionTestAsync(string transaction_id)
        {
            Assert.IsNotNull(client);

            await GetSingleTestWithParamAsync(client.GetTransactionAsync, transaction_id);
        }

        [TestMethod("Post BatchList for IntKey")]
        public async Task PostBatchListIntkeyTestAsync()
        {
            Assert.IsNotNull(client);

            IntKeyTransactionFamily txnFamily = new IntKeyTransactionFamily();

            var signer = new Signer();

            var settings = new EncoderSettings()
            {
                BatcherPublicKey = signer.GetPublicKey().ToHexString(),
                SignerPublickey = signer.GetPublicKey().ToHexString(),
                FamilyName = txnFamily.Name,
                FamilyVersion = txnFamily.Version
            };
            settings.Inputs.Add(txnFamily.AddressPrefix);
            settings.Outputs.Add(txnFamily.AddressPrefix);

            var encoder = new Encoder(settings, signer.GetPrivateKey());


            IntKeyTransaction txn = txnFamily.CreateEmptyTxn();
            
            txn.Name = "Foo2";
            txn.Verb = "inc";
            txn.Value = 42;


            var payload = encoder.EncodeSingleTransaction(txn.Wrap());


            var json =  await client.PostBatchListAsync(payload);

            Console.WriteLine(ToJson(json));

            if (json != null && json.Link != null)
            {
                var statuses = await client.GetBatchStatusUsingLinkAsync(json.Link);

                if (statuses != null)
                {
                    foreach (var status in statuses)
                    {
                        Console.WriteLine(ToJson(status));
                    }
                }
            }
        }

    }
}