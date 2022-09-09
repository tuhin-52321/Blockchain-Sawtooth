using System;
using Google.Protobuf;
using NetMQ;
using NetMQ.Sockets;
using static Message.Types;
using System.Linq;
using Sawtooth.Sdk.Net.Utils;

namespace Sawtooth.Sdk.Net.Messaging
{
    /// <summary>
    /// Stream.
    /// </summary>
    public class EventStream
    {
        private static readonly Logger log = Logger.GetLogger(typeof(EventStream));

        private readonly string Address;
        private readonly string CorrelationId;
        private string UnsubCorrelationId = String.Empty;
        private readonly Action<ClientEventsSubscribeResponse.Types.Status, string> OnError;
        private readonly Action<StateChange> OnStateChange;
        private readonly NetMQSocket Socket;
        private readonly NetMQPoller Poller;


        /// <summary>
        /// Initializes a new instance of the <see cref="T:Sawtooth.Sdk.Messaging.Stream"/> class.
        /// </summary>
        /// <param name="address">Address.</param>
        /// <param name="listener">Listener.</param>
        public EventStream(string address, ClientEventsSubscribeRequest request, Action<StateChange> OnStateChange, Action<ClientEventsSubscribeResponse.Types.Status, string> OnError)
        {
            this.OnError = OnError;
            this.OnStateChange = OnStateChange;

            Socket = new DealerSocket();
            Socket.ReceiveReady += Receive;
            Socket.Options.ReconnectInterval = TimeSpan.FromSeconds(2);

            Poller = new NetMQPoller();
            Poller.Add(Socket);

            Message message = request.Wrap(MessageType.ClientEventsSubscribeRequest);

            Address = address;
            CorrelationId = message.CorrelationId;

            Connect();

            log.Debug("Subscribing for state changes...", message.MessageType);

            Send(message);
        }

        void Receive(object? _, NetMQSocketEventArgs e)
        {
            var message = new Message();
            message.MergeFrom(Socket.ReceiveMultipartBytes().SelectMany(x => x).ToArray());

            log.Debug("Received Message: {0}", message.MessageType);


            //Check the subscription response
            if (message.MessageType == MessageType.ClientEventsSubscribeResponse && message.CorrelationId == CorrelationId)
            {
                ClientEventsSubscribeResponse response = message.Unwrap<ClientEventsSubscribeResponse>();

                if (response.Status != ClientEventsSubscribeResponse.Types.Status.Ok)
                {
                    log.Error("Subscription to state events failed: {0} ({1})", response.ResponseMessage, response.Status);

                    OnError(response.Status, response.ResponseMessage);
                }
            }

            //Respond to Pings
            if (message.MessageType == MessageType.PingRequest)
            {
                log.Debug("Sending Ping Response.");

                Socket.SendFrame(new PingResponse().Wrap(message, MessageType.PingResponse).ToByteArray());
                return;
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

                        OnStateChange(state);
                    }
                }
            }

            //Check the Unsubscribe response
            if (message.MessageType == MessageType.ClientEventsUnsubscribeResponse && message.CorrelationId == UnsubCorrelationId)
            {
                ClientEventsUnsubscribeResponse response = message.Unwrap<ClientEventsUnsubscribeResponse>();
                if(response.Status != ClientEventsUnsubscribeResponse.Types.Status.Ok)
                {
                    log.Error("Unsubscription to state events failed: {0}", response.Status);
                }
                else
                {
                    log.Info("Unsubscribed to State Events.");
                }
            }



        }

        /// <summary>
        /// Send the specified message.
        /// </summary>
        /// <returns>The send.</returns>
        /// <param name="message">Message.</param>
        private void Send(Message message) => Socket.SendFrame(message.ToByteString().ToByteArray());

        /// <summary>
        /// Connects to the validator
        /// </summary>
        private void Connect()
        {
            log.Debug("Connecting to {0} for event subscriptions.", Address);

            Socket.Connect(Address);
            Poller.RunAsync();
        }

        /// <summary>
        /// Disconnects from the validator
        /// </summary>
        public void Disconnect()
        {
            log.Debug("Disconnecting from {0} for event subscriptions.", Address);

            //Unsubscribe
            ClientEventsUnsubscribeRequest request = new ClientEventsUnsubscribeRequest();
            Message message = request.Wrap(MessageType.ClientEventsUnsubscribeRequest);
            UnsubCorrelationId =  message.CorrelationId;
            Send(message);

            //Diconnect
            Socket.Disconnect(Address);

            //Stop Poller
            Poller.StopAsync();

            Poller.RemoveAndDispose(Socket);

        }
    }
}
