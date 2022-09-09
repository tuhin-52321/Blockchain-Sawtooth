using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sawtooth.Sdk.Net.Messaging
{
    /// <summary>
    /// Stream listener.
    /// </summary>
    public interface IStreamListener
    {
        /// <summary>
        /// Called when a new message is received from the validator
        /// </summary>
        /// <param name="message">Message.</param>
        void OnMessage(Message message);

        void OnPingRequest();

        /// <summary>
        /// Send a message to the validator
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="message">Message.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task<ZMQResponse> SendAsync(Message message, CancellationToken cancellationToken, int timeout_seconds = -1);
    }
}