using Google.Protobuf;
using log4net;
using Sawtooth.Sdk.Net.Utils;
using System.Collections.Concurrent;
using static Message.Types;

namespace Sawtooth.Sdk.Net.Messaging
{
    /// <summary>
    /// Stream listener base.
    /// </summary>
    public abstract class StreamListenerBase : IStreamListener
    {
        private static readonly Logger log = Logger.GetLogger(typeof(StreamListenerBase));

        protected readonly List<EventSubscription> subscriptions = new List<EventSubscription>();
        protected readonly Dictionary<string, Action<StateChange>> stateChangeHandlers = new Dictionary<string, Action<StateChange>>();

        readonly ConcurrentDictionary<string, TaskCompletionSource<Message>> Futures;
        /// <summary>
        /// The stream.
        /// </summary>
        protected readonly Stream Stream;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Sawtooth.Sdk.Messaging.StreamListenerBase"/> class.
        /// </summary>
        /// <param name="address">Address.</param>
        public StreamListenerBase(string address)
        {
            Stream = new Stream(address, this);
            Futures = new ConcurrentDictionary<string, TaskCompletionSource<Message>>();
        }

        /// <summary>
        /// Called when the stream receives a message
        /// </summary>
        /// <param name="message">Message.</param>
        public virtual void OnMessage(Message message)
        {
            if (Futures.TryGetValue(message.CorrelationId, out var source))
            {
                if (source.Task.Status != TaskStatus.RanToCompletion) source.SetResult(message);
                Futures.TryRemove(message.CorrelationId, out var _);
            }

            //Check client events
            if (message.MessageType == MessageType.ClientEvents)
            {
                EventList eventList = message.Unwrap<EventList>();
                foreach (Event ev in eventList.Events)
                {
                    StateChangeList StateChanges = new StateChangeList();
                    StateChanges.MergeFrom(ev.Data);

                    log.Debug("Received {0} State Changes.", StateChanges.StateChanges.Count);

                    foreach (var state in StateChanges.StateChanges)
                    {
                        log.Debug("State change for '{0}' -> Type: {1}", state.Address, state.Type);

                        //Do we have an handler for this state address?
                        foreach(string address_prefix in stateChangeHandlers.Keys)
                        {
                            if(state.Address.StartsWith(address_prefix))
                            {
                                stateChangeHandlers[address_prefix](state);
                                break; //Found handler, no need to search more (assuming we have one handler per address prefix
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sends the message to the stream
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="message">Message.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        private Task<Message> SendImplAsync(Message message, CancellationToken cancellationToken)
        {
            var source = new TaskCompletionSource<Message>();
            cancellationToken.Register(() => source.SetCanceled());

            if (Futures.TryAdd(message.CorrelationId, source))
            {
                Stream.Send(message);
                return source.Task;
            }
            if (Futures.TryGetValue(message.CorrelationId, out var task))
            {
                return task.Task;
            }
            throw new InvalidOperationException("Cannot get or set future context for this message.");
        }


        /// <summary>
        /// Sends the message to the stream (with timeout
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="message">Message.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public async Task<ZMQResponse> SendAsync(Message message,  CancellationToken cancellationToken, int timeout_seconds = -1)
        {
            try
            {
                if (timeout_seconds > 0)
                {
                    var task = SendImplAsync(message, cancellationToken);
                    if (await Task.WhenAny(task, Task.Delay(timeout_seconds * 1000)) == task)
                    {
                        return new ZMQResponse { IsSuccess = true, Message = task.Result };
                    }
                    else
                    {
                        Futures.TryRemove(message.CorrelationId, out var _); //Remove the tracker
                        return new ZMQResponse { IsSuccess = false, Error = $"Operation timeout : waited for {timeout_seconds} seconds." };
                    }
                }
                else
                {
                    return new ZMQResponse { IsSuccess = true, Message = await SendImplAsync(message, cancellationToken) }; //Infinite wait
                }
            }catch(Exception ex)
            {
                return new ZMQResponse { IsSuccess = false, Error = ex.Message };
            }
        }

        /// <summary>
        /// Connects to the stream
        /// </summary>
        protected void Connect(Action OncommunicationLost) => Stream.Connect(OncommunicationLost);

        /// <summary>
        /// Disconnects from the stream
        /// </summary>
        protected void Disconnect() => Stream.Disconnect();

    }
}