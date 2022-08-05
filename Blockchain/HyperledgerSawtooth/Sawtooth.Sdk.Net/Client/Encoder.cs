using Sawtooth.Sdk.Net.RESTApi.Payload;
using Sawtooth.Sdk.Net.RESTApi.Payload.Json;
using Sawtooth.Sdk.Net.Utils;

namespace Sawtooth.Sdk.Net.Client
{
    /// <summary>
    /// Encoder.
    /// </summary>
    public class Encoder
    {
        readonly EncoderSettings settings;
        readonly ISigner signer;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Sawtooth.Sdk.Client.Encoder"/> class.
        /// </summary>
        /// <param name="settings">Settings.</param>
        /// <param name="privateKey">Private key.</param>
        public Encoder(EncoderSettings settings, byte[] privateKey)
        {
            this.settings = settings;
            this.signer = new Signer(privateKey);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Sawtooth.Sdk.Client.Encoder"/> class.
        /// </summary>
        /// <param name="settings">Settings.</param>
        /// <param name="signer">Signer.</param>
        public Encoder(EncoderSettings settings, ISigner signer)
        {
            this.settings = settings;
            this.signer = signer;
        }

        /// <summary>
        /// Creates new transaction.
        /// </summary>
        /// <returns>The transaction.</returns>
        /// <param name="payload">Payload.</param>
        public Transaction CreateTransaction(byte[] payload)
        {
            var header = new TransactionHeader();
            header.FamilyName = settings.FamilyName;
            header.FamilyVersion = settings.FamilyVersion;
            header.Inputs.AddRange(settings.Inputs);
            header.Outputs.AddRange(settings.Outputs);
            header.Nonce = Guid.NewGuid().ToString();
            header.SignerPublicKey = settings.SignerPublickey;
            header.BatcherPublicKey = settings.BatcherPublicKey;
            header.PayloadSha512 = payload.ToSha512().ToHexString();

            var transaction = new Transaction();
            transaction.Payload = payload.ToHexString();
            transaction.Header = header;
            transaction.HeaderSignature = signer.Sign(header.ToByteArray().ToSha256()).ToHexString();

            return transaction;
        }




    }
}
