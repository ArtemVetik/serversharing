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
            var response = await CloudFunction.Post(Request.Create("COUNT", "", ""));
            Assert.That(response.IsSuccess, Is.True);
            Assert.That(response.Body, Is.EqualTo("0"));

            await CloudFunction.Upload("test_user_1", new UploadData() { Image = new byte[] { 1 }, Data = new byte[] { 2 } });

            response = await CloudFunction.Post(Request.Create("COUNT", "", ""));
            Assert.That(response.IsSuccess, Is.True);
            Assert.That(response.Body, Is.EqualTo("1"));

            await CloudFunction.Upload("test_user_2", new UploadData() { Image = new byte[] { 1 }, Data = new byte[] { 2 } });

            response = await CloudFunction.Post(Request.Create("COUNT", "", ""));
            Assert.That(response.IsSuccess, Is.True);
            Assert.That(response.Body, Is.EqualTo("2"));
        }

        [Test]
        public async Task Count_UploadAndDelete_ShouldCorrectCount()
        {
            var id = await CloudFunction.Upload("test_user_2", new UploadData() { Image = new byte[] { 1 }, Data = new byte[] { 2 } });

            var response = await CloudFunction.Post(Request.Create("COUNT", "", ""));
            Assert.That(response.IsSuccess, Is.True);
            Assert.That(response.Body, Is.EqualTo("1"));

            await CloudFunction.Delete("test_user_2", id);

            response = await CloudFunction.Post(Request.Create("COUNT", "", ""));
            Assert.That(response.IsSuccess, Is.EqualTo(true));
            Assert.That(response.Body, Is.EqualTo("0"));
        }

        [Test]
        public async Task Count_TwiseRequest_ShouldSameData()
        {
            static async Task CountRequest(string expectingResult)
            {
                var response = await CloudFunction.Post(Request.Create("COUNT", "", ""));

                Assert.That(response.IsSuccess, Is.True);
                Assert.That(response.Body, Is.EqualTo(expectingResult));
            }

            await CloudFunction.Upload("test_user_2", new UploadData() { Image = new byte[] { 1 }, Data = new byte[] { 2 } });
            
            await CountRequest("1");
            await CountRequest("1");
        }
    }
}