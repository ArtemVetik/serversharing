﻿using Newtonsoft.Json;
using NUnit.Framework;
using ServerSharing.Data;

namespace ServerSharing.Tests
{
    [TestFixture]
    public class Test_010_RateTests
    {
        [Test]
        public async Task Rate_001_CorrectId_ShouldLike()
        {
            var id = await CloudFunction.Upload("test_upload", new UploadData() { Image = new byte[] { }, Data = new byte[] { } });

            var response = await CloudFunction.Post(Request.Create("RATE", "some_user", JsonConvert.SerializeObject(new RatingRequestBody()
            {
                Id = id,
                Rating = 5,
            })));

            Assert.True(response.IsSuccess, $"{response.StatusCode}, {response.ReasonPhrase}");
        }

        [Test]
        public async Task Rate_002_UnknownId_ShouldLike()
        {
            var response = await CloudFunction.Post(Request.Create("RATE", "some_user", JsonConvert.SerializeObject(new RatingRequestBody()
            {
                Id = "unknown",
                Rating = 5,
            })));
            
            Assert.True(response.IsSuccess, $"{response.StatusCode}, {response.ReasonPhrase}");
        }

        [Test]
        public async Task Rate_003_InvalidRating_ShouldBeNotSuccess()
        {
            var response = await CloudFunction.Post(Request.Create("RATE", "some_user", JsonConvert.SerializeObject(new RatingRequestBody()
            {
                Id = "unknown",
                Rating = 10,
            })));

            Assert.False(response.IsSuccess, $"{response.StatusCode}, {response.ReasonPhrase}");
        }
    }
}