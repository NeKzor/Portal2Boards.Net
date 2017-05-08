using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Portal2Boards.Net.API;
using Portal2Boards.Net.API.Models;
using Portal2Boards.Utilities;

namespace Portal2Boards.Net
{
	public sealed class Portal2BoardsClient : IDisposable
	{
		public const string BaseApiUrl = "https://board.iverb.me/getChangelogJSON";
		public BoardParameters Parameters { get; set; }
		private WebClient _client { get; set; }

		public Portal2BoardsClient()
		{
			_client = new WebClient();
			Parameters = new BoardParameters();
		}
		public Portal2BoardsClient(HttpClient client = default(HttpClient))
		{
			_client = new WebClient(client);
			Parameters = new BoardParameters();
		}
		public Portal2BoardsClient(BoardParameters parameters = default(BoardParameters), HttpClient client = default(HttpClient))
		{
			_client = new WebClient(client);
			Parameters = parameters ?? new BoardParameters();
		}

		public async Task<Changelog> GetChangelogAsync()
		{
			var result = default(Changelog);
			try
			{
				var query = "?";
				foreach (var parameter in Parameters)
				{
					if (string.IsNullOrEmpty(parameter.Value))
						continue;
					query += $"{parameter.Key.Value}={Uri.EscapeDataString(parameter.Value)}&";
				}
				result = new Changelog(await _client.GetJsonObjectAsync<ChangelogData[]>(BaseApiUrl + query.Remove(query.Length - 1)).ConfigureAwait(false));
			}
			catch (Exception e)
			{
				Debug.WriteLine($"[Portal2Boards.Net] Failed to fetch changelog object.\nStacktrace: {e}");
			}
			return result;
		}
		public async Task<Changelog> GetChangelogAsync(string query)
		{
			var result = default(Changelog);
			try
			{
				result = new Changelog(await _client.GetJsonObjectAsync<ChangelogData[]>(BaseApiUrl + query).ConfigureAwait(false));
			}
			catch (Exception e)
			{
				Debug.WriteLine($"[Portal2Boards.Net] Failed to fetch changelog object.\nStacktrace: {e}");
			}
			return result;
		}

		public void Dispose()
		{
			Parameters = null;
			_client = null;
			GC.Collect();
		}
	}
}