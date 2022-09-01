using Sawtooth.Sdk.Net.Messaging;
using Sawtooth.Sdk.Net.Utils;

namespace Sawtooth.Sdk.Net.Client
{
    public class ValidatorStateEventClient : IDisposable
    {
        readonly EventStream Stream;
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Sawtooth.Sdk.Client.ValidatorClient"/> class.
        /// </summary>
        /// <param name="address">Address.</param>
        ValidatorStateEventClient(string address, Action<StateChange> OnStateChange, Action<ClientEventsSubscribeResponse.Types.Status, string> OnError, params string[] address_filter)
        {

            EventSubscription eventSubscription = new EventSubscription
            {
                EventType = "sawtooth/state-delta"
            };

            foreach (string filter in address_filter)
            {
                eventSubscription.Filters.Add(new EventFilter
                {
                    FilterType = global::EventFilter.Types.FilterType.RegexAny,
                    Key = "address",
                    MatchString = filter + ".*"
                });
            }
            ClientEventsSubscribeRequest request = new ClientEventsSubscribeRequest();
            request.Subscriptions.Add(eventSubscription);
            Stream = new EventStream(address, request, OnStateChange, OnError);
        }

        /// <summary>
        /// Creates a <see cref="ValidatorClient"/> instance and connects to the specified address.
        /// Use this inside a <c>using</c> statement.
        /// </summary>
        /// <returns>The create.</returns>
        /// <param name="address">Address.</param>
        public static ValidatorStateEventClient Create(string address, Action<StateChange> OnStateChange, Action<ClientEventsSubscribeResponse.Types.Status, string> OnError, params string[] address_filter)
        {
            return new ValidatorStateEventClient(address, OnStateChange, OnError, address_filter);
        }


        public void Dispose()
        {
            Stream.Disconnect();
        }

    }
}