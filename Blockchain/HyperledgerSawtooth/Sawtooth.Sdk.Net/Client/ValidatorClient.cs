﻿using Google.Protobuf;
using Google.Protobuf.Collections;
using NetMQ;
using NetMQ.Sockets;
using Sawtooth.Sdk.Net.Messaging;
using Sawtooth.Sdk.Net.Utils;
using System.Runtime.Serialization;
using static ClientStateListResponse.Types;

namespace Sawtooth.Sdk.Net.Client
{
    public class ValidatorClient : StreamListenerBase, IDisposable
    {

        private static readonly Logger log = Logger.GetLogger(typeof(ValidatorClient));

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
            log.Debug("Submitting Batch ...");

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
            log.Debug("Get Batch {0}", batchId);

            var response = await SendAsync(new ClientBatchGetRequest() { BatchId = batchId }
                                           .Wrap(Message.Types.MessageType.ClientBatchGetRequest), CancellationToken.None);
            return response.Unwrap<ClientBatchGetResponse>();
        }

        public async Task<PageOf<Block>> GetBlocksAsync(string? start)
        {
            log.Debug("Get Block {0}", start!=null?start:"<From Latest>");

            ClientBlockListRequest request = new ClientBlockListRequest();

            if (start != null)
            {
                request.Paging = new ClientPagingControls { Start = start };
            }
            ClientBlockListResponse response = await GetBlockListAsync(request);

            PageOf<Block> pagesOfBlocks = new PageOf<Block>(response.HeadId, response.Paging.Next);

            if (CheckStatus(response.Status))
            {
                foreach (Block block in response.Blocks)
                {
                    pagesOfBlocks.Add(block);
                }
            }

            return pagesOfBlocks;
        }

        private bool CheckStatus(ClientBlockListResponse.Types.Status status)
        {
            log.Info("Status : {0}", status);
            switch (status)
            {
                case ClientBlockListResponse.Types.Status.Ok: return true;
                case ClientBlockListResponse.Types.Status.Unset: throw new ValidatorClientException("Status unset.");
                case ClientBlockListResponse.Types.Status.InternalError: throw new ValidatorClientException("Intrnal Error.");
                case ClientBlockListResponse.Types.Status.NotReady: throw new ValidatorClientException("Not Ready.");
                case ClientBlockListResponse.Types.Status.NoResource: return false;
                case ClientBlockListResponse.Types.Status.InvalidPaging: throw new ValidatorClientException("Invalid Paging.");
                case ClientBlockListResponse.Types.Status.InvalidSort: throw new ValidatorClientException("Invalid Sort Criteria Specified.");
                case ClientBlockListResponse.Types.Status.InvalidId: throw new ValidatorClientException("Invalid Id Sent.");
                case ClientBlockListResponse.Types.Status.NoRoot: throw new ValidatorClientException("No Root Block Found.");
            }
            return false;
        }

        /// <summary>
        /// Gets the state for aa spcific address
        /// </summary>
        /// <returns>The state async.</returns>
        /// <param name="address">Address.</param>
        /// <param name="stateRoot">State root.</param>
        public async Task<ClientStateGetResponse> GetStateAsync(string address, string stateRoot)
        {
            log.Debug("Get State {0} {1}", address , stateRoot );
            var response = await SendAsync(new ClientStateGetRequest
            {
                Address = address,
                StateRoot = stateRoot
            }.Wrap(Message.Types.MessageType.ClientStateGetRequest), CancellationToken.None);
            return response.Unwrap<ClientStateGetResponse>();
        }

        public async Task<ClientStateGetResponse> GetStateAsync(string address)
        {
            log.Debug("Get State {0}", address);

            var response = await SendAsync(new ClientStateGetRequest
            {
                Address = address
            }.Wrap(Message.Types.MessageType.ClientStateGetRequest), CancellationToken.None);
            return response.Unwrap<ClientStateGetResponse>();
        }

        public async Task<ClientStateListResponse> GetStatesAsync(ClientStateListRequest request)
        {
            log.Debug("Get State {0} {1}", request.Address, request.StateRoot);

            var response = await SendAsync(request.Wrap(Message.Types.MessageType.ClientStateListRequest), CancellationToken.None);

            return response.Unwrap<ClientStateListResponse>();
        }
        public async Task<FullList<Entry>> GetAllStatesWithFilterAsync(string addressPrefix)
        {
            log.Debug("Get All States with filter {0}", addressPrefix);

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

                list = new FullList<Entry>(response.StateRoot);

                if (CheckStatus(response.Status))
                {
                    foreach (var state in response.Entries)
                    {
                        list.Add(state);
                    }
                    if (string.IsNullOrEmpty(response.Paging.Next))
                    {
                        break;
                    }
                    start = response.Paging.Next;
                }
                else
                {
                    break;
                }
            } while (true);
            return list;

        }

        public async Task<RepeatedField<string>> PostBatchListAsync(BatchList batchList)
        {
            log.Debug("Post Batch list: Count-> {0}", batchList.Batches.Count);

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
            log.Info("Status : {0}", status);

            switch (status)
            {
                case ClientBatchSubmitResponse.Types.Status.Ok: return;
                case ClientBatchSubmitResponse.Types.Status.Unset: throw new ValidatorClientException("Status unset.");
                case ClientBatchSubmitResponse.Types.Status.InternalError: throw new ValidatorClientException("Intrnal Error.");
                case ClientBatchSubmitResponse.Types.Status.InvalidBatch: throw new ValidatorClientException("Invalid Batch.");
                case ClientBatchSubmitResponse.Types.Status.QueueFull: throw new ValidatorClientException("Queue Full.");
            }

        }

        private bool CheckStatus(Status status)
        {
            log.Info("Status : {0}", status);

            switch (status)
            {
                case Status.Ok: return true;
                case Status.Unset: throw new ValidatorClientException("Status unset.");
                case Status.InternalError: throw new ValidatorClientException("Intrnal Error.");
                case Status.NotReady: throw new ValidatorClientException("Not Ready.");
                case Status.NoResource: return false;
                case Status.InvalidPaging: throw new ValidatorClientException("Invalid Paging.");
                case Status.InvalidSort: throw new ValidatorClientException("Invalid Sort Criteria Specified.");
                case Status.InvalidAddress: throw new ValidatorClientException("Invalid Address Sent.");
                case Status.NoRoot: throw new ValidatorClientException("No Root Block Found.");
                case Status.InvalidRoot: throw new ValidatorClientException("Invalid Sent.");
            }

            return false;
        }
        /// <summary>
        /// Gets a paged list of block data.
        /// </summary>
        /// <returns>The block list async.</returns>
        /// <param name="request">Request.</param>
        public async Task<ClientBlockListResponse> GetBlockListAsync(ClientBlockListRequest request)
        {
            log.Debug("Get Block list");

            var response = await SendAsync(request.Wrap(Message.Types.MessageType.ClientBlockListRequest), CancellationToken.None);
            return response.Unwrap<ClientBlockListResponse>();
        }

        public async Task<ClientBatchStatusResponse> GetBatchStatusesAsync(ClientBatchStatusRequest request)
        {
            log.Debug("Get Batch Statuses");
            var response = await SendAsync(request.Wrap(Message.Types.MessageType.ClientBatchStatusRequest), CancellationToken.None);
            return response.Unwrap<ClientBatchStatusResponse>();
        }
        public async Task<List<ClientBatchStatus>> GetBatchStatusAsync(string batchId)
        {
            log.Debug("Get Batch Status : {0}", batchId);
            RepeatedField<string> batchIds = new RepeatedField<string>();
            batchIds.Add(batchId);
            return await GetBatchStatusesAsync(batchIds);
        }
        
        public async Task<List<ClientBatchStatus>> GetBatchStatusesAsync(RepeatedField<string> batchIds)
        {
            log.Debug("Get Batch Statuses : {0}", batchIds);
            ClientBatchStatusRequest request = new ClientBatchStatusRequest();
            request.BatchIds.AddRange(batchIds);
            ClientBatchStatusResponse response = await GetBatchStatusesAsync(request);

            List<ClientBatchStatus> list = new List<ClientBatchStatus>();

            if (CheckStatus(response.Status))
            {
                foreach (ClientBatchStatus status in response.BatchStatuses)
                {
                    list.Add(status);
                }
            }

            return list;

        }

        private bool CheckStatus(ClientBatchStatusResponse.Types.Status status)
        {
            log.Info("Status : {0}", status);

            switch (status)
            {
                case ClientBatchStatusResponse.Types.Status.Ok: return true;
                case ClientBatchStatusResponse.Types.Status.Unset: throw new ValidatorClientException("Status unset.");
                case ClientBatchStatusResponse.Types.Status.InternalError: throw new ValidatorClientException("Intrnal Error.");
                case ClientBatchStatusResponse.Types.Status.NoResource: return false;
                case ClientBatchStatusResponse.Types.Status.InvalidId: throw new ValidatorClientException("Invalid Id Sent.");
            }
            return false;
        }

        /// <summary>
        /// Gets a paged list of state data.
        /// </summary>
        /// <returns>The state list async.</returns>
        /// <param name="request">Request.</param>
        public async Task<ClientStateListResponse> GetStateListAsync(ClientStateListRequest request)
        {
            log.Debug("Get State List");

            var respoonse = await SendAsync(request.Wrap(Message.Types.MessageType.ClientStateListRequest), CancellationToken.None);
            return respoonse.Unwrap<ClientStateListResponse>();
        }

        /// <summary>
        /// Gets a transaction information from the validator.
        /// </summary>
        /// <returns>The transaction async.</returns>
        /// <param name="trasnactionId">Trasnaction identifier.</param>
        public async Task<Transaction?> GetTransactionAsync(string trasnactionId)
        {
            log.Debug("Get Transaction {0}", trasnactionId);

            var response = await SendAsync(new ClientTransactionGetRequest { TransactionId = trasnactionId }
                                           .Wrap(Message.Types.MessageType.ClientTransactionGetRequest), CancellationToken.None);
            ClientTransactionGetResponse clientTxnResponse =  response.Unwrap<ClientTransactionGetResponse>();

            if (CheckStatus(clientTxnResponse.Status))
            {
                return clientTxnResponse.Transaction;
            }
            else
            {
                return null;
            }
        }

        private bool CheckStatus(ClientTransactionGetResponse.Types.Status status)
        {
            log.Info("Status : {0}", status);

            switch (status)
            {
                case ClientTransactionGetResponse.Types.Status.Ok: return true;
                case ClientTransactionGetResponse.Types.Status.Unset: throw new ValidatorClientException("Status unset.");
                case ClientTransactionGetResponse.Types.Status.InternalError: throw new ValidatorClientException("Intrnal Error.");
                case ClientTransactionGetResponse.Types.Status.NoResource: return false;
                case ClientTransactionGetResponse.Types.Status.InvalidId: throw new ValidatorClientException("Invalid Id Sent.");
            }
            return false;
        }
        /// <summary>
        /// Sends a message to the validator.
        /// This method allows sending a message directly to the validator. The message content must be of a type the validator can process.
        /// </summary>
        /// <returns>The message async.</returns>
        /// <param name="message">Message.</param>
        public Task<Message> SendMessageAsync(Message message)
        {
            log.Info("Sending Message {0}", message.MessageType);

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
            log.Info("Disposing ValidatorClient ...");

            Disconnect();
        }

    }

    [Serializable]
    public class ValidatorClientException : Exception
    {
        private static readonly Logger log = Logger.GetLogger(typeof(ValidatorClient));
        public ValidatorClientException()
        {
            log.Error("Validator Client Raised Exception.");
        }

        public ValidatorClientException(string? message) : base(message)
        {
            log.Error(message!=null? message: "Validator Client Raised Exception.");
        }

        public ValidatorClientException(string? message, Exception? innerException) : base(message, innerException)
        {
            log.Error(message != null ? message : "Validator Client Raised Exception.");
            if (innerException != null)
            {
                log.Error(innerException.GetType().FullName + " : " +  innerException.Message);
            }
        }

        protected ValidatorClientException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            log.Error("{0} : {1}", context, info);
        }
    }
}