using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Portal2Boards.Net.API;
using Portal2Boards.Net.API.Models;
using Portal2Boards.Net.Extensions;
using Portal2Boards.Net.Utilities;
using Portal2Boards.Utilities;

namespace Portal2Boards.Net
{
	public sealed class Portal2BoardsClient : IDisposable
	{
		public bool NoSsl { get; set; }
		public string BaseApiUrl => $"http{((NoSsl) ? string.Empty : "s")}://board.iverb.me";
		public ChangelogParameters Parameters { get; set; }
		public ResponseType LastResponse { get; internal set; } = ResponseType.Unknown;
		public bool AutoCache
		{
			get => _autoCache;
			set
			{
				if (_autoCache != value)
				{
					_autoCache = value;
					if (_autoCache)
					{
						_cache = new Cache();
						_timer = new Timer(TimerCallback, _autoCache, (int)_cacheResetTime, (int)_cacheResetTime);
					}
					else
					{
						_cache = null;
						_timer = null;
					}
				}
			}
		}
		public uint CacheResetTime
		{
			get => _cacheResetTime / 60 / 1000;
			set => _cacheResetTime = ((value == 0) ? 5 : value) * 60 * 1000;
		}
		private WebClient _client { get; set; }
		private Cache _cache { get; set; }
		private Timer _timer { get; set; }
		private bool _autoCache;
		private uint _cacheResetTime;

		public Portal2BoardsClient(HttpClient client = default(HttpClient), bool autoCache = true, uint cacheResetTime = 5, bool noSsl = false)
		{
			_client = new WebClient(client);
			CacheResetTime = cacheResetTime;
			AutoCache = autoCache;
			NoSsl = noSsl;
		}
		public Portal2BoardsClient(ChangelogParameters parameters, HttpClient client = default(HttpClient), bool autoCache = true, uint cacheResetTime = 5, bool noSsl = false)
		{
			_client = new WebClient(client);
			CacheResetTime = cacheResetTime;
			AutoCache = autoCache;
			Parameters = parameters;
			NoSsl = noSsl;
		}

		public async Task<Changelog> GetChangelogAsync(string query = default(string))
		{
			var result = default(Changelog);
			try
			{
				var url = $"{BaseApiUrl}/changelog/json{((query == default(string)) ? await Parameters.ToQuery().ConfigureAwait(false) : query)}";
				result = new Changelog(await GetCacheOrFetch<ChangelogData[]>(url).ConfigureAwait(false), url);
				LastResponse = ResponseType.Success;
			}
			catch (Exception e)
			{
				LastResponse = await Logger.LogModelException<Changelog>(e).ConfigureAwait(false);
			}
			return result;
		}
		public async Task<Board> GetLeaderboardAsync(uint chamberId)
		{
			var result = default(Board);
			try
			{
				var url = $"{BaseApiUrl}/chamber/{chamberId}/json";
				result = new Board(await GetCacheOrFetch<IReadOnlyDictionary<ulong, BoardEntryData>>(url).ConfigureAwait(false), url);
				LastResponse = ResponseType.Success;
			}
			catch (Exception e)
			{
				LastResponse = await Logger.LogModelException<Board>(e).ConfigureAwait(false);
			}
			return result;
		}
		public async Task<Board> GetLeaderboardAsync(Map map)
		{
			var result = default(Board);
			try
			{
				var url = $"{BaseApiUrl}/chamber/{map.BestTimeId}/json";
				result = new Board(await GetCacheOrFetch<IReadOnlyDictionary<ulong, BoardEntryData>>(url).ConfigureAwait(false), url);
				LastResponse = ResponseType.Success;
			}
			catch (Exception e)
			{
				LastResponse = await Logger.LogModelException<Board>(e).ConfigureAwait(false);
			}
			return result;
		}
		public async Task<Profile> GetProfileAsync(string boardName)
		{
			var result = default(Profile);
			try
			{
				var url = $"{BaseApiUrl}/profile/{boardName.Replace(" ", string.Empty)}/json";
				result = new Profile(await GetCacheOrFetch<ProfileData>(url).ConfigureAwait(false), url);
				LastResponse = ResponseType.Success;
			}
			catch (Exception e)
			{
				LastResponse = await Logger.LogModelException<Profile>(e).ConfigureAwait(false);
			}
			return result;
		}
		public async Task<Profile> GetProfileAsync(ulong steamId)
		{
			var result = default(Profile);
			try
			{
				var url = $"{BaseApiUrl}/profile/{steamId}/json";
				result = new Profile(await GetCacheOrFetch<ProfileData>(url).ConfigureAwait(false), url);
				LastResponse = ResponseType.Success;
			}
			catch (Exception e)
			{
				LastResponse = await Logger.LogModelException<Profile>(e).ConfigureAwait(false);
			}
			return result;
		}
		public async Task<Aggregated> GetAggregatedAsync(Chapter id = default(Chapter))
		{
			var result = default(Aggregated);
			try
			{
				var url = $"{BaseApiUrl}/aggregated/{await GetMode(id).ConfigureAwait(false)}/json";
				result = new Aggregated(await GetCacheOrFetch<AggregatedData>(url).ConfigureAwait(false), url);
				LastResponse = ResponseType.Success;
			}
			catch (Exception e)
			{
				LastResponse = await Logger.LogModelException<Aggregated>(e).ConfigureAwait(false);
			}
			return result;
		}
		public async Task<byte[]> GetDemoContentAsync(uint changelogId)
		{
			var result = default(byte[]);
			try
			{
				var url = $"{BaseApiUrl}/getDemo?id={changelogId}";
				if (!_autoCache)
				{
					result = await _client.GetBytesAsync(url).ConfigureAwait(false);
				}
				else
				{
					result = await _cache.Get<byte[]>(url).ConfigureAwait(false) ?? await _client.GetBytesAsync(url).ConfigureAwait(false);
					await _cache.AddOrUpdate(url, result).ConfigureAwait(false);
				}
				LastResponse = ResponseType.Success;
			}
			catch (Exception e)
			{
				LastResponse = (e is HttpRequestException) ? ResponseType.HttpRequestError : ResponseType.Unknown;
				await Logger.LogException<string>(e).ConfigureAwait(false);
			}
			return result;
		}

		public Task ResetCacheTimer()
		{
			if (AutoCache)
				_timer = new Timer(TimerCallback, _autoCache, (int)_cacheResetTime, (int)_cacheResetTime);
			return Task.FromResult(AutoCache);
		}

		public Task ClearCache()
		{
			_cache = new Cache();
			return Task.FromResult(true);
		}

		internal async Task<T> GetCacheOrFetch<T>(string url)
			where T : class
		{
			if (!_autoCache)
				return await _client.GetJsonObjectAsync<T>(url).ConfigureAwait(false);
			var obj = await _cache.Get<T>(url).ConfigureAwait(false) ?? await _client.GetJsonObjectAsync<T>(url).ConfigureAwait(false);
			await _cache.AddOrUpdate(url, obj).ConfigureAwait(false);
			return obj;
		}

		internal void TimerCallback(Object stateInfo)
		{
			if ((bool)stateInfo)
				_cache = new Cache();
		}

		internal Task<string> GetMode(Chapter id)
		{
			var mode = "overall";
			switch (id)
			{
				case Chapter.TeamBuilding:
				case Chapter.MassAndVelocity:
				case Chapter.HardLight:
				case Chapter.ExcursionFunnels:
				case Chapter.MobilityGels:
				case Chapter.ArtTherapy:
					mode = $"coop/{id}";
					break;
				case Chapter.TheCourtseyCall:
				case Chapter.TheColdBoot:
				case Chapter.TheReturn:
				case Chapter.TheSurprise:
				case Chapter.TheEscape:
				case Chapter.TheFall:
				case Chapter.TheReunion:
				case Chapter.TheItch:
				case Chapter.ThePartWhereHeKillsYou:
					mode = $"sp/{id}";
					break;
			}
			return Task.FromResult(mode);
		}

		public void Dispose()
		{
			Parameters = null;
			_client = null;
			_cache = null;
			_timer = null;
		}
	}
}