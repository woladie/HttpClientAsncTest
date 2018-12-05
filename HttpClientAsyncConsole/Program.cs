using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace HttpClientAsyncConsole
{
    public class Program
    {
        private const string Url = @"https://samples.openweathermap.org/data/2.5/forecast?id=524901&appid=b1b15e88fa797225412429c1c50c122a1";

        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var httpClient = new HttpClient();
            object contentSync = await GetWeatherSync(httpClient);
            Console.WriteLine(contentSync);

            object contentAsync = await GetWeatherAsync(httpClient);
        }

        public static async Task<object> GetWeatherAsync(HttpClient httpClient)
        {
            return await httpClient.GetContentFromJsonAsync<object>(Url, null, NullLogger.Instance);
        }

        public static async Task<object> GetWeatherSync(HttpClient httpClient)
        {
            return await httpClient.GetContentFromJson<object>(Url, null, NullLogger.Instance);
        }
    }
}
