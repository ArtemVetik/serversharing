using Newtonsoft.Json;
using NUnit.Framework;
using ServerSharing.Data;

namespace ServerSharing.Tests
{
    [TestFixture]
    public class Test_012_SelectMetricCountTests
    {
        [SetUp]
        public async Task Setup()
        {
            await CloudFunction.Clear("records");
        }

        [Test]
        public async Task Select_MetricsCount_ShouldReturnCorrectCount()
        {
            var id = await CloudFunction.Upload("user1", new UploadData() { Image = new byte[] { }, Data = new byte[] { } });

            await CloudFunction.Download("test_download_1", id);
            await CloudFunction.Download("test_download_2", id);
            await CloudFunction.Like("test_like_1", id);
            await CloudFunction.Rate("test_Rate_1", id, 3);
            await CloudFunction.Rate("test_Rate_2", id, 4);
            await CloudFunction.Rate("test_Rate_3", id, 5);

            var selectRequestBody = new SelectRequestBody()
            {
                Parameters = new SelectRequestBody.SortParameters()
                {
                    Sort = Sort.Date,
                    Date = DateTime.Now.AddDays(1),
                },
                Limit = 10,
            };

            Response[] responses = new Response[4];
            responses[0] = await CloudFunction.Post(Request.Create("SELECT_SELF", "user1", JsonConvert.SerializeObject(SelectEntryType.Uploaded)));
            responses[1] = await CloudFunction.Post(Request.Create("SELECT_SELF", "test_download_1", JsonConvert.SerializeObject(SelectEntryType.Downloaded)));
            responses[2] = await CloudFunction.Post(Request.Create("SELECT_SELF", "test_like_1", JsonConvert.SerializeObject(SelectEntryType.Liked)));
            responses[3] = await CloudFunction.Post(Request.Create("SELECT", "user", JsonConvert.SerializeObject(selectRequestBody)));

            foreach (var response in responses)
            {
                Assert.True(response.IsSuccess, $"{response.StatusCode}, {response.ReasonPhrase}");
                
                var selectData = JsonConvert.DeserializeObject<List<SelectResponseData>>(response.Body);

                TestContext.WriteLine($"DATA: {selectData.Count}");
                Assert.That(selectData[0].Downloads, Is.EqualTo(2));
                Assert.That(selectData[0].Likes, Is.EqualTo(1));
                Assert.That(selectData[0].RatingCount, Is.EqualTo(3));
                Assert.That(selectData[0].RatingAverage, Is.EqualTo(40000));
            }
        }

        [Test]
        public async Task Select_TwiceDownloadAndLike_ShouldBeNotIncreaseCount()
        {
            var id = await CloudFunction.Upload("user1", new UploadData() { Image = new byte[] { }, Data = new byte[] { } });

            await CloudFunction.Download("test_download_1", id);
            await CloudFunction.Download("test_download_1", id);
            await CloudFunction.Like("test_like_1", id);
            await CloudFunction.Like("test_like_1", id);

            var response = await CloudFunction.Post(Request.Create("SELECT_SELF", "user1", JsonConvert.SerializeObject(SelectEntryType.Uploaded)));

            Assert.True(response.IsSuccess, $"{response.StatusCode}, {response.ReasonPhrase}");

            var selectData = JsonConvert.DeserializeObject<List<SelectResponseData>>(response.Body);

            Assert.That(selectData[0].Downloads, Is.EqualTo(1));
            Assert.That(selectData[0].Likes, Is.EqualTo(1));
        }

        [Test]
        public async Task Select_ChangeRate_ShouldChangeAvg()
        {
            var id = await CloudFunction.Upload("user1", new UploadData() { Image = new byte[] { }, Data = new byte[] { } });

            await CloudFunction.Rate("test_rate_1", id, 3);

            var response = await CloudFunction.Post(Request.Create("SELECT_SELF", "user1", JsonConvert.SerializeObject(SelectEntryType.Uploaded)));
            Assert.That(response.IsSuccess, Is.True);
            var selectData = JsonConvert.DeserializeObject<List<SelectResponseData>>(response.Body);
            Assert.That(selectData[0].RatingCount, Is.EqualTo(1));
            Assert.That(selectData[0].RatingAverage, Is.EqualTo(30000));
            
            await CloudFunction.Rate("test_rate_1", id, 5);

            response = await CloudFunction.Post(Request.Create("SELECT_SELF", "user1", JsonConvert.SerializeObject(SelectEntryType.Uploaded)));
            Assert.That(response.IsSuccess, Is.True);
            selectData = JsonConvert.DeserializeObject<List<SelectResponseData>>(response.Body);
            Assert.That(selectData[0].RatingCount, Is.EqualTo(1));
            Assert.That(selectData[0].RatingAverage, Is.EqualTo(50000));
        }
    }
}