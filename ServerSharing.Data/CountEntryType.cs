using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ServerSharing.Data
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CountEntryType
    {
        All,
        Downloaded,
        Uploaded,
        Liked,
    }
}