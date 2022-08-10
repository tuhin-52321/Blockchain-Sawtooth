using PeterO.Cbor;
using Sawtooth.Sdk.Net.Client;
using Sawtooth.Sdk.Net.RESTApi.Payload;
using Sawtooth.Sdk.Net.Transactions;
using Sawtooth.Sdk.Net.Utils;
using System.Text.Json;

namespace Sawtooth.Sdk.Net.RESTApi.Client.Tests
{
    [TestClass()]
    public class TxnFamiliesTest
    {
        [TestMethod("Test Address Prefix for IntKey")]
        public void IntKeyAddressPrefixTest()
        {
            IntKeyAddress address = new IntKeyAddress();

            Assert.AreEqual("1cf126", address.Prefix);
        }

        [TestMethod("Test Address Prefix for XO")]
        public void XOAddressPrefixTest()
        {
            XOAddress address = new XOAddress();

            Assert.AreEqual("5b7349", address.Prefix );
        }

        [TestMethod("Test Address Prefix for Settings")]
        public void SettingsAddressPrefixTest()
        {
            SawtoothSettingsAddress address = new SawtoothSettingsAddress();

            Assert.AreEqual("000000", address.Prefix );
        }


    }
}