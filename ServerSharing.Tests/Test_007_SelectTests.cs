using Newtonsoft.Json;
using NUnit.Framework;
using ServerSharing.Data;
using System.Text;

namespace ServerSharing.Tests
{
    [TestFixture]
    public class Test_007_SelectTests
    {
        private List<string> _idList = new List<string>();
        
        [OneTimeSetUp]
        public async Task Setup()
        {
            var uploadData = new UploadData()
            {
                Metadata = new RecordMetadata() { Name = "name", Description = "empty" },
                Image = new byte[0],
                Data = Encoding.UTF8.GetBytes("some_date"),
            };

            await CloudFunction.Clear("records");

            _idList.Add(await CloudFunction.Upload("user0", uploadData));
            await Task.Delay(1000);
            _idList.Add(await CloudFunction.Upload("user1", uploadData));
            await Task.Delay(1000);
            _idList.Add(await CloudFunction.Upload("user2", uploadData));
            await Task.Delay(1000);
            _idList.Add(await CloudFunction.Upload("user3", uploadData));
            await Task.Delay(1000);
            _idList.Add(await CloudFunction.Upload("user4", uploadData));
            await Task.Delay(1000);
            _idList.Add(await CloudFunction.Upload("user5", uploadData));
            await Task.Delay(1000);
            _idList.Add(await CloudFunction.Upload("user6", uploadData));
            await Task.Delay(1000);
            _idList.Add(await CloudFunction.Upload("user7", uploadData));
            await Task.Delay(1000);
            _idList.Add(await CloudFunction.Upload("user8", uploadData));
            await Task.Delay(1000);
            _idList.Add(await CloudFunction.Upload("user9", uploadData));
        }

        [Test]
        public async Task Select_SortByDate_ShouldCorrectData()
        {
            var selectRequest = new SelectRequestBody()
            {
                Parameters = new SelectRequestBody.SortParameters()
                {
                    Sort = Sort.Date,
                    Date = DateTime.Now.AddDays(1)
                },
                Limit = 5,
            };

            var response = await CloudFunction.Post(Request.Create("SELECT", "user0", JsonConvert.SerializeObject(selectRequest)));

            Assert.That(response.IsSuccess, Is.True);

            var selectData = JsonConvert.DeserializeObject<List<SelectResponseData>>(response.Body);
            Assert.That(selectData.Count, Is.EqualTo(5));
            Assert.That(selectData[0].Id, Is.EqualTo(_idList[9]));
            Assert.That(selectData[1].Id, Is.EqualTo(_idList[8]));
            Assert.That(selectData[4].Id, Is.EqualTo(_idList[5]));
        }

        [Test]
        public async Task Select_SortByDownloadCount_ShouldCorrectData()
        {
            await CloudFunction.Download("test_user_1", _idList[5]);
            await CloudFunction.Download("test_user_1", _idList[2]);
            await CloudFunction.Download("test_user_2", _idList[5]);
            
            var selectRequest = new SelectRequestBody()
            {
                Parameters = new SelectRequestBody.SortParameters()
                {
                    Sort = Sort.Downloads,
                    Date = DateTime.Now.AddDays(1),
                    DownloadCount = uint.MaxValue,
                },
                Limit = 5,
            };

            var response = await CloudFunction.Post(Request.Create("SELECT", "user0", JsonConvert.SerializeObject(selectRequest)));

            Assert.That(response.IsSuccess, Is.True);

            var selectData = JsonConvert.DeserializeObject<List<SelectResponseData>>(response.Body);

            Assert.That(selectData.Count, Is.EqualTo(5));
            Assert.That(selectData[0].Id, Is.EqualTo(_idList[5]));
            Assert.That(selectData[1].Id, Is.EqualTo(_idList[2]));
        }

        [Test]
        public async Task Select_SortByLikeCount_ShouldCorrectData()
        {
            await CloudFunction.Like("test_user_1", _idList[5]);
            await CloudFunction.Like("test_user_1", _idList[2]);
            await CloudFunction.Like("test_user_2", _idList[5]);

            var selectRequest = new SelectRequestBody()
            {
                Parameters = new SelectRequestBody.SortParameters()
                {
                    Sort = Sort.Likes,
                    Date = DateTime.Now.AddDays(1),
                    LikeCount = uint.MaxValue
                },
                Limit = 5,
            };

            var response = await CloudFunction.Post(Request.Create("SELECT", "user0", JsonConvert.SerializeObject(selectRequest)));

            Assert.That(response.IsSuccess, Is.True);

            var selectData = JsonConvert.DeserializeObject<List<SelectResponseData>>(response.Body);

            Assert.That(selectData.Count, Is.EqualTo(5));
            Assert.That(selectData[0].Id, Is.EqualTo(_idList[5]));
            Assert.That(selectData[1].Id, Is.EqualTo(_idList[2]));
        }

        [Test]
        public async Task Select_SortByRatingAverage_ShouldCorrectData()
        {
            await CloudFunction.Rate("test_user_1", _idList[5], 5);
            await CloudFunction.Rate("test_user_1", _idList[2], 3);
            await CloudFunction.Rate("test_user_2", _idList[5], 4);

            var selectRequest = new SelectRequestBody()
            {
                Parameters = new SelectRequestBody.SortParameters()
                {
                    Sort = Sort.RaingAverage,
                    Date = DateTime.Now.AddDays(1),
                    RatingAverage = 50000,
                    RatingCount = uint.MaxValue,
                },
                Limit = 5,
            };

            var response = await CloudFunction.Post(Request.Create("SELECT", "user0", JsonConvert.SerializeObject(selectRequest)));

            Assert.That(response.IsSuccess, Is.True);

            var selectData = JsonConvert.DeserializeObject<List<SelectResponseData>>(response.Body);

            Assert.That(selectData.Count, Is.EqualTo(5));
            Assert.That(selectData[0].Id, Is.EqualTo(_idList[5]));
            Assert.That(selectData[1].Id, Is.EqualTo(_idList[2]));
        }
    }
}