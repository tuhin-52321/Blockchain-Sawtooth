using Sawtooth.Sdk.Net.RESTApi.Payload.Json;
using System.Text;
using System.Text.Json;
using Websocket.Client;

namespace Sawtooth.Sdk.Net.Test.RESTApi.WebSocket
{
    public class SawtoothWSClient : IDisposable
    {

        WebsocketClient client;

        public SawtoothWSClient(string address, Action<WSEvent?> OnMessage, params string[] address_prefix)
        {

            client = new WebsocketClient(new Uri(address));

            client.ReconnectTimeout = TimeSpan.FromSeconds(30);
            client.ReconnectionHappened.Subscribe(info => Subscribe(address_prefix));

            client.MessageReceived.Subscribe(msg => OnMessage(JsonSerializer.Deserialize<WSEvent>(msg.Text)));

            client.Start();

            Task.Run(() => Subscribe(address_prefix));
        }

        public void Dispose()
        {
            UnSubscribe();
            client.Dispose();
        }

        public void Subscribe(params string[] address_prefix)
        {
            WSSubscribe subscribe = new WSSubscribe();
            foreach(string prefix in address_prefix)
            {
                subscribe.AddressPrefixes.Add(prefix);
            }

            string msg = JsonSerializer.Serialize(subscribe);

            client.Send(msg);
        }

        public void UnSubscribe()
        {
            WSUnSubscribe unsubscribe = new WSUnSubscribe();

            string msg = JsonSerializer.Serialize(unsubscribe);

            client.Send(msg);
        }

    }
}
