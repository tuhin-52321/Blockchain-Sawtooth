using System.Text.Json.Serialization;

namespace Sawtooth.Sdk.Net.RESTApi.Payload
{
    public class PageOf<T>
    {
        public List<T?> List { get; set; } = new List<T?>();

        public string? Head { get; set; }

        public string? Next { get; set; } //Start id of Next page, null if no more pages



    }
}
