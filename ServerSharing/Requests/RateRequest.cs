using Ydb.Sdk.Table;
using System.Text;
using Ydb.Sdk.Value;
using Newtonsoft.Json;
using ServerSharing.Data;

namespace ServerSharing
{
    public class RateRequest : BaseRequest
    {
        public RateRequest(TableClient tableClient, Request request)
            : base(tableClient, request)
        { }

        protected async override Task<Response> Handle(TableClient client, Request request)
        {
            RatingRequestBody body = default;

            try
            {
                body = JsonConvert.DeserializeObject<RatingRequestBody>(request.body);

                if (body.Rating < 1 || body.Rating > 5)
                    throw new ArgumentOutOfRangeException("The rating has an incorrect value: " + body.Rating);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException("Request body has an invalid format", exception);
            }

            var rated = await AlreadyRated(client, request.user_id, body.Id);
            
            if (rated == false)
                await IncreaseCount(client, body.Id);
            
            var response = await client.SessionExec(async session =>
            {
                var query = $@"
                    DECLARE $user_id AS string;
                    DECLARE $id AS string;
                    DECLARE $rating AS int8;

                    UPSERT INTO `{Tables.Ratings}` (user_id, id, rating)
                    VALUES ($user_id, $id, $rating);

                    $avg_rating = (SELECT CAST(AVG(ratings.rating) * 10000 AS Uint32)
                    FROM `{Tables.Ratings}` VIEW idx_id ratings
                    WHERE ratings.id = $id);

                    UPDATE `{Tables.Records}`
                    SET rating_avg = if (rating_avg is null, $rating, $avg_rating)
                    WHERE id = $id;
                ";

                return await session.ExecuteDataQuery(
                    query: query,
                    txControl: TxControl.BeginSerializableRW().Commit(),
                    parameters: new Dictionary<string, YdbValue>
                    {
                        { "$user_id", YdbValue.MakeString(Encoding.UTF8.GetBytes(request.user_id)) },
                        { "$id", YdbValue.MakeString(Encoding.UTF8.GetBytes(body.Id)) },
                        { "$rating", YdbValue.MakeInt8(body.Rating) },
                    }
                );
            });

            return new Response((uint)response.Status.StatusCode, response.Status.StatusCode.ToString(), string.Empty);
        }

        private async Task<bool> AlreadyRated(TableClient client, string userId, string id)
        {
            var response = await client.SessionExec(async session =>
            {
                var query = $@"
                    DECLARE $user_id AS string;
                    DECLARE $id AS string;

                    SELECT id
                    FROM `{Tables.Ratings}`
                    WHERE user_id = $user_id AND id = $id;
                ";

                return await session.ExecuteDataQuery(
                    query: query,
                    txControl: TxControl.BeginSerializableRW().Commit(),
                    parameters: new Dictionary<string, YdbValue>
                    {
                        { "$user_id", YdbValue.MakeString(Encoding.UTF8.GetBytes(userId)) },
                        { "$id", YdbValue.MakeString(Encoding.UTF8.GetBytes(id)) },
                    }
                );
            });

            var queryResponse = (ExecuteDataQueryResponse)response;
            var resultSet = queryResponse.Result.ResultSets[0];

            var increateRatingCount = string.Empty;

            return resultSet.Rows.Count != 0;
        }

        private async Task IncreaseCount(TableClient client, string id)
        {
            await client.SessionExec(async session =>
            {
                var query = $@"
                    DECLARE $id AS string;

                    UPDATE `{Tables.Records}`
                    SET rating_count = if (rating_count is null, 1u, rating_count + 1u)
                    WHERE id = $id
                ";

                return await session.ExecuteDataQuery(
                    query: query,
                    txControl: TxControl.BeginSerializableRW().Commit(),
                    parameters: new Dictionary<string, YdbValue>
                    {
                        { "$id", YdbValue.MakeString(Encoding.UTF8.GetBytes(id)) },
                    }
                );
            });
        }
    }
}