using Ydb.Sdk.Table;
using ServerSharing.Data;
using Newtonsoft.Json;
using System.Text;
using Ydb.Sdk.Value;

namespace ServerSharing
{
    public class CountRequest : BaseRequest
    {
        public CountRequest(TableClient tableClient, Request request)
            : base(tableClient, request)
        { }

        protected async override Task<Response> Handle(TableClient client, Request request)
        {
            CountEntryType type;
            try
            {
                type = JsonConvert.DeserializeObject<CountEntryType>(request.body);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException("Request body has an invalid format", exception);
            }

            var response = await client.SessionExec(async session =>
            {
                var queryBody = type switch
                {
                    CountEntryType.All => All(),
                    CountEntryType.Downloaded => Downloaded(),
                    CountEntryType.Uploaded => Uploaded(),
                    CountEntryType.Liked => Liked(),
                    _ => throw new ArgumentOutOfRangeException(),
                };

                var query = $@"
                    DECLARE $user_id AS string;
                    {queryBody}
                ";

                return await session.ExecuteDataQuery(
                    query: query,
                    txControl: TxControl.BeginSerializableRW().Commit(),
                    parameters: new Dictionary<string, YdbValue>
                    {
                        { "$user_id", YdbValue.MakeString(Encoding.UTF8.GetBytes(request.user_id)) },
                    }
                );
            });

            if (response.Status.IsSuccess == false)
                return new Response((uint)response.Status.StatusCode, response.Status.StatusCode.ToString(), string.Empty);

            var queryResponse = (ExecuteDataQueryResponse)response;
            var count = queryResponse.Result.ResultSets[0].Rows[0]["count"].GetUint64();

            return new Response((uint)response.Status.StatusCode, response.Status.StatusCode.ToString(), count.ToString());
        }

        private static string All()
        {
            return $@"
                    SELECT COUNT(*) as count
                    FROM `{Tables.Records}` as records";
        }

        private static string Uploaded()
        {
            return $@"
                    SELECT COUNT(*) as count
                    FROM `{Tables.Records}` VIEW idx_user_id as records
                    where records.user_id == $user_id";
        }

        private static string Downloaded()
        {
            return $@"
                    $records = (SELECT records.id
                    FROM `{Tables.Records}` records
                    ORDER BY records.id);

                    SELECT COUNT(*) as count
                    FROM `{Tables.Downloads}` as downloads
                    INNER JOIN $records as records on records.id == downloads.id
                    where user_id == $user_id";
        }

        private static string Liked()
        {
            return $@"
                    $records = (SELECT records.id
                    FROM `{Tables.Records}` records
                    ORDER BY records.id);

                    SELECT COUNT(*) as count
                    FROM `{Tables.Likes}` as likes
                    INNER JOIN $records as records on records.id == likes.id
                    where user_id == $user_id";
        }
    }
}