using NUnit.Framework;
using ServerSharing.Data;
using System.Text;

namespace ServerSharing.Tests
{
    [TestFixture]
    public class Test_002_DownloadTests
    {
        private string _userOne1;
        private string _userTwo1;
        private string _userTwo2;
        private string _userThree1;

        [OneTimeSetUp]
        public async Task Setup()
        {
            await CloudFunction.Clear("records");
            await CloudFunction.Clear("downloads");

            _userOne1 = await CloudFunction.Upload("userOne", new UploadData() { Image = new byte[] { }, Data = Encoding.UTF8.GetBytes("data_userOne") });
            _userTwo1 = await CloudFunction.Upload("userTwo", new UploadData() { Image = new byte[] { }, Data = Encoding.UTF8.GetBytes("data_userTwo1") });
            _userTwo2 = await CloudFunction.Upload("userTwo", new UploadData() { Image = new byte[] { }, Data = Encoding.UTF8.GetBytes("data_userTwo2") });
            _userThree1 = await CloudFunction.Upload("userThree", new UploadData() { Image = new byte[] { }, Data = Encoding.UTF8.GetBytes("data_userThree") });
        }

        [Test]
        public async Task Download_FewUsers_SouldCorrectData()
        {
            var response = await CloudFunction.Post(Request.Create("DOWNLOAD", "test_download_1", _userOne1));

            Assert.That(response.IsSuccess, Is.True);
            Assert.That(EncodeBody(response.Body), Is.EqualTo("data_userOne"));

            response = await CloudFunction.Post(Request.Create("DOWNLOAD", "test_download_2", _userTwo2));

            Assert.That(response.IsSuccess, Is.True);
            Assert.That(EncodeBody(response.Body), Is.EqualTo("data_userTwo2"));
        }

        [Test]
        public async Task Download_TwiceDownload_SouldReturnSameData()
        {
            for (int i = 0; i < 2; i++)
            {
                var response = await CloudFunction.Post(Request.Create("DOWNLOAD", "test_download_3", _userThree1));

                Assert.That(response.IsSuccess, Is.True);
                Assert.That(EncodeBody(response.Body), Is.EqualTo("data_userThree"));
            }
        }

        [Test]
        public async Task Download_InvalidId_SouldNotSuccess()
        {
            var response = await CloudFunction.Post(Request.Create("DOWNLOAD", "test_download_4", "invalid_id"));

            Assert.That(response.IsSuccess, Is.False);
        }

        private static string EncodeBody(string body)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(body));
        }
    }
}