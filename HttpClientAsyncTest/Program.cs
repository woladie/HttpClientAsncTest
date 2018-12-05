using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace HttpClientAsyncTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var httpClient = new HttpClient();
            const string url = @"https://samples.openweathermap.org/data/2.5/forecast?id=524901&appid=b1b15e88fa797225412429c1c50c122a1";
            object contentSync = await httpClient.GetContentFromJson<object>(url, null, NullLogger.Instance);
            Console.WriteLine(contentSync);

            object contentAsync = await httpClient.GetContentFromJsonAsync<object>(url, null, NullLogger.Instance);
        }
    }
}
