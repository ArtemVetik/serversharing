using Newtonsoft.Json;
using ServerSharing.Data;
using System.Text;
using Ydb.Sdk.Value;

namespace ServerSharing
{
    internal static class SelectExtentions
    {
        public static SelectResponseData CreateResponseData(this ResultSet.Row row)
        {
            var downloadsCount = row["download_count"];
            var likesCount = row["like_count"];
            var ratingsCount = row["rating_count"];
            var ratingsAverage = row["rating_avg"];
            var myRating = row["my_rating"];
            var myLike = row["my_like"];
            var myDownload = row["my_download"];
            var myRecord = row["my_record"];

            return new SelectResponseData()
            {
                Id = Encoding.UTF8.GetString(row["id"].GetString()),
                Metadata = JsonConvert.DeserializeObject<RecordMetadata>(row["body"].GetOptionalJson()),
                Datetime = row["date"].GetOptionalDatetime() ?? DateTime.MinValue,
                Downloads = downloadsCount.TypeId == YdbTypeId.OptionalType ? (downloadsCount.GetOptionalUint32() ?? 0) : downloadsCount.GetUint32(),
                Likes = likesCount.TypeId == YdbTypeId.OptionalType ? (likesCount.GetOptionalUint32() ?? 0) : likesCount.GetUint32(),
                RatingCount = ratingsCount.TypeId == YdbTypeId.OptionalType ? (ratingsCount.GetOptionalUint32() ?? 0) : ratingsCount.GetUint32(),
                RatingAverage = ratingsAverage.TypeId == YdbTypeId.OptionalType ? (ratingsAverage.GetOptionalUint32() ?? 0) : ratingsAverage.GetUint32(),
                MyRating = myRating.GetOptionalInt8(),
                MyLike = myLike.GetOptional() != null && myLike.GetOptional().GetBool(),
                MyDownload = myDownload.GetOptional() != null && myDownload.GetOptional().GetBool(),
                MyRecord = myRecord.GetBool(),
            };
        }

        public static SelectRequestBody ParseSelectRequestBody(this string body)
        {
            try
            {
                var selectData = JsonConvert.DeserializeObject<SelectRequestBody>(body);

                if (Enum.IsDefined(typeof(Sort), selectData.Parameters.Sort) == false)
                    throw new ArgumentException($"Request is missing {nameof(selectData.Parameters.Sort)} parameter");

                return selectData;
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException("Request body has an invalid format", exception);
            }
        }
    }
}