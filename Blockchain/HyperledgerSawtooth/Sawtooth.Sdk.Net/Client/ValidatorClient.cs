using Google.Protobuf.Collections;
using NetMQ;
using NetMQ.Sockets;
using Sawtooth.Sdk.Net.Messaging;
using Sawtooth.Sdk.Net.Utils;
using static ClientStateListResponse.Types;

namespace Sawtooth.Sdk.Net.Client
{
    public class ValidatorClient : StreamListenerBase, IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Sawtooth.Sdk.Client.ValidatorClient"/> class.
        /// </summary>
        /// <param name="address">Address.</param>
        ValidatorClient(string address) : base(address)
        {
            Connect();
        }

        /// <summary>
        /// Creates a <see cref="ValidatorClient"/> instance and connects to the specified address.
        /// Use this inside a <c>using</c> statement.
        /// </summary>
        /// <returns>The create.</returns>
        /// <param name="address">Address.</param>
        public static ValidatorClient Create(string address)
        {
            return new ValidatorClient(address);
        }

        /// <summary>
        /// Sends a batch list to the validator
        /// </summary>
        /// <returns>The batch list async.</returns>
        /// <param name="batchList">Batch list.</param>
        public async Task<ClientBatchSubmitResponse> SubmitBatchAsync(BatchList batchList)
        {
            var request = new ClientBatchSubmitRequest();
            request.Batches.AddRange(batchList.Batches);

            var response = await SendAsync(request.Wrap(Message.Types.MessageType.ClientBatchSubmitRequest), CancellationToken.None);
            return response.Unwrap<ClientBatchSubmitResponse>();
        }

        /// <summary>
        /// Gets the batch data for the given <c>batchId</c>. />
        /// </summary>
        /// <returns>The batch async.</returns>
        /// <param name="batchId">Batch identifier.</param>
        public async Task<ClientBatchGetResponse> GetBatchAsync(string batchId)
        {
            var response = await SendAsync(new ClientBatchGetRequest() { BatchId = batchId }
                                           .Wrap(Message.Types.MessageType.ClientBatchGetRequest), CancellationToken.None);
            return response.Unwrap<ClientBatchGetResponse>();
        }

        public async Task<PageOf<Block>> GetBlocksAsync(string? start)
        {
            ClientBlockListRequest request = new ClientBlockListRequest();

            if (start != null)
            {
                request.Paging = new ClientPagingControls { Start = start };
            }
            ClientBlockListResponse response = await GetBlockListAsync(request);

            CheckStatus(response.Status);

            PageOf<Block> pagesOfBlocks = new PageOf<Block>(response.HeadId, response.Paging.Next);
            

            foreach (Block block in response.Blocks)
            {
                pagesOfBlocks.Add(block);
            }

            return pagesOfBlocks;
        }

        private void CheckStatus(ClientBlockListResponse.Types.Status status)
        {
            switch (status)
            {
                case ClientBlockListResponse.Types.Status.Ok: return;
                case ClientBlockListResponse.Types.Status.Unset: throw new IOException("Status unset.");
                case ClientBlockListResponse.Types.Status.InternalError: throw new IOException("Intrnal Error.");
                case ClientBlockListResponse.Types.Status.NotReady: throw new IOException("Not Ready.");
                case ClientBlockListResponse.Types.Status.NoResource: throw new IOException("No Resource.");
                case ClientBlockListResponse.Types.Status.InvalidPaging: throw new IOException("Invalid Paging.");
                case ClientBlockListResponse.Types.Status.InvalidSort: throw new IOException("Invalid Sort Criteria Specified.");
                case ClientBlockListResponse.Types.Status.InvalidId: throw new IOException("Invalid Id Sent.");
                case ClientBlockListResponse.Types.Status.NoRoot: throw new IOException("No Root Block Found.");
            }

        }

        /// <summary>
        /// Gets the state for aa spcific address
        /// </summary>
        /// <returns>The state async.</returns>
        /// <param name="address">Address.</param>
        /// <param name="stateRoot">State root.</param>
        public async Task<ClientStateGetResponse> GetStateAsync(string address, string stateRoot)
        {
            var response = await SendAsync(new ClientStateGetRequest
            {
                Address = address,
                StateRoot = stateRoot
            }.Wrap(Message.Types.MessageType.ClientStateGetRequest), CancellationToken.None);
            return response.Unwrap<ClientStateGetResponse>();
        }

        public async Task<ClientStateGetResponse> GetStateAsync(string address)
        {
            var response = await SendAsync(new ClientStateGetRequest
            {
                Address = address
            }.Wrap(Message.Types.MessageType.ClientStateGetRequest), CancellationToken.None);
            return response.Unwrap<ClientStateGetResponse>();
        }

        public async Task<ClientStateListResponse> GetStatesAsync(ClientStateListRequest request)
        {
            var response = await SendAsync(request.Wrap(Message.Types.MessageType.ClientStateListRequest), CancellationToken.None);

            return response.Unwrap<ClientStateListResponse>();
        }
        public async Task<FullList<Entry>> GetAllStatesWithFilterAsync(string addressPrefix)
        {
            string? start = null;
            FullList<Entry> list;
            do
            {
                ClientStateListRequest request = new ClientStateListRequest { Address = addressPrefix };
                if(start != null)
                {
                    request.Paging.Start = start;
                }
                ClientStateListResponse response = await GetStatesAsync(request);

                CheckStatus(response.Status);

                list = new FullList<Entry>(response.StateRoot);
                foreach (var state in response.Entries)
                {
                    list.Add(state);
                }
                if(string.IsNullOrEmpty(response.Paging.Next))
                {
                    break;
                }
                start = response.Paging.Next;
            } while (true);
            return list;

        }

        public async Task<RepeatedField<string>> PostBatchListAsync(BatchList batchList)
        {
            var response = await SubmitBatchAsync(batchList);

            CheckStatus(response.Status);

            RepeatedField<string> batchIds = new RepeatedField<string>();
            foreach(Batch b in batchList.Batches)
            {
                batchIds.Add(b.HeaderSignature);
            }

            return batchIds;
        }

        private void CheckStatus(ClientBatchSubmitResponse.Types.Status status)
        {
            switch (status)
            {
                case ClientBatchSubmitResponse.Types.Status.Ok: return;
                case ClientBatchSubmitResponse.Types.Status.Unset: throw new IOException("Status unset.");
                case ClientBatchSubmitResponse.Types.Status.InternalError: throw new IOException("Intrnal Error.");
                case ClientBatchSubmitResponse.Types.Status.InvalidBatch: throw new IOException("Invalid Batch.");
                case ClientBatchSubmitResponse.Types.Status.QueueFull: throw new IOException("Queue Full.");
            }

        }

        private void CheckStatus(ClientStateListResponse.Types.Status status)
        {
            switch (status)
            {
                case ClientStateListResponse.Types.Status.Ok: return;
                case ClientStateListResponse.Types.Status.Unset: throw new IOException("Status unset.");
                case ClientStateListResponse.Types.Status.InternalError: throw new IOException("Intrnal Error.");
                case ClientStateListResponse.Types.Status.NotReady: throw new IOException("Not Ready.");
                case ClientStateListResponse.Types.Status.NoResource: throw new IOException("No Resource.");
                case ClientStateListResponse.Types.Status.InvalidPaging: throw new IOException("Invalid Paging.");
                case ClientStateListResponse.Types.Status.InvalidSort: throw new IOException("Invalid Sort Criteria Specified.");
                case ClientStateListResponse.Types.Status.InvalidAddress: throw new IOException("Invalid Address Sent.");
                case ClientStateListResponse.Types.Status.NoRoot: throw new IOException("No Root Block Found.");
                case ClientStateListResponse.Types.Status.InvalidRoot: throw new IOException("Invalid Sent.");
            }

        }
        /// <summary>
        /// Gets a paged list of block data.
        /// </summary>
        /// <returns>The block list async.</returns>
        /// <param name="request">Request.</param>
        public async Task<ClientBlockListResponse> GetBlockListAsync(ClientBlockListRequest request)
        {
            var response = await SendAsync(request.Wrap(Message.Types.MessageType.ClientBlockListRequest), CancellationToken.None);
            return response.Unwrap<ClientBlockListResponse>();
        }

        public async Task<ClientBatchStatusResponse> GetBatchStatusesAsync(ClientBatchStatusRequest request)
        {
            var response = await SendAsync(request.Wrap(Message.Types.MessageType.ClientBatchStatusRequest), CancellationToken.None);
            return response.Unwrap<ClientBatchStatusResponse>();
        }
        public async Task<List<ClientBatchStatus>> GetBatchStatusesAsync(string batchId)
        {
            RepeatedField<string> batchIds = new RepeatedField<string>();
            batchIds.Add(batchId);
            return await GetBatchStatusesAsync(batchIds);
        }
        
        public async Task<List<ClientBatchStatus>> GetBatchStatusesAsync(RepeatedField<string> batchIds)
        {
            ClientBatchStatusRequest request = new ClientBatchStatusRequest();
            request.BatchIds.AddRange(batchIds);
            ClientBatchStatusResponse response = await GetBatchStatusesAsync(request);

            CheckStatus(response.Status);

            List<ClientBatchStatus> list = new List<ClientBatchStatus>();


            foreach (ClientBatchStatus status in response.BatchStatuses)
            {
                list.Add(status);
            }

            return list;

        }

        private void CheckStatus(ClientBatchStatusResponse.Types.Status status)
        {
            switch (status)
            {
                case ClientBatchStatusResponse.Types.Status.Ok: return;
                case ClientBatchStatusResponse.Types.Status.Unset: throw new IOException("Status unset.");
                case ClientBatchStatusResponse.Types.Status.InternalError: throw new IOException("Intrnal Error.");
                case ClientBatchStatusResponse.Types.Status.NoResource: throw new IOException("No Resource.");
                case ClientBatchStatusResponse.Types.Status.InvalidId: throw new IOException("Invalid Id Sent.");
            }

        }

        /// <summary>
        /// Gets a paged list of state data.
        /// </summary>
        /// <returns>The state list async.</returns>
        /// <param name="request">Request.</param>
        public async Task<ClientStateListResponse> GetStateListAsync(ClientStateListRequest request)
        {
            var respoonse = await SendAsync(request.Wrap(Message.Types.MessageType.ClientStateListRequest), CancellationToken.None);
            return respoonse.Unwrap<ClientStateListResponse>();
        }

        /// <summary>
        /// Gets a transaction information from the validator.
        /// </summary>
        /// <returns>The transaction async.</returns>
        /// <param name="trasnactionId">Trasnaction identifier.</param>
        public async Task<Transaction> GetTransactionAsync(string trasnactionId)
        {
            var response = await SendAsync(new ClientTransactionGetRequest { TransactionId = trasnactionId }
                                           .Wrap(Message.Types.MessageType.ClientTransactionGetRequest), CancellationToken.None);
            ClientTransactionGetResponse clientTxnResponse =  response.Unwrap<ClientTransactionGetResponse>();

            CheckStatus(clientTxnResponse.Status);

            return clientTxnResponse.Transaction;
        }

        private void CheckStatus(ClientTransactionGetResponse.Types.Status status)
        {
            switch (status)
            {
                case ClientTransactionGetResponse.Types.Status.Ok: return;
                case ClientTransactionGetResponse.Types.Status.Unset: throw new IOException("Status unset.");
                case ClientTransactionGetResponse.Types.Status.InternalError: throw new IOException("Intrnal Error.");
                case ClientTransactionGetResponse.Types.Status.NoResource: throw new IOException("No Resource.");
                case ClientTransactionGetResponse.Types.Status.InvalidId: throw new IOException("Invalid Id Sent.");
            }

        }
        /// <summary>
        /// Sends a message to the validator.
        /// This method allows sending a message directly to the validator. The message content must be of a type the validator can process.
        /// </summary>
        /// <returns>The message async.</returns>
        /// <param name="message">Message.</param>
        public Task<Message> SendMessageAsync(Message message)
        {
            return SendAsync(message, CancellationToken.None);
        }

        /// <summary>
        /// Releases all resource used by the <see cref="T:Sawtooth.Sdk.Client.ValidatorClient"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the
        /// <see cref="T:Sawtooth.Sdk.Client.ValidatorClient"/>. The <see cref="Dispose"/> method leaves the
        /// <see cref="T:Sawtooth.Sdk.Client.ValidatorClient"/> in an unusable state. After calling
        /// <see cref="Dispose"/>, you must release all references to the
        /// <see cref="T:Sawtooth.Sdk.Client.ValidatorClient"/> so the garbage collector can reclaim the memory that the
        /// <see cref="T:Sawtooth.Sdk.Client.ValidatorClient"/> was occupying.</remarks>
        public void Dispose()
        {
            Disconnect();
        }

    }
}