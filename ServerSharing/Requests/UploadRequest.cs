using Ydb.Sdk.Table;
using Ydb.Sdk.Value;
using System.Text;
using Newtonsoft.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System.Net;
using ServerSharing.Data;

namespace ServerSharing
{
    public class UploadRequest : BaseRequest
    {
        private readonly AmazonDynamoDBClient _awsClient;

        public UploadRequest(AmazonDynamoDBClient awsClient, TableClient tableClient, Request request)
            : base(tableClient, request)
        {
            _awsClient = awsClient;
        }

        protected async override Task<Response> Handle(TableClient client, Request request)
        {
            UploadData uploadData = ParseUploadData(request.body);
            var guid = Guid.NewGuid();

            var awsRequest = new BatchWriteItemRequest(new Dictionary<string, List<WriteRequest>>()
            {
                {
                    Tables.Images, new List<WriteRequest>()
                    {
                        new WriteRequest(new PutRequest()
                        {
                            Item = new Dictionary<string, AttributeValue>
                            {
                                { "id", new AttributeValue { S = guid.ToString()}},
                                { "image", new AttributeValue { B = new MemoryStream(uploadData.Image) }},
                            }
                        })
                    }
                },
                {
                    Tables.Data, new List<WriteRequest>()
                    {
                        new WriteRequest(new PutRequest()
                        {
                            Item = new Dictionary<string, AttributeValue>
                            {
                                { "id", new AttributeValue { S = guid.ToString()}},
                                { "data", new AttributeValue { B = new MemoryStream(uploadData.Data) }},
                            }
                        })
                    }
                }
            });

            var awsResponse = await _awsClient.BatchWriteItemAsync(awsRequest);

            if (awsResponse.HttpStatusCode != HttpStatusCode.OK)
                throw new InvalidOperationException("Error BatchWriteItemAsync: " + awsResponse);

            var response = await client.SessionExec(async session =>
            {
                var query = $@"
                    DECLARE $id AS string;
                    DECLARE $user_id AS string;
                    DECLARE $body AS json;
                    DECLARE $date AS Datetime;

                    UPSERT INTO `{Tables.Records}` (id, user_id, body, date, download_count, like_count, rating_avg, rating_count)
                    VALUES ($id, $user_id, $body, $date, 0u, 0u, 0u, 0u);
                ";

                return await session.ExecuteDataQuery(
                    query: query,
                    txControl: TxControl.BeginSerializableRW().Commit(),
                    parameters: new Dictionary<string, YdbValue>
                    {
                        { "$id", YdbValue.MakeString(Encoding.UTF8.GetBytes(guid.ToString())) },
                        { "$user_id", YdbValue.MakeString(Encoding.UTF8.GetBytes(request.user_id)) },
                        { "$body", YdbValue.MakeJson(JsonConvert.SerializeObject(uploadData.Metadata)) },
                        { "$date", YdbValue.MakeDatetime(DateTime.UtcNow) }
                    }
                );
            });

            return new Response((uint)response.Status.StatusCode, response.Status.StatusCode.ToString(), guid.ToString());
        }

        private UploadData ParseUploadData(string body)
        {
            try
            {
                var uploadData = JsonConvert.DeserializeObject<UploadData>(body);

                return uploadData ?? throw new NullReferenceException("Can't deserialize body");
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException("Request body has invalid format", exception);
            }
        }
    }
}