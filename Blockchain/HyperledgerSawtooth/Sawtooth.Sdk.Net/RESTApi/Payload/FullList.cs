using System.Text.Json.Serialization;

namespace Sawtooth.Sdk.Net.RESTApi.Payload
{
    public class FullList<T>
    {
        public List<T?> List { get; set; } = new List<T?>();

        public string? Head { get; set; }


    }
}
