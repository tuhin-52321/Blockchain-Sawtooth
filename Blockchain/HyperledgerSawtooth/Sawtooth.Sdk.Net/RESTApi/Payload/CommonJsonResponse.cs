﻿using System.Text.Json.Serialization;


namespace Sawtooth.Sdk.Net.RESTApi.Payload
{
    public class CommonJsonResponse
    {
        [JsonPropertyName("head")]
        public string? Head { get; set; }

        [JsonPropertyName("link")]
        public string? Link { get; set; }
    }
}
