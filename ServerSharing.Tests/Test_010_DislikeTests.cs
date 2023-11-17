using Newtonsoft.Json;
using NUnit.Framework;
using ServerSharing.Data;

namespace ServerSharing.Tests
{
    [TestFixture]
    public class Test_010_DislikeTests
    {
        [SetUp]
        public async Task Setup()
        {
            await CloudFunction.Clear("records");
            await CloudFunction.Clear("likes");
        }

        [Test]
        public async Task SelectLiked_AfterDislikeAll_ShouldReturnEmptyData()
        {
            var id = await CloudFunction.Upload("some_user", new UploadData() { Data = new byte[] { 0 }, Image = new byte[] { 1 } });
            await CloudFunction.Like("user1", id);
            await CloudFunction.Post(new Request() { method = "DISLIKE", user_id = "user1", body = id });

            var response = await CloudFunction.Post(Request.Create("SELECT_SELF", "user1", JsonConvert.SerializeObject(SelectEntryType.Liked)));
            Assert.That(response.IsSuccess, Is.True);
            var selectData = JsonConvert.DeserializeObject<List<SelectResponseData>>(response.Body);
            Assert.That(selectData.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task SelectLiked_DislikeOtherUserId_ShouldNotDeleteLike()
        {
            var id = await CloudFunction.Upload("some_user", new UploadData() { Data = new byte[] { 0 }, Image = new byte[] { 1 } });
            await CloudFunction.Like("user1", id);
            await CloudFunction.Post(new Request() { method = "DISLIKE", user_id = "user2", body = id });

            var response = await CloudFunction.Post(Request.Create("SELECT_SELF", "user1", JsonConvert.SerializeObject(SelectEntryType.Liked)));
            Assert.That(response.IsSuccess, Is.True);
            var selectData = JsonConvert.DeserializeObject<List<SelectResponseData>>(response.Body);
            Assert.That(selectData.Count, Is.EqualTo(1));
        }
    }
}