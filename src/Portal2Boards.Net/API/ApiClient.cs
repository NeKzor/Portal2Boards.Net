using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Portal2Boards.API
{
    internal sealed class ApiClient : IDisposable
    {
        private readonly HttpClient _client;

        public ApiClient(string userAgent)
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.UserAgent.ParseAdd
            (
                ((!string.IsNullOrEmpty(userAgent)) ? userAgent : string.Empty)
                + " Portal2Boards.Net/2.2"
            );
        }

        public async Task<T> GetJsonObjectAsync<T>(string url)
        {
            var get = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await _client.SendAsync(get, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
        }

        // Gets the content of a demo file
        public async Task<byte[]> GetBytesAsync(string url)
        {
            var get = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await _client.SendAsync(get, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
