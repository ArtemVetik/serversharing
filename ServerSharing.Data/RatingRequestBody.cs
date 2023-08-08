﻿using Newtonsoft.Json;

namespace ServerSharing.Data
{
    public class RatingRequestBody
    {
        [JsonProperty("id")] public string Id { get; init; }
        [JsonProperty("rating")] public sbyte Rating { get; init; }
    }
}