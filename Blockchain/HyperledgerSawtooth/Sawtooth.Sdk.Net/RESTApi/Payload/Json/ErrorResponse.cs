using System.Text.Json.Serialization;

namespace Sawtooth.Sdk.Net.RESTApi.Payload.Json
{
    public class ErrorResponse
    {
        [JsonPropertyName("error")]
        public Error? Error { get; set; }
    }
    public class Error
    {
        [JsonPropertyName("code")]
        public int? Code { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }
}
