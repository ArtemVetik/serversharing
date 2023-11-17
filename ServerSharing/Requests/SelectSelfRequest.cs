using Newtonsoft.Json;
using ServerSharing.Data;
using System.Text;
using Ydb.Sdk.Table;
using Ydb.Sdk.Value;

namespace ServerSharing
{
    public partial class SelectSelfRequest : BaseRequest
    {
        public SelectSelfRequest(TableClient tableClient, Request request)
            : base(tableClient, request)
        { }

        protected async override Task<Response> Handle(TableClient client, Request request)
        {
            var entryType = JsonConvert.DeserializeObject<SelectEntryType>(request.body);

            if (Enum.IsDefined(typeof(SelectEntryType), entryType) == false)
                throw new ArgumentException($"Request is missing {nameof(entryType)} parameter");

            var response = await client.SessionExec(async session =>
            {
                var query = $@" DECLARE $user_id AS String;

                                $data = ({CreateQuery(entryType)});
                                
                                $downloads = (SELECT id, true AS my_download
                                FROM `{Tables.Downloads}`
                                WHERE user_id = $user_id);

                                $likes = (SELECT id, true AS my_like
                                FROM `{Tables.Likes}`
                                WHERE user_id = $user_id);

                                $rating = (SELECT id, rating as my_rating
                                FROM `{Tables.Ratings}`
                                WHERE user_id = $user_id);

                                SELECT data.*, downloads.*, likes.*, rating.*, if (user_id = $user_id, true, false) as my_record
                                FROM $data AS data
                                LEFT JOIN $downloads AS downloads ON downloads.id = data.id
                                LEFT JOIN $likes AS likes ON likes.id = data.id
                                LEFT JOIN $rating AS rating ON rating.id = data.id;
                            ";

                return await session.ExecuteDataQuery(
                    query: query,
                    txControl: TxControl.BeginSerializableRW().Commit(),
                    parameters: new Dictionary<string, YdbValue>
                    {
                        { "$user_id", YdbValue.MakeString(Encoding.UTF8.GetBytes(request.user_id)) }
                    }
                );
            });

            if (response.Status.IsSuccess == false)
                return new Response((uint)response.Status.StatusCode, response.Status.StatusCode.ToString(), string.Empty);

            var queryResponse = (ExecuteDataQueryResponse)response;
            var resultSet = queryResponse.Result.ResultSets[0].Rows;

            var responseData = new List<SelectResponseData>();

            foreach (var row in resultSet)
                responseData.Add(row.CreateResponseData());

            return new Response((uint)Ydb.Sdk.StatusCode.Success, Ydb.Sdk.StatusCode.Success.ToString(), JsonConvert.SerializeObject(responseData));
        }

        private static string CreateQuery(SelectEntryType type)
        {
            return type switch
            {
                SelectEntryType.Downloaded => 
                        $@" SELECT records.*
                        FROM (SELECT id
                            FROM `{Tables.Downloads}`
                            WHERE user_id = $user_id) AS my_downloads
                        INNER JOIN `{Tables.Records}` AS records ON records.id = my_downloads.id",
                SelectEntryType.Uploaded => 
                        $@" SELECT records.*
                        FROM `{Tables.Records}` VIEW idx_user_id AS records
                        WHERE user_id = $user_id",
                SelectEntryType.Liked => 
                        $@" SELECT records.*
                        FROM (SELECT id
                            FROM `{Tables.Likes}`
                            WHERE user_id = $user_id) AS my_likes
                        INNER JOIN `{Tables.Records}` AS records ON records.id = my_likes.id",
                _ => string.Empty,
            };
        }
    }
}