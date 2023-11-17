using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ServerSharing.Data
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SelectEntryType
    {
        Downloaded,
        Uploaded,
        Liked,
    }
}