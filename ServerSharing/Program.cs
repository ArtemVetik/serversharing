using Newtonsoft.Json;
using ServerSharing.Data;

namespace ServerSharing
{
    public class Program
    {
        private static readonly HttpClient _client = new();

        public static async Task Main(string[] args)
        {
            var body = new SelectRequestBody()
            {
                Parameters = new SelectRequestBody.SortParameters()
                {
                    Sort = Sort.Date,
                    Date = DateTime.Now.AddDays(1)
                },
                Limit = 3,
            };

            var request = new Request() { method = "SELECT", user_id = "thisUser", body = JsonConvert.SerializeObject(body) };
            var content = new StringContent(JsonConvert.SerializeObject(request));

            var response = await _client.PostAsync("https://functions.yandexcloud.net/d4e2m4vp43u2gjs1an5h?integration=raw", content);
            var responseString = await response.Content.ReadAsStringAsync();

            Console.WriteLine(responseString);
        }
    }
}