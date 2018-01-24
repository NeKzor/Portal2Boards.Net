using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Portal2Boards.Utilities
{
	internal sealed class WebClient : IDisposable
	{
		private readonly HttpClient _client;

		public WebClient(string userAgent)
		{
			_client = new HttpClient();
			_client.DefaultRequestHeaders.UserAgent.ParseAdd
			(
				$"Portal2Boards.Net/2.0" +
				((!string.IsNullOrEmpty(userAgent)) ? userAgent : string.Empty)
			);
		}

		public async Task<T> GetJsonObjectAsync<T>(string url)
		{
			var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Get, url), HttpCompletionOption.ResponseContentRead).ConfigureAwait(false);
			response.EnsureSuccessStatusCode();
			return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
		}

		// Gets the content of a demo file
		public async Task<byte[]> GetBytesAsync(string url)
		{
			var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Get, url), HttpCompletionOption.ResponseContentRead).ConfigureAwait(false);
			response.EnsureSuccessStatusCode();
			return await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
		}

		public void Dispose()
		{
			_client.Dispose();
		}
	}
}