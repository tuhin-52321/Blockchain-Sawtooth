using System.Runtime.Serialization;

namespace Sawtooth.Sdk.Net.Utils
{
    [Serializable]
    internal class InternalErrorException : Exception
    {
        public InternalErrorException()
        {
        }

        public InternalErrorException(string? message) : base(message)
        {
        }

        public InternalErrorException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected InternalErrorException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}