using NUnit.Framework;
using ServerSharing.Data;

namespace ServerSharing.Tests
{
    [TestFixture]
    public class Test_003_LoadImageTests
    {
        [OneTimeSetUp]
        public async Task Setup()
        {
            await CloudFunction.Clear("records");
        }

        [Test]
        public async Task LoadImage_CorrectId_ShouldLoad()
        {
            var id = await CloudFunction.Upload("userOne", new UploadData() { Image = new byte[] { 255, 0, 255 }, Data = new byte[] { 0, 0, 0 } });

            var response = await CloudFunction.Post(Request.Create("LOAD_IMAGE", "test_load_user", id));

            Assert.That(response.IsSuccess, Is.True, $"{response.StatusCode}, {response.ReasonPhrase}");

            var image = Convert.FromBase64String(response.Body);
            Assert.That(Enumerable.SequenceEqual(image, new byte[] { 255, 0, 255 }), Is.True, $"Expected: [255, 0, 255], But was: {string.Join(", ", image)}");
        }
    }
}