using Ydb.Sdk.Table;
using ServerSharing.Data;

namespace ServerSharing
{
    public class CountRequest : BaseRequest
    {
        public CountRequest(TableClient tableClient, Request request)
            : base(tableClient, request)
        { }

        protected async override Task<Response> Handle(TableClient client, Request request)
        {
            var response = await client.SessionExec(async session =>
            {
                var query = $@"
                    SELECT COUNT(*) as count
                    FROM `{Tables.Records}` as records
                ";

                return await session.ExecuteDataQuery(
                    query: query,
                    txControl: TxControl.BeginSerializableRW().Commit()
                );
            });

            if (response.Status.IsSuccess == false)
                return new Response((uint)response.Status.StatusCode, response.Status.StatusCode.ToString(), string.Empty);

            var queryResponse = (ExecuteDataQueryResponse)response;
            var count = queryResponse.Result.ResultSets[0].Rows[0]["count"].GetUint64();

            return new Response((uint)response.Status.StatusCode, response.Status.StatusCode.ToString(), count.ToString());
        }
    }
}