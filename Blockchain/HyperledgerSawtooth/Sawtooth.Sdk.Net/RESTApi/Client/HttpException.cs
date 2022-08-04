using Sawtooth.Sdk.Net.RESTApi.Payload;
using System.Net;
using System.Runtime.Serialization;

namespace Sawtooth.Sdk.Net.RESTApi.Client
{
    [Serializable]
    public class HttpException : Exception
    {
        public int? Code { get; private set; }
        public string? Title { get; private set; }


        public HttpException(ErrorResponse? response) : base(response?.Error?.Message)
        {
            Code = response?.Error?.Code;
            Title = response?.Error?.Title;
        }

        public HttpException(HttpStatusCode statusCode, string response_text) : base(response_text)
        {
            Code = (int)statusCode;
            Title = statusCode.ToString();
        }
    }
}