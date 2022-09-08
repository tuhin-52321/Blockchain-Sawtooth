using Google.Protobuf;
using log4net;
using Sawtooth.Sdk.Net.Client;
using Sawtooth.Sdk.Net.Messaging;
using Sawtooth.Sdk.Net.Utils;
using static Message.Types;
using static Policy.Types;

namespace Sawtooth.Sdk.Net.Processor
{
    /// <summary>
    /// Transaction context.
    /// </summary>
    public class TransactionContext
    {
        readonly IStreamListener Stream;
        readonly string ContextId;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Sawtooth.Sdk.Processor.TransactionContext"/> class.
        /// </summary>
        /// <param name="stream">Stream.</param>
        /// <param name="contextId">Context identifier.</param>
        public TransactionContext(IStreamListener stream, string contextId)
        {
            Stream = stream;
            ContextId = contextId;
        }

        /// <summary>
        /// Gets the state for the given addresses.
        /// </summary>
        /// <returns>The state async.</returns>
        /// <param name="addresses">Addresses.</param>
        public async Task<Dictionary<string, ByteString>> GetStateAsync(string[] addresses)
        {
            var request = new TpStateGetRequest { ContextId = ContextId };
            request.Addresses.AddRange(addresses);

            var response = await Stream.SendAsync(request.Wrap(MessageType.TpStateGetRequest), CancellationToken.None);
            return response.Unwrap<TpStateGetResponse>()
                           .Entries.ToDictionary(x => x.Address, x => x.Data);
        }

        private async Task<ClientStateListResponse> GetStatesAsync(ClientStateListRequest request)
        {

            var response = await Stream.SendAsync(request.Wrap(MessageType.ClientStateListRequest), CancellationToken.None);

            return response.Unwrap<ClientStateListResponse>();
        }
        public async Task<T?> GetFirstMatchingStateAsync<T>(string address_prefix, Predicate<T> filter) where T:IMessage, new()
        {
            string? start = null;
            do
            {
                ClientStateListRequest request = new ClientStateListRequest { Address = address_prefix };
                if (start != null)
                {
                    request.Paging.Start = start;
                }
                ClientStateListResponse response = await GetStatesAsync(request);

                if (response.Status == ClientStateListResponse.Types.Status.Ok)
                {
                    foreach (var state in response.Entries)
                    {
                        T data = state.Data.ToProtobufClass<T>();
                        if (filter(data))
                        {
                            return data;
                        }
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
            return default(T);

        }

        /// <summary>
        /// Sets the state at the given addresses
        /// </summary>
        /// <returns>The state async.</returns>
        /// <param name="addressValuePairs">Address value pairs.</param>
        public async Task<string[]> SetStateAsync(Dictionary<string, ByteString> addressValuePairs)
        {
            var request = new TpStateSetRequest { ContextId = ContextId };
            request.Entries.AddRange(addressValuePairs.Select(x => new TpStateEntry { Address = x.Key, Data = x.Value }));

            var response = await Stream.SendAsync(request.Wrap(MessageType.TpStateSetRequest), CancellationToken.None);
            return response.Unwrap<TpStateSetResponse>()
                             .Addresses.ToArray();
        }

        /// <summary>
        /// Deletes the state for the given addresses.
        /// </summary>
        /// <returns>The state async.</returns>
        /// <param name="addresses">Addresses.</param>
        public async Task<string[]> DeleteStateAsync(string[] addresses)
        {
            var request = new TpStateDeleteRequest { ContextId = ContextId };
            request.Addresses.AddRange(addresses);

            var response = await Stream.SendAsync(request.Wrap(MessageType.TpStateDeleteRequest), CancellationToken.None);
            return response.Unwrap<TpStateDeleteResponse>()
                             .Addresses.ToArray();
        }

        /// <summary>
        /// Adds custom receipt data for the trasnaction.
        /// </summary>
        /// <returns>The receipt data async.</returns>
        /// <param name="data">Data.</param>
        public async Task<bool> AddReceiptDataAsync(ByteString data)
        {
            var request = new TpReceiptAddDataRequest() { ContextId = ContextId };
            request.Data = data;

            var response = await Stream.SendAsync(request.Wrap(MessageType.TpReceiptAddDataRequest), CancellationToken.None);
            return response.Unwrap<TpReceiptAddDataResponse>()
                             .Status == TpReceiptAddDataResponse.Types.Status.Ok;
        }

        /// <summary>
        /// Adds an event with custom data
        /// </summary>
        /// <returns><code>true</code> if the event request succeeded.</returns>
        /// <param name="name">Name.</param>
        /// <param name="attributes">Attributes.</param>
        /// <param name="data">Data.</param>
        public async Task<bool> AddEventAsync(string name, Dictionary<string, string> attributes, ByteString data)
        {
            var addEvent = new Event { EventType = name, Data = data };
            addEvent.Attributes.AddRange(attributes.Select(x => new Event.Types.Attribute { Key = x.Key, Value = x.Value }));

            var request = new TpEventAddRequest { ContextId = ContextId, Event = addEvent };

            var response = await Stream.SendAsync(request.Wrap(MessageType.TpEventAddRequest), CancellationToken.None);
            return response.Unwrap<TpEventAddResponse>()
                             .Status == TpEventAddResponse.Types.Status.Ok;
        }
    }
}
