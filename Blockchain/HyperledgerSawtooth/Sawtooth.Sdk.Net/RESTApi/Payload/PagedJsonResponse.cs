using System.Text.Json.Serialization;


namespace Sawtooth.Sdk.Net.RESTApi.Payload
{
    public class PagedJsonResponse<T> : CommonJsonResponse
    {
        [JsonPropertyName("data")]
        public List<T?> Data { get; set; } = new List<T?>();

        [JsonPropertyName("paging")]
        public Paging? Paging { get; set; }
    }
}
