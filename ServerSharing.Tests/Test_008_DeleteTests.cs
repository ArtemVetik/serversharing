using Newtonsoft.Json;
using NUnit.Framework;
using ServerSharing.Data;

namespace ServerSharing.Tests
{
    [TestFixture]
    public class Test_008_DeleteTests
    {
        [SetUp]
        public async Task Setup()
        {
            await CloudFunction.Clear("records");
        }

        [Test]
        public async Task Delete_CorrectRecord_ShouldDelete()
        {
            var id = await CloudFunction.Upload("user1", new UploadData() { Image = new byte[] { 1 }, Data = new byte[] { 2 } });
            var response = await CloudFunction.Post(Request.Create("DELETE", "user1", id));
            Assert.That(response.IsSuccess, Is.True, response.Body);

            var selectData = await SelectAll();
            Assert.That(selectData.Count, Is.EqualTo(0));

            response = await CloudFunction.Post(Request.Create("DOWNLOAD", "test_user", id));
            Assert.That(response.IsSuccess, Is.False);

            response = await CloudFunction.Post(Request.Create("LOAD_IMAGE", "test_user", id));
            Assert.That(response.IsSuccess, Is.False);
        }

        [Test]
        public async Task Delete_InvalidUser_ShouldNotDelete()
        {
            var id = await CloudFunction.Upload("user1", new UploadData() { Image = new byte[] { 1 }, Data = new byte[] { 2 } });
            var response = await CloudFunction.Post(Request.Create("DELETE", "user1_1", id));
            Assert.That(response.IsSuccess, Is.False, response.Body);

            var selectData = await SelectAll();
            Assert.That(selectData.Count, Is.EqualTo(1));
        }

        private static async Task<List<SelectResponseData>> SelectAll()
        {
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

            if (response.IsSuccess == false)
                throw new InvalidOperationException("Select error: " + response);

            return JsonConvert.DeserializeObject<List<SelectResponseData>>(response.Body);
        }
    }
}