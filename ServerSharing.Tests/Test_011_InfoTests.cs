using Newtonsoft.Json;
using NUnit.Framework;
using ServerSharing.Data;

namespace ServerSharing.Tests
{
    [TestFixture]
    public class Test_011_InfoTests
    {
        [SetUp]
        public async Task Setup()
        {
            await CloudFunction.Clear("records");
        }

        [Test]
        public async Task Info_CorrectId_ShouldReturnData()
        {
            var id = await CloudFunction.Upload("user", new UploadData() { Data = new byte[] { }, Image = new byte[] { } });

            var response = await CloudFunction.Post(new Request() { method = "INFO", user_id = "user1", body = id });
            Assert.That(response.IsSuccess, Is.True);

            var data = JsonConvert.DeserializeObject<SelectResponseData>(response.Body);
            Assert.That(data.Id, Is.EqualTo(id));
        }

        [Test]
        public async Task Info_WrongId_ShouldReturnEmptyData()
        {
            await CloudFunction.Upload("user", new UploadData() { Data = new byte[] { }, Image = new byte[] { } });

            var response = await CloudFunction.Post(new Request() { method = "INFO", user_id = "user1", body = "wrong-id" });
            Assert.That(response.IsSuccess, Is.False);
        }
    }
}