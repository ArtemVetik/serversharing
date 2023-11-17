using Newtonsoft.Json;
using NUnit.Framework;
using ServerSharing.Data;

namespace ServerSharing.Tests
{
    [TestFixture]
    public class Test_009_CountTests
    {
        [SetUp]
        public async Task Setup()
        {
            await CloudFunction.Clear("records");
            await CloudFunction.Clear("downloads");
            await CloudFunction.Clear("likes");
        }

        [Test]
        public async Task Count_AllCount_CountAfterAdd_ShouldCorrectCount()
        {
            var response = await CloudFunction.Post(Request.Create("COUNT", "user1", JsonConvert.SerializeObject(CountEntryType.All)));
            Assert.That(response.IsSuccess, Is.True);
            Assert.That(response.Body, Is.EqualTo("0"));

            await CloudFunction.Upload("test_user_1", new UploadData() { Image = new byte[] { 1 }, Data = new byte[] { 2 } });

            response = await CloudFunction.Post(Request.Create("COUNT", "user2", JsonConvert.SerializeObject(CountEntryType.All)));
            Assert.That(response.IsSuccess, Is.True);
            Assert.That(response.Body, Is.EqualTo("1"));

            await CloudFunction.Upload("test_user_2", new UploadData() { Image = new byte[] { 1 }, Data = new byte[] { 2 } });

            response = await CloudFunction.Post(Request.Create("COUNT", "user3", JsonConvert.SerializeObject(CountEntryType.All)));
            Assert.That(response.IsSuccess, Is.True);
            Assert.That(response.Body, Is.EqualTo("2"));
        }

        [Test]
        public async Task Count_DownloadedCount_UploadAndDelete_ShouldCorrectCount()
        {
            var id = await CloudFunction.Upload("test_user_2", new UploadData() { Image = new byte[] { 1 }, Data = new byte[] { 2 } });
            await CloudFunction.Download("abracadabra", id);

            var response = await CloudFunction.Post(Request.Create("COUNT", "abracadabra", JsonConvert.SerializeObject(CountEntryType.Downloaded)));
            Assert.That(response.IsSuccess, Is.True);
            Assert.That(response.Body, Is.EqualTo("1"));

            await CloudFunction.Delete("test_user_2", id);

            response = await CloudFunction.Post(Request.Create("COUNT", "abracadabra", JsonConvert.SerializeObject(CountEntryType.Downloaded)));
            Assert.That(response.IsSuccess == true, Is.True);
            Assert.That(response.Body, Is.EqualTo("0"));
        }

        [Test]
        public async Task Count_DownloadedCount_DifferentUsers_ShouldCorrectCount()
        {
            var id1 = await CloudFunction.Upload("test_user_1", new UploadData() { Image = new byte[] { 1 }, Data = new byte[] { 2 } });
            var id2 = await CloudFunction.Upload("test_user_2", new UploadData() { Image = new byte[] { 1 }, Data = new byte[] { 2 } });
            await CloudFunction.Download("some_user_1", id1);
            await CloudFunction.Download("some_user_2", id1);
            await CloudFunction.Download("some_user_2", id2);
            await CloudFunction.Download("some_user_3", id2);
            await CloudFunction.Download("abracadabra", id2);

            var response = await CloudFunction.Post(Request.Create("COUNT", "abracadabra", JsonConvert.SerializeObject(CountEntryType.Downloaded)));
            Assert.That(response.IsSuccess, Is.True);
            Assert.That(response.Body, Is.EqualTo("1"));

            response = await CloudFunction.Post(Request.Create("COUNT", "some_user_2", JsonConvert.SerializeObject(CountEntryType.Downloaded)));
            Assert.That(response.IsSuccess, Is.True);
            Assert.That(response.Body, Is.EqualTo("2"));
        }

        [Test]
        public async Task Count_TwiseRequest_ShouldSameData()
        {
            static async Task CountRequest(string userId, string expectingResult)
            {
                var response = await CloudFunction.Post(Request.Create("COUNT", userId, JsonConvert.SerializeObject(CountEntryType.Downloaded)));

                Assert.That(response.IsSuccess, Is.True);
                Assert.That(response.Body, Is.EqualTo(expectingResult));
            }

            var userId = "abracadabra";

            var id2 = await CloudFunction.Upload("test_user_2", new UploadData() { Image = new byte[] { 1 }, Data = new byte[] { 2 } });
            await CloudFunction.Download(userId, id2);

            await CountRequest(userId, "1");
            await CountRequest(userId, "1");
        }
    }
}