using System;
using Google.Protobuf;
using NetMQ;
using NetMQ.Sockets;
using static Message.Types;
using System.Linq;
using Sawtooth.Sdk.Net.Utils;
using System.Net.Sockets;
using Sawtooth.Sdk.Net.Processor;

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

        private DateTime lastCommReceived = DateTime.Now;
        private Action? OnCommunicationLost = null;

        private object _locker = new object();
        private bool commMonitorStarted = false;

        private readonly ManualResetEvent _shutdownEvent = new ManualResetEvent(false);
        private Thread? _backGroundWorkerThread = null;

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
            lastCommReceived = DateTime.Now;

            StartCommunicationMonitor();

            var message = new Message();
            message.MergeFrom(Socket.ReceiveMultipartBytes().SelectMany(x => x).ToArray());
            log.Debug("Received Message: {0}", message.MessageType);
            if (message.MessageType == MessageType.PingRequest)
            {
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
        public void Connect(Action? CommunicationLostHandler)
        {
            log.Debug("Connecting to {0} ...", Address);
            Socket.Connect(Address);
            Poller.RunAsync();
            OnCommunicationLost = CommunicationLostHandler;
        }

        private void StartCommunicationMonitor()
        {
            lock (_locker)
            {
                if (commMonitorStarted) return;
                if (OnCommunicationLost != null)
                {
                    string threadName = Thread.CurrentThread.Name !=null? Thread.CurrentThread.Name: Thread.CurrentThread.ManagedThreadId.ToString();
                    _backGroundWorkerThread = new Thread(RunCommMonitor)
                    {
                        Name = "CommMonitor",
                        IsBackground = false
                    };
                    _backGroundWorkerThread.Start();
                }
            }
        }

        private void RunCommMonitor()
        {
            commMonitorStarted = true;
            bool isCommLost = false;
            log.Debug("Starting communication monitor ...");
            while(true)
            {
                //This kills our process
                if (_shutdownEvent.WaitOne(SawtoothConstants.PingIntervals*1000))
                {
                    break;
                }

                log.Debug("Last communication received from server at {0}.", lastCommReceived.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss','fff"));

                DateTime now = DateTime.Now;

                TimeSpan ping_interval = now.Subtract(lastCommReceived);

                if (ping_interval.Seconds > SawtoothConstants.PingIntervals)
                {
                    log.Debug("No communication received from server within {0} seconds...", SawtoothConstants.PingIntervals);
                    isCommLost = true;
                    break;
                }
            }
            log.Debug("Exiting communication monitor ...");
            commMonitorStarted = false;

            if (isCommLost && OnCommunicationLost != null)
            {
                Task.Run(() => 
                {
                    Thread.CurrentThread.Name = "CommLostHandler";
                    OnCommunicationLost();
                });
            }
        }

        /// <summary>
        /// Disconnects from the validator
        /// </summary>
        public void Disconnect()
        {
            log.Debug("Stopping comm monitor ...");

            _shutdownEvent.Set();
            _backGroundWorkerThread?.Join(); //Wait till monitor thread exits

            log.Debug("Disconnecting from {0} ...", Address);
            Socket.Options.Linger = TimeSpan.Zero;
            Socket.Disconnect(Address);
            Poller.RemoveAndDispose(Socket);

            log.Debug("Disconnected and sockets closed.");


            Poller.Stop();
            Poller.Dispose();

            log.Debug("Poller stopped and disposed.");

        }
    }
}
