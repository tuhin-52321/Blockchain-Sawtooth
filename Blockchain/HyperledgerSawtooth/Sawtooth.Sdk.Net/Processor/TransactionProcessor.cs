using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Org.BouncyCastle.Asn1.Ocsp;
using Sawtooth.Sdk.Net.Messaging;
using Sawtooth.Sdk.Net.Utils;
using static Message.Types;

namespace Sawtooth.Sdk.Net.Processor
{
    /// <summary>
    /// Transaction processor.
    /// </summary>
    public class TransactionProcessor : StreamListenerBase
    {
        private static Logger log = Logger.GetLogger(typeof(TransactionProcessor));

        readonly List<ITransactionHandler> Handlers;

        readonly List<Task<ZMQResponse>> HandlerTasks;

        private bool stopping;

        readonly Dictionary<int,CancellationTokenSource> taskCancellationTokenSources = new Dictionary<int, CancellationTokenSource>();

        private ManualResetEvent trackRegistration = new ManualResetEvent(false);

        private DateTime lastPingReceived = DateTime.Now;


        /// <summary>
        /// Initializes a new instance of the <see cref="T:Sawtooth.Sdk.Processor.TransactionProcessor"/> class.
        /// </summary>
        /// <param name="address">Address.</param>
        public TransactionProcessor(string address) : base(address)
        {
            Handlers = new List<ITransactionHandler>();
            HandlerTasks = new List<Task<ZMQResponse>>();

        }

        private void StayConnected()
        {
            bool restart;
            while (true)
            {
                restart = false;
                int? delay_id = null;
                try
                {
                    var taskCancellationTokenSource = new CancellationTokenSource();
                    Task delay = Task.Delay(SawtoothConstants.PingIntervals, taskCancellationTokenSource.Token);
                    delay_id = delay.Id;
                    taskCancellationTokenSources.Add((int)delay_id, taskCancellationTokenSource);
                    delay.Wait();
                    if (stopping) break;
                }
                catch { break;}
                finally
                {
                    if(delay_id != null) taskCancellationTokenSources.Remove((int)delay_id);
                }

                DateTime now = DateTime.Now;

                TimeSpan ping_interval = now.Subtract(lastPingReceived);

                if(ping_interval.Seconds > SawtoothConstants.PingIntervals)
                {
                    log.Debug("No ping response received within {0} seconds, rstarting ...", SawtoothConstants.Timeout);
                    restart = true;
                    break;

                }

            }
            if(restart) Restart();


        }

        private void Restart()
        {
            log.Info("No ping response received, assuming node connection is lost.", SawtoothConstants.PingIntervals);
            log.Info("Restarting new registration ...");
            Start(true);

        }

        /// <summary>
        /// Adds a new transaction handler to this process. This method must be called before calling <see cref="Start()"/>
        /// </summary>
        /// <param name="handler">Handler.</param>
        public void AddHandler(ITransactionHandler handler) => Handlers.Add(handler);

        /// <summary>
        /// Starts the processor and connects to the message stream
        /// </summary>
        public void Start(bool restart = false)
        {
            stopping = false;
            if (!restart)
            {
                Connect();
            }

            if (restart)
            {
                CancellationTokenSource taskCancellationTokenSource = new CancellationTokenSource();
                log.Info("[Restarting] Sending unregistration message ...");
                Task<ZMQResponse> unregister = SendAsync(new TpUnregisterRequest().Wrap(MessageType.TpUnregisterRequest), taskCancellationTokenSource.Token); ;
                taskCancellationTokenSources.Add(unregister.Id, taskCancellationTokenSource);
                log.Info("[Restarting] Wating for unregistration responses ...");
                while (true)
                {
                    if (Task.WaitAll(new[] { unregister }, TimeSpan.FromSeconds(SawtoothConstants.GranularWaitTimeInSeconds)))
                    {
                        taskCancellationTokenSources.Remove(unregister.Id);
                        var response = unregister.Result;
                        if (response.IsSuccess)
                        {
                            TpUnregisterResponse unregistration_resp = response.Message.Unwrap<TpUnregisterResponse>();
                            log.Info("[Restarting] Received Unregistration Response : {0}", unregistration_resp.Status);
                        }
                        else
                        {
                            log.Info("[Restarting] Unregistration Failed : {0}", response.Error);
                        }

                        //Do not care about actual response, will continue registration process anyways
                        break;

                    }
                    else
                    {
                        log.Info("[Restarting] No response received within {0} seconds, still waiting ...", SawtoothConstants.GranularWaitTimeInSeconds);
                    }
                }

            }
            HandlerTasks.Clear();
            if (stopping) return;
            foreach (var handler in Handlers)
            {
                var request = new TpRegisterRequest { Version = handler.Version, Family = handler.FamilyName };
                request.Namespaces.AddRange(handler.Namespaces);
                CancellationTokenSource taskCancellationTokenSource = new CancellationTokenSource();
                var task = SendAsync(request.Wrap(MessageType.TpRegisterRequest), taskCancellationTokenSource.Token);
                HandlerTasks.Add(task);
                taskCancellationTokenSources.Add(task.Id, taskCancellationTokenSource);
            }
            if (Handlers.Count > 0)
            {
                Task.Run(() =>
                {
                    while (true)
                    {
                        log.Info("Wating for registration responses ...");
                        if (Task.WaitAll(HandlerTasks.ToArray(), TimeSpan.FromSeconds(SawtoothConstants.GranularWaitTimeInSeconds)))
                        {
                            foreach (int id in HandlerTasks.Select(x => x.Id))
                            {
                                taskCancellationTokenSources.Remove(id);
                            }
                            log.Info("Received registration responses.");
                            int i = 1;
                            foreach (var task in HandlerTasks)
                            {
                                var response = task.Result;
                                if (response.IsSuccess)
                                {
                                    TpRegisterResponse registration_resp = response.Message.Unwrap<TpRegisterResponse>();
                                    if (registration_resp.Status == TpRegisterResponse.Types.Status.Ok)
                                    {
                                        log.Info("[Handler:{0}] Registration Successful", i);
                                        //Monitor Connectivity
                                        lastPingReceived = DateTime.Now;//Just received a response, assume, this is last ping received.
                                        Task.Run(StayConnected);
                                    }
                                    else
                                    {
                                        log.Info("[Handler:{0}] Registration Unsuccessful", i);
                                        //Retry
                                        Start(true);
                                    }
                                }
                                else
                                {
                                    log.Info("[Handler:{0}] Registration Failed : {1}", i, response.Error);
                                }
                                i++;
                            }
                            break;
                        }
                        else
                        {
                            log.Info("No response received within {0} seconds, still waiting ...", SawtoothConstants.GranularWaitTimeInSeconds);
                        }
                    }
                    HandlerTasks.Clear();
                    trackRegistration.Set();

                });
            }

        }

        /// <summary>
        /// Stops the processor and sends <see cref="TpUnregisterRequest"/> message to the validator
        /// </summary>
        public void Stop()
        {
            log.Info("Stop reqested.");
            stopping = true;
            try
            {
                foreach (var taskCancellationTokenSource in taskCancellationTokenSources)
                {
                    try
                    {
                        //cancel all runnning tasks
                        log.Debug("Cancelling task : {0} ...", taskCancellationTokenSource.Key);
                        taskCancellationTokenSource.Value.Cancel();
                    }
                    catch(Exception e)
                    {
                        //Ignore if cancellation fails - may be tasks already completed
                        log.Info("Cancelling task failed: {0}", e.Message);

                    }
                }
                //Wait for loop to exit
                trackRegistration.WaitOne();

                log.Debug("Sending Ungistration message ...");
                Task<ZMQResponse> unregistration_response = SendAsync(new TpUnregisterRequest().Wrap(MessageType.TpUnregisterRequest), CancellationToken.None, 5);
                Task.WaitAll(new[] { unregistration_response });
                var response = unregistration_response.Result;
                if (response.IsSuccess)
                {
                    TpUnregisterResponse registration_resp = response.Message.Unwrap<TpUnregisterResponse>();
                    if (registration_resp.Status == TpUnregisterResponse.Types.Status.Ok)
                    {
                        log.Info("Unregistration Successful.");
                    }
                    else
                    {
                        log.Info("Unregistration Unsuccessful.");
                    }
                }
                else
                {
                    log.Info("Unregistration call Failed : {0}", response.Error);
                }
                log.Info("Disconnecting ...");
                Disconnect();
                log.Info("Disconnected.");
            }catch(Exception e)
            {
                log.Fatal("Exception : {0}", e.Message);
            }
        }

        public override void OnPingRequest()
        {
            lastPingReceived = DateTime.Now;
        }

        /// <summary>
        /// Processes the received message from the validator
        /// </summary>
        /// <remarks>
        /// Do not call this method directly
        /// </remarks>
        /// <param name="message">Message.</param>
        public override async void OnMessage(Message message)
        {
            
            base.OnMessage(message);

            if (message.MessageType == MessageType.TpProcessRequest)
            {
                var request = message.Unwrap<TpProcessRequest>();
                var handler = Handlers.FirstOrDefault(x => x.FamilyName == request.Header.FamilyName && x.Version == request.Header.FamilyVersion);

                if (handler == null)
                {
                    // This shouldn't ever happen, but if it does, fail gracefully with internal error
                    await ApplyCompletion(Task.FromException(new Exception("Cannot locate handler.")), message);
                    return;
                }

                await handler
                    .ApplyAsync(request, new TransactionContext(this, request.ContextId))
                    .ContinueWith(ApplyCompletion, message);
            }
        }

        /// <summary>
        /// Completes the <see cref="TpProcessRequest"/> message by sending the resulting status code.
        /// </summary>
        /// <returns>The completion.</returns>
        /// <param name="task">Task.</param>
        /// <param name="msg">Message.</param>
        async Task ApplyCompletion(Task task, object? msg)
        {
            var message = msg as Message;

            if (message == null) return;

            switch (task.Status)
            {
                case TaskStatus.RanToCompletion:
                    await SendAsync(new TpProcessResponse { Status = TpProcessResponse.Types.Status.Ok }
                         .Wrap(message, MessageType.TpProcessResponse), CancellationToken.None);
                    break;

                case TaskStatus.Faulted:
                    if (task.Exception != null && task.Exception.InnerException is InvalidTransactionException)
                    {
                        await SendAsync(new TpProcessResponse { Status = TpProcessResponse.Types.Status.InvalidTransaction , Message = task.Exception.InnerException.Message, ExtendedData = ByteString.CopyFrom(task.Exception.Message, Encoding.UTF8) }
                             .Wrap(message, MessageType.TpProcessResponse), CancellationToken.None);
                    }
                    else
                    {
                        await SendAsync(new TpProcessResponse { Status = TpProcessResponse.Types.Status.InternalError, Message = "Internal Error", ExtendedData = ByteString.CopyFrom(task.Exception?.Message??string.Empty, Encoding.UTF8) }
                             .Wrap(message, MessageType.TpProcessResponse), CancellationToken.None);
                    }
                    break;
            }
        }
    }
}
