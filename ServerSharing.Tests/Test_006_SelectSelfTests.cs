using Newtonsoft.Json;
using NUnit.Framework;
using ServerSharing.Data;
using System.Text;

namespace ServerSharing.Tests
{
    [TestFixture]
    public class Test_006_SelectSelfTests
    {
        private string _id1;
        private string _id2;
        private string _id3;
        private string _id4;
        private string _id5;

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

            _id1 = await CloudFunction.Upload("user1", uploadData);
            _id2 = await CloudFunction.Upload("user4", uploadData);
            _id3 = await CloudFunction.Upload("user2", uploadData);
            _id4 = await CloudFunction.Upload("user1", uploadData);
            _id5 = await CloudFunction.Upload("user3", uploadData);
        }

        [Test]
        public async Task SelectSelf_Uploaded_ShouldCorrectCount()
        {
            var selectRequest = JsonConvert.SerializeObject(SelectEntryType.Uploaded);
            var response = await CloudFunction.Post(Request.Create("SELECT_SELF", "user1", selectRequest));

            Assert.That(response.IsSuccess, Is.True);

            var selectData = JsonConvert.DeserializeObject<List<SelectResponseData>>(response.Body);
            
            Assert.That(selectData.Count, Is.EqualTo(2));
            Assert.That(selectData.Any(data => data.Id == _id1));
            Assert.That(selectData.Any(data => data.Id == _id4));
        }

        [Test]
        public async Task SelectSelf_Downloaded_ShouldCorrectCount()
        {
            await CloudFunction.Download("user1", _id1);
            await CloudFunction.Download("user1", _id2);

            var selectRequest = JsonConvert.SerializeObject(SelectEntryType.Downloaded);
            var response = await CloudFunction.Post(Request.Create("SELECT_SELF", "user1", selectRequest));

            Assert.That(response.IsSuccess, Is.True);

            var selectData = JsonConvert.DeserializeObject<List<SelectResponseData>>(response.Body);

            Assert.That(selectData.Count, Is.EqualTo(2));
            Assert.That(selectData.Any(data => data.Id == _id1));
            Assert.That(selectData.Any(data => data.Id == _id2));
        }

        [Test]
        public async Task SelectSelf_Liked_ShouldCorrectCount()
        {
            await CloudFunction.Like("user1", _id3);
            await CloudFunction.Like("user1", _id4);

            var selectRequest = JsonConvert.SerializeObject(SelectEntryType.Liked);
            var response = await CloudFunction.Post(Request.Create("SELECT_SELF", "user1", selectRequest));

            Assert.That(response.IsSuccess, Is.True);

            var selectData = JsonConvert.DeserializeObject<List<SelectResponseData>>(response.Body);

            Assert.That(selectData.Count, Is.EqualTo(2));
            Assert.That(selectData.Any(data => data.Id == _id3));
            Assert.That(selectData.Any(data => data.Id == _id4));
        }

        [Test]
        public async Task SelectSelf_EmptyUser_ShouldEmptyData()
        {
            SelectEntryType[] types = new SelectEntryType[] { SelectEntryType.Uploaded, SelectEntryType.Downloaded, SelectEntryType.Liked };

            foreach (var type in types)
            {
                var selectRequest = JsonConvert.SerializeObject(type);
                var response = await CloudFunction.Post(Request.Create("SELECT_SELF", "user0", selectRequest));

                Assert.That(response.IsSuccess, Is.True);

                var selectData = JsonConvert.DeserializeObject<List<SelectResponseData>>(response.Body);

                Assert.That(selectData.Count, Is.EqualTo(0));
            }
        }
    }
}