using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Portal2Boards.Utilities
{
	internal sealed class WebClient
	{
		private static HttpClient _client { get; set; }

		public WebClient(HttpClient client = default(HttpClient))
		{
			_client = client ?? new HttpClient();
			_client.DefaultRequestHeaders.UserAgent.ParseAdd("Portal2Boards.Net/1.0");
		}

		public async Task<T> GetJsonObjectAsync<T>(string url)
		{
			var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Get, url), HttpCompletionOption.ResponseContentRead).ConfigureAwait(false);
			response.EnsureSuccessStatusCode();
			return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
		}
	}
}