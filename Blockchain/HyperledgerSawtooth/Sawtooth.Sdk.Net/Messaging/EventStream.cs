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
        readonly string Address;
        readonly string CorrelationId;
        readonly Action<ClientEventsSubscribeResponse.Types.Status, string> OnError;
        readonly Action<StateChange> OnStateChange;
        readonly NetMQSocket Socket;
        readonly NetMQPoller Poller;


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

            Send(message);
        }

        void Receive(object? _, NetMQSocketEventArgs e)
        {
            var message = new Message();
            message.MergeFrom(Socket.ReceiveMultipartBytes().SelectMany(x => x).ToArray());

            //Check the subscription response
            if (message.MessageType == MessageType.ClientEventsSubscribeResponse && message.CorrelationId == CorrelationId)
            {
                ClientEventsSubscribeResponse response = message.Unwrap<ClientEventsSubscribeResponse>();

                if (response.Status != ClientEventsSubscribeResponse.Types.Status.Ok)
                {
                    OnError(response.Status, response.ResponseMessage);
                }
            }

            //Respond to Pings
            if (message.MessageType == MessageType.PingRequest)
            {
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
                    foreach (var state in StateChanges.StateChanges)
                    {
                        OnStateChange(state);
                    }
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
            Socket.Connect(Address);
            Poller.RunAsync();
        }

        /// <summary>
        /// Disconnects from the validator
        /// </summary>
        public void Disconnect()
        {
            Socket.Disconnect(Address);
            Poller.StopAsync();
        }
    }
}
