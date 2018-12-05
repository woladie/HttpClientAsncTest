using System;
using System.Net.Http;
using System.Threading.Tasks;
using HttpClientAsyncConsole;
using Xunit;
using Xunit.Abstractions;

namespace HttpClientAsyncXUnitTest
{
    public class HttpClientAsyncTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public HttpClientAsyncTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task Test1()
        {
            var client = new HttpClient();
            const string url = @"https://samples.openweathermap.org/data/2.5/forecast?id=524901&appid=b1b15e88fa797225412429c1c50c122a1";
            object contentSync = await Program.GetWeatherSync(client);
            _testOutputHelper.WriteLine(contentSync.ToString());

            object contentAsync = await client.GetContentFromJsonAsync<object>(url, null, NullLogger.Instance);
            _testOutputHelper.WriteLine(contentAsync.ToString());
            Assert.NotNull(contentAsync);
        }
    }
}
