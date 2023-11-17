using Newtonsoft.Json;
using NUnit.Framework;
using ServerSharing.Data;

namespace ServerSharing.Tests
{
    [TestFixture]
    public class Test_013_SelectMy
    {
        [SetUp]
        public async Task Setup()
        {
            await CloudFunction.Clear("records");
            await CloudFunction.Clear("downloads");
            await CloudFunction.Clear("likes");
        }

        [Test]
        public async Task SelectAll_WrongUser_ShouldReturnFalse()
        {
            var id1 = await CloudFunction.Upload("user1", new UploadData() { Data = new byte[] { 0 }, Image = new byte[] { 1 } });
            var id2 = await CloudFunction.Upload("user2", new UploadData() { Data = new byte[] { 2 }, Image = new byte[] { 3 } });

            await CloudFunction.Like("like1", id1);
            await CloudFunction.Like("like2", id2);
            await CloudFunction.Download("download1", id1);
            await CloudFunction.Download("download2", id2);
            await CloudFunction.Rate("rate1", id1, 2);
            await CloudFunction.Rate("rate2", id2, 3);

            var selectRequest = new SelectRequestBody()
            {
                Parameters = new SelectRequestBody.SortParameters()
                {
                    Sort = Sort.Date,
                    Date = DateTime.Now.AddDays(1),
                },
                Limit = 10,
            };

            var response = await CloudFunction.Post(Request.Create("SELECT", "test_user", JsonConvert.SerializeObject(selectRequest)));

            Assert.That(response.IsSuccess, Is.True);

            var selectData = JsonConvert.DeserializeObject<List<SelectResponseData>>(response.Body);

            Assert.That(selectData.First(data => data.Id == id1).MyLike, Is.EqualTo(false));
            Assert.That(selectData.First(data => data.Id == id2).MyLike, Is.EqualTo(false));
            Assert.That(selectData.First(data => data.Id == id1).MyDownload, Is.EqualTo(false));
            Assert.That(selectData.First(data => data.Id == id2).MyDownload, Is.EqualTo(false));
            Assert.That(selectData.First(data => data.Id == id1).MyRecord, Is.EqualTo(false));
            Assert.That(selectData.First(data => data.Id == id2).MyRecord, Is.EqualTo(false));
            Assert.That(selectData.First(data => data.Id == id1).MyRating, Is.Null);
            Assert.That(selectData.First(data => data.Id == id2).MyRating, Is.Null);
        }

        [Test]
        public async Task SelectAll_PartiallyCorrect_ShouldReturnTrueAndFalse()
        {
            var id1 = await CloudFunction.Upload("user1", new UploadData() { Data = new byte[] { 0 }, Image = new byte[] { 1 } });
            var id2 = await CloudFunction.Upload("user2", new UploadData() { Data = new byte[] { 2 }, Image = new byte[] { 3 } });

            await CloudFunction.Like("self_id", id1);
            await CloudFunction.Download("self_id", id2);
            await CloudFunction.Rate("self_id", id1, 4);

            var selectRequest = new SelectRequestBody()
            {
                Parameters = new SelectRequestBody.SortParameters()
                {
                    Sort = Sort.Date,
                    Date = DateTime.Now.AddDays(1),
                },
                Limit = 10,
            };

            var response = await CloudFunction.Post(Request.Create("SELECT", "self_id", JsonConvert.SerializeObject(selectRequest)));

            Assert.That(response.IsSuccess, Is.True);

            var selectData = JsonConvert.DeserializeObject<List<SelectResponseData>>(response.Body);

            Assert.That(selectData.First(data => data.Id == id1).MyLike, Is.EqualTo(true));
            Assert.That(selectData.First(data => data.Id == id2).MyDownload, Is.EqualTo(true));
            Assert.That(selectData.First(data => data.Id == id1).MyRating, Is.EqualTo(4));
        }

        [Test]
        public async Task SelectAll_Uploaded_ShouldReturnTrue()
        {
            var id1 = await CloudFunction.Upload("user1", new UploadData() { Data = new byte[] { 0 }, Image = new byte[] { 1 } });

            var selectRequest = new SelectRequestBody()
            {
                Parameters = new SelectRequestBody.SortParameters()
                {
                    Sort = Sort.Date,
                    Date = DateTime.Now.AddDays(1),
                },
                Limit = 10,
            };

            var response = await CloudFunction.Post(Request.Create("SELECT", "user1", JsonConvert.SerializeObject(selectRequest)));

            Assert.That(response.IsSuccess, Is.True);

            var selectData = JsonConvert.DeserializeObject<List<SelectResponseData>>(response.Body);

            Assert.That(selectData[0].MyRecord, Is.EqualTo(true));
        }
    }
}