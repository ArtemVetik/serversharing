using Newtonsoft.Json;
using System;

namespace ServerSharing.Data
{
    public class SelectRequestBody
    {
        [JsonProperty("sort_parameters")] public SortParameters Parameters { get; set; }
        [JsonProperty("limit")] public ulong Limit { get; set; }

        public class SortParameters
        {
            [JsonProperty("sort")] public Sort Sort { get; set; }
            [JsonProperty("id")] public string Id { get; set; }
            [JsonProperty("date")] public DateTime Date { get; set; }
            [JsonProperty("download_count")] public uint DownloadCount { get; set; }
            [JsonProperty("like_count")] public uint LikeCount { get; set; }
            [JsonProperty("rating_count")] public uint RatingCount { get; set; }
            [JsonProperty("rating_average")] public uint RatingAverage { get; set; }
        }
    }
}