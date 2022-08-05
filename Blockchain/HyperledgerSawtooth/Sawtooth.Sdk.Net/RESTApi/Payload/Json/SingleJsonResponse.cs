using System.Text.Json.Serialization;


namespace Sawtooth.Sdk.Net.RESTApi.Payload.Json
{
    public class SingleJsonResponse<T> : CommonJsonResponse
    {
        [JsonPropertyName("data")]
        public T? Data { get; set; }
    }
}
