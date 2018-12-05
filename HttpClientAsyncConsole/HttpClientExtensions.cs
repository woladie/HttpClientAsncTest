using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HttpClientAsyncConsole
{
    /// <summary>
    /// Extends <seealso cref="HttpClient"/>.
    /// </summary>
    public static class HttpClientExtensions
    {
        private static readonly string _dateTimeWebUtcFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";

        /// <summary>
        /// Sets the authorization header with bearer token.
        /// </summary>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="accessToken">The users access token.</param>
        public static void SetBearerToken(this HttpClient httpClient, string accessToken)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        /// <summary>
        /// Reads the content from response message.
        /// </summary>
        /// <typeparam name="TData">The data type of the content.</typeparam>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="url">The full URL containing for a GET request.</param>
        /// <param name="accessToken">The users access token.</param>
        /// <param name="logger">The logging facility.</param>
        /// <returns></returns>
        public static async Task<TData> GetContentFromJson<TData>(this HttpClient httpClient, string url, string accessToken, ILogger logger)
        {
            if (!string.IsNullOrWhiteSpace(accessToken))
                SetBearerToken(httpClient, accessToken);

            HttpResponseMessage message = httpClient.GetAsync(url).ConfigureAwait(false).GetAwaiter().GetResult();
            return await message.GetContentFromJson<TData>(logger);
        }

        public static async Task<TData> GetContentFromJsonAsync<TData>(this HttpClient httpClient, string url, string accessToken, ILogger logger)
        {
            if (!string.IsNullOrWhiteSpace(accessToken))
                SetBearerToken(httpClient, accessToken);

            HttpResponseMessage message = await httpClient.GetAsync(url);
            return await message.GetContentFromJson<TData>(logger);
        }

        /// <summary>
        /// Reads the content and deserializes it to <typeparamref name="TData"/>.
        /// </summary>
        /// <typeparam name="TData">The data type of the content.</typeparam>
        /// <param name="message">The response of a HTTP request.</param>
        /// <param name="logger">The logging facility.</param>
        /// <returns></returns>
        public static async Task<TData> GetContentFromJson<TData>(this HttpResponseMessage message, ILogger logger)
        {
            if (message?.Content == null)
            {
                return default(TData);
            }

            using (var stream = await message.Content.ReadAsStreamAsync())
            {
                if (message.IsSuccessStatusCode)
                    return DeserializeJsonFromStream<TData>(stream);

                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    string error = await reader.ReadToEndAsync();
                    logger?.LogError(error);
                }

                return default(TData);
            }
        }

        private static TData DeserializeJsonFromStream<TData>(Stream stream)
        {
            if (stream == null || stream.CanRead == false)
                return default(TData);

            using (var sr = new StreamReader(stream))
            using (var jtr = new JsonTextReader(sr))
            {
                var js = new JsonSerializer();
                return js.Deserialize<TData>(jtr);
            }
        }

        /// <summary>
        /// Serializes 
        /// </summary>
        /// <typeparam name="TData">The data type of the content.</typeparam>
        /// <param name="stream">The stream into which data should be flushed.</param>
        /// <param name="data">The data to serialize into <paramref name="stream"/>.</param>
        public static void SerializeJsonToStream<TData>(Stream stream, TData data)
        {
            if (stream?.CanWrite != true)
                return;

            using (var sr = new StreamWriter(stream, Encoding.ASCII, 256, true))
            using (var jtw = new JsonTextWriter(sr))
            {
                jtw.DateFormatString = _dateTimeWebUtcFormat;
                jtw.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                var js = new JsonSerializer();
                js.Serialize(jtw, data);
                jtw.Flush();

                if (stream.CanSeek)
                    stream.Seek(0, SeekOrigin.Begin);
            }
        }

        private static async Task<TData> StreamToStringAsync<TData>(this Stream stream)
        {
            if (stream == null)
                return default(TData);

            string content = null;
            using (var sr = new StreamReader(stream))
                content = await sr.ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(content))
            {
                return default(TData);
            }

            return JsonConvert.DeserializeObject<TData>(content);
        }


        /// <summary>
        /// Gets the content by HTTP GET for a <paramref name="url"/> containing a component identifier.
        /// </summary>
        /// <typeparam name="TData">The data type of the content.</typeparam>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="url">The full URL containing a component identifier.</param>
        /// <param name="accessToken">The users access token.</param>
        /// <param name="logger">The logging facility.</param>
        /// <returns></returns>
        public static async Task<TData> GetFromComponentAsync<TData>(this HttpClient httpClient, string url, string accessToken, ILogger logger) where TData : new()
        {
            httpClient.SetBearerToken(accessToken);

            var message = await httpClient.GetAsync(url, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false);
            return await GetContentFromJson<TData>(message, NullLogger.Instance);
        }

        /// <summary>
        /// Gets the UTC web formatted date.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string GetUtcWebDate(this DateTime date)
        {
            return date.ToUniversalTime().ToString(_dateTimeWebUtcFormat);
        }

        /// <summary>
        /// Creates the JSON content for HTTP request from <paramref name="data"/>.
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="stream"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static StreamContent CreateJsonStreamRequest<TData>(this MemoryStream stream, TData data) where TData : new()
        {
            if (data == null)
            {
                HttpClientExtensions.SerializeJsonToStream(stream, default(TData));
                return new StreamContent(stream);
            }

            HttpClientExtensions.SerializeJsonToStream(stream, data);
            var streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return streamContent;
        }

        /// <summary>
        /// Ensures that <paramref name="date"/> is of kind UTC.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static DateTime? EnsureDateTimeUtc(this DateTime? date)
        {
            if (date.HasValue && date.Value.Kind != DateTimeKind.Utc)
            {
                date = date.Value.ToUniversalTime();
            }

            return date;
        }

    }

    public interface ILogger
    {
        void LogError(string error);
    }

    public class NullLogger : ILogger
    {
        public void LogError(string error)
        {
            
        }

        public static ILogger Instance => new NullLogger();
    }

}
