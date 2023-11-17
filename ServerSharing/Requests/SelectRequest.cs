using Newtonsoft.Json;
using Ydb.Sdk.Table;
using ServerSharing.Data;
using Ydb.Sdk.Value;
using System.Text;

namespace ServerSharing
{
    public partial class SelectRequest : BaseRequest
    {
        public SelectRequest(TableClient tableClient, Request request)
            : base(tableClient, request)
        { }

        protected async override Task<Response> Handle(TableClient client, Request request)
        {
            var selectParameters = SelectExtentions.ParseSelectRequestBody(request.body);

            var sort = CreateSort(selectParameters.Parameters);
            var whereList = sort.Where();

            var rows = new List<ResultSet.Row>();

            for (int i = 0; i < whereList.Length; i++)
            {
                if ((ulong)rows.Count >= selectParameters.Limit)
                    break;

                var response = await client.SessionExec(async session =>
                {
                    var query = $@" DECLARE $user_id AS String;
                                DECLARE $limit AS Uint64;

                                $data = (SELECT records.*
                                FROM `{Tables.Records}` VIEW {sort.View()} AS records
                                {whereList[i]}
                                {sort.OrderBy()}
                                LIMIT $limit);
                                
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
                            { "$user_id", YdbValue.MakeString(Encoding.UTF8.GetBytes(request.user_id)) },
                            { "$limit", YdbValue.MakeUint64(selectParameters.Limit - (ulong)rows.Count) }
                        }
                    );
                });

                if (response.Status.IsSuccess == false)
                    return new Response((uint)response.Status.StatusCode, response.Status.StatusCode.ToString(), string.Empty);

                var queryResponse = (ExecuteDataQueryResponse)response;
                var resultSet = queryResponse.Result.ResultSets[0].Rows;

                rows.AddRange(resultSet);
            }

            var responseData = new List<SelectResponseData>();

            foreach (var row in rows)
                responseData.Add(row.CreateResponseData());

            return new Response((uint)Ydb.Sdk.StatusCode.Success, Ydb.Sdk.StatusCode.Success.ToString(), JsonConvert.SerializeObject(responseData));
        }

        private static ISortParameter CreateSort(SelectRequestBody.SortParameters parameters)
        {
            return parameters.Sort switch
            {
                Sort.Date => new DateSort(parameters.Date, parameters.Id),
                Sort.Downloads => new DownloadCountSort(parameters.DownloadCount, parameters.Date, parameters.Id),
                Sort.Likes => new LikeCountSort(parameters.LikeCount, parameters.Date, parameters.Id),
                Sort.RaingAverage => new RatingAverageSort(parameters.RatingAverage, parameters.RatingCount, parameters.Date, parameters.Id),
                _ => throw new ArgumentOutOfRangeException($"Not found sort {parameters.Sort}"),
            };
        }
    }
}