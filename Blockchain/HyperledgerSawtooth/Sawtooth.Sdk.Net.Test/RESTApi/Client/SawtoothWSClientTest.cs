using PeterO.Cbor;
using Sawtooth.Sdk.Net.Client;
using Sawtooth.Sdk.Net.RESTApi.Payload;
using Sawtooth.Sdk.Net.Test.RESTApi.WebSocket;
using Sawtooth.Sdk.Net.Utils;
using System.Text.Json;

namespace Sawtooth.Sdk.Net.RESTApi.Client.Tests
{
    [TestClass()]
    public class SawtoothWSClientTest
    {
        private string ToJson<T>(T? json)
        {
            if (json != null)
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                return JsonSerializer.Serialize(json, options);
            }

            return "<Null>";
        }


        [TestMethod("WS Client")]
        public void WSCleintTest()
        {

            using(var client = new SawtoothWSClient("ws://localhost:8008/subscriptions", e => Console.WriteLine(ToJson(e)), "1cf126"))
            {
                Thread.Sleep(10000);
            }
            

        }

    }
}