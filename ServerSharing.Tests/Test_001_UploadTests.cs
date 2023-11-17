using Newtonsoft.Json;
using NUnit.Framework;
using ServerSharing.Data;
using System.Text;

namespace ServerSharing.Tests
{
    [TestFixture]
    public class Test_001_UploadTests
    {
        private UploadData _uploadData;

        [OneTimeSetUp]
        public async Task ClearRecords()
        {
            await CloudFunction.Clear("records");

            _uploadData = new UploadData()
            {
                Metadata = new RecordMetadata()
                {
                    Name = "Joe",
                    Description = "My description",
                },
                Image = new byte[] { 0, 1, 2 },
                Data = Encoding.UTF8.GetBytes("some_data"),
            };
        }

        [Test]
        public async Task Upload_CorrectJson_ShouldSuccess()
        {
            var response = await CloudFunction.Post(Request.Create("UPLOAD", "test_user", JsonConvert.SerializeObject(_uploadData)));

            Assert.That(response.IsSuccess, Is.True);
            Assert.That(response.Body, Is.Not.Null);
        }

        [Test]
        public async Task Upload_EmptyJson_ShouldError()
        {
            var response = await CloudFunction.Post(Request.Create("UPLOAD", "test_user", "{}"));

            Assert.That(response.IsSuccess, Is.False);
        }

        [Test]
        public async Task Upload_WrongJson_ShouldNotSuccess()
        {
            var response = await CloudFunction.Post(Request.Create("UPLOAD", "test_user", "abracadabra"));

            Assert.That(response.IsSuccess, Is.False);
        }

        [Test]
        public async Task Upload_SameRecord_IdMustBeDifferent()
        {
            var response1 = await CloudFunction.Post(Request.Create("UPLOAD", "test_user", JsonConvert.SerializeObject(_uploadData)));
            var response2 = await CloudFunction.Post(Request.Create("UPLOAD", "test_user", JsonConvert.SerializeObject(_uploadData)));

            Assert.That(response1.IsSuccess, Is.True);
            Assert.That(response2.IsSuccess, Is.True);
            Assert.That(response1.Body != response2.Body);
        }

        [Test]
        public async Task Upload_OtherUserSameRecord_AllIdMustBeDifferent()
        {
            var response1 = await CloudFunction.Post(Request.Create("UPLOAD", "test_user", JsonConvert.SerializeObject(_uploadData)));
            var response2 = await CloudFunction.Post(Request.Create("UPLOAD", "test_user_2", JsonConvert.SerializeObject(_uploadData)));

            Assert.That(response1.IsSuccess, Is.True);
            Assert.That(response2.IsSuccess, Is.True);
            Assert.That(response1.Body != response2.Body);
        }
    }
}