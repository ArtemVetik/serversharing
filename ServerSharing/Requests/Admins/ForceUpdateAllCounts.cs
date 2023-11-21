#if ADMIN_ENVIRONMENT
using ServerSharing.Data;
using Ydb.Sdk.Table;

namespace ServerSharing
{
    public class ForceUpdateAllCounts : BaseRequest
    {
        public ForceUpdateAllCounts(TableClient tableClient, Request request) : base(tableClient, request)
        { }

        protected async override Task<Response> Handle(TableClient client, Request request)
        {
            var response = await client.SessionExec(async session =>
            {
                var query = $@" $download_count = (SELECT id, CAST(COUNT(downloads.id) AS Uint32?) AS count
                                FROM `{Tables.Downloads}` VIEW idx_id downloads
                                GROUP BY id);

                                $like_count = (SELECT id, CAST(COUNT(likes.id) AS Uint32?) AS count
                                                FROM `{Tables.Likes}` VIEW idx_id likes
                                                GROUP BY id);

                                $rating = (SELECT id, CAST(COUNT(ratings.id) AS Uint32?) AS count,
                                                        CAST(AVG(ratings.rating) * 10000 AS Uint32) AS avg
                                                FROM `{Tables.Ratings}` VIEW idx_id ratings
                                                GROUP BY id);

                                UPDATE `{Tables.Records}` ON
                                SELECT records.id AS id, records.user_id AS user_id, downloads.count AS download_count,
                                likes.count AS like_count, rating.count AS rating_count, rating.avg AS rating_avg
                                FROM `{Tables.Records}` AS records
                                LEFT JOIN $download_count AS downloads ON downloads.id = records.id
                                LEFT JOIN $like_count AS likes ON likes.id = records.id
                                LEFT JOIN $rating AS rating ON rating.id = records.id
                            ";

                return await session.ExecuteDataQuery(
                    query: query,
                    txControl: TxControl.BeginSerializableRW().Commit()
                );
            });

            return new Response((uint)response.Status.StatusCode, response.Status.StatusCode.ToString(), string.Empty);
        }
    }
}
#endif