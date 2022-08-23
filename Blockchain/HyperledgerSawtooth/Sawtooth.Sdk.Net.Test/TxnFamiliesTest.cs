using PeterO.Cbor;
using Sawtooth.Sdk.Net.Client;
using Sawtooth.Sdk.Net.RESTApi.Payload;
using Sawtooth.Sdk.Net.RESTApi.Payload.Protobuf;
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
            IntKeyTransactionFamily family = new IntKeyTransactionFamily();

            Assert.AreEqual("1cf126", family.AddressPrefix);
        }

        [TestMethod("Test Address Prefix for XO")]
        public void XOAddressPrefixTest()
        {
            XOTransactionFamily family = new XOTransactionFamily();

            Assert.AreEqual("5b7349", family.AddressPrefix);
        }

        [TestMethod("Test Address Prefix for Settings")]
        public void SettingsAddressPrefixTest()
        {
            SawtoothSettingsTransactionFamily family = new SawtoothSettingsTransactionFamily();

            Assert.AreEqual("000000", family.AddressPrefix);
        }

        [TestMethod("Test Address Prefix for Smallbank")]
        public void SmallbankAddressPrefixTest()
        {
            SmallbankTransactionFamily family = new SmallbankTransactionFamily();

            Assert.AreEqual("332514", family.AddressPrefix);
        }

        [TestMethod("Test Address for IntKey")]
        public void IntKeyAddressTest()
        {
            IntKeyTransactionFamily family = new IntKeyTransactionFamily();

            Assert.AreEqual("1cf126cc488cca4cc3565a876f6040f8b73a7b92475be1d0b1bc453f6140fba7183b9a", family.AddressPrefix + family.AddressSuffix("name"));
        }

        [TestMethod("Test Address Prefix for XO")]
        public void XOAddressTest()
        {
            XOTransactionFamily family = new XOTransactionFamily();

            Assert.AreEqual("5b7349700e158b598043efd6d7610345a75a00b22ac14c9278db53f586179a92b72fbd", family.AddressPrefix + family.AddressSuffix("mygame"));
        }

        [TestMethod("Test Address for Settings")]
        public void SettingsAddressTest()
        {
            SawtoothSettingsTransactionFamily family = new SawtoothSettingsTransactionFamily();

            Assert.AreEqual("000000a87cb5eafdcca6a8b79606fb3afea5bdab274474a6aa82c1c0cbf0fbcaf64c0b", family.AddressPrefix + family.AddressSuffix("sawtooth.config.vote.proposals"));
        }

        [TestMethod("Test Address for Smallbank")]
        public void SmallbankAddressTest()
        {
            SmallbankTransactionFamily family = new SmallbankTransactionFamily();

            Assert.AreEqual("3325143ff98ae73225156b2c6c9ceddbfc16f5453e8fa49fc10e5d96a3885546a46ef4", family.AddressPrefix + family.AddressSuffix("42"));
        }

        [TestMethod("Test serializtion/deserialization")]
        public void SmallbankTxnSerialization()
        {
            var txn1 = SmallbankTransaction.CreateAccountTransaction(3, "Tuhin", 5, 10);

            var family1 = new SmallbankTransactionFamily();

            byte[] marshaled = family1.WrapTxnPayload(txn1);

            var family2 = new SmallbankTransactionFamily();

            var txn2 = family2.UnwrapTxnPayload(marshaled);

            Console.WriteLine(txn2.DisplayString);

            Assert.AreEqual(txn1.DisplayString, txn2?.DisplayString);
        }


    }
}