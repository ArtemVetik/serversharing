using NUnit.Framework;
using ServerSharing.Data;

namespace ServerSharing.Tests
{
    [TestFixture]
    public class Test_004_LikeTests
    {
        [Test]
        public async Task Like_CorrectId_ShouldLike()
        {
            var id = await CloudFunction.Upload("test_upload", new UploadData() { Image = new byte[] { }, Data = new byte[] { } });

            var response = await CloudFunction.Post(Request.Create("LIKE", "some_user", id));
            Assert.That(response.IsSuccess, Is.True);
        }

        [Test]
        public async Task Like_UnknownId_ShouldLike()
        {
            var response = await CloudFunction.Post(Request.Create("LIKE", "some_user", "unknown_id"));
            Assert.That(response.IsSuccess, Is.True);
        }
    }
}