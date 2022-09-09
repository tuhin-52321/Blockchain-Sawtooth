using System;
using Google.Protobuf;
using NetMQ;
using NetMQ.Sockets;
using static Message.Types;
using System.Linq;
using Sawtooth.Sdk.Net.Utils;
using System.Net.Sockets;

namespace Sawtooth.Sdk.Net.Messaging
{
    /// <summary>
    /// Stream.
    /// </summary>
    public class Stream
    {
        private static readonly Logger log = Logger.GetLogger(typeof(Stream));

        readonly string Address;

        NetMQSocket Socket;
        NetMQPoller Poller;

        readonly IStreamListener? Listener;

        readonly object _sendLock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Sawtooth.Sdk.Messaging.Stream"/> class.
        /// </summary>
        /// <param name="address">Address.</param>
        /// <param name="listener">Listener.</param>
        public Stream(string address, IStreamListener? listener = null)
        {
            Address = address;

            Socket = new DealerSocket();

            Socket.ReceiveReady += Receive;

            Socket.Options.ReconnectInterval = TimeSpan.FromSeconds(2);

            Poller = new NetMQPoller();
            Poller.Add(Socket);


            Listener = listener;


        }


        void Receive(object? _, NetMQSocketEventArgs e)
        {
            var message = new Message();
            message.MergeFrom(Socket.ReceiveMultipartBytes().SelectMany(x => x).ToArray());
            log.Debug("Received Message: {0}", message.MessageType);
            if (message.MessageType == MessageType.PingRequest)
            {

                Listener?.OnPingRequest();

                log.Debug("Sending Ping Response.");
                Socket.SendFrame(new PingResponse().Wrap(message, MessageType.PingResponse).ToByteArray());


                return;
            }

            Listener?.OnMessage(message);
        }

        /// <summary>
        /// Send the specified message.
        /// </summary>
        /// <returns>The send.</returns>
        /// <param name="message">Message.</param>
        public void Send(Message message)
        {
            lock (_sendLock)
            {
                log.Info("Sending message {0} {1} ...", message.MessageType, message.CorrelationId);
                Socket.SendFrame(message.ToByteString().ToByteArray());

            }
        }


        /// <summary>
        /// Connects to the validator
        /// </summary>
        public void Connect()
        {
            log.Debug("Connecting to {0} ...", Address);
            Socket.Connect(Address);
            Poller.RunAsync();
        }

        /// <summary>
        /// Disconnects from the validator
        /// </summary>
        public void Disconnect()
        {
            log.Debug("Disconnecting from {0} ...", Address);
            Socket.Options.Linger = TimeSpan.Zero;
            Socket.Disconnect(Address);
            Poller.StopAsync();
            Poller.RemoveAndDispose(Socket);
            log.Debug("Disconnected and scokets closed.");
        }
    }
}
