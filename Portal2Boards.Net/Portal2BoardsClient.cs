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
		public bool AutoCache
		{
			get => _autoCache;
			set
			{
				if ((_autoCache != value) && (_autoCache))
				{
					_timer = null;
					_cache = null;
				}
				_autoCache = value;
			}
		}
		public uint CacheResetTime
		{
			get => _cacheResetTime / 60 / 1000;
			set => _cacheResetTime = ((value == 0) ? 5 : value) * 60 * 1000;
		}
		public event Func<object, LogMessage, Task> OnException;
		private WebClient _client { get; }
		private Cache _cache { get; set; }
		private Timer _timer { get; set; }
		private bool _autoCache;
		private uint _cacheResetTime;

		public Portal2BoardsClient(HttpClient client = default(HttpClient), bool autoCache = true, uint cacheResetTime = 5, bool noSsl = false)
		{
			_client = new WebClient(client);
			_autoCache = autoCache;
			CacheResetTime = cacheResetTime;
			NoSsl = noSsl;
		}
		public Portal2BoardsClient(ChangelogParameters parameters, HttpClient client = default(HttpClient), bool autoCache = true, uint cacheResetTime = 5, bool noSsl = false)
			: this(client, autoCache, cacheResetTime, noSsl)
		{
			Parameters = parameters;
		}

		public async Task<Changelog> GetChangelogAsync(string query = default(string))
		{
			var result = default(Changelog);
			if (!string.IsNullOrEmpty(query))
			{
				if (!query.StartsWith("?"))
					throw new InvalidOperationException("Missing \"?\" at the start of query string!");
				if (query.IndexOf(" ") != -1)
					throw new InvalidOperationException("Did you forget to escape the uri string?");
			}
			else
			{
				if (Parameters == null)
					throw new InvalidOperationException("Cannot fetch changelog without defined query string or parameters. Use \"?\" alone for an empty request.");
				query = await Parameters.ToQuery().ConfigureAwait(false);
			}

			try
			{
				var url = $"{BaseApiUrl}/changelog/json{query}";
				result = new Changelog(await GetCacheOrFetch<ChangelogData[]>(url).ConfigureAwait(false), url);
			}
			catch (Exception e)
			{
				if (OnException != null)
					await OnException.Invoke(this, new LogMessage(typeof(Changelog), e)).ConfigureAwait(false);
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
			}
			catch (Exception e)
			{
				if (OnException != null)
					await OnException.Invoke(this, new LogMessage(typeof(Board), e)).ConfigureAwait(false);
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
			}
			catch (Exception e)
			{
				if (OnException != null)
					await OnException.Invoke(this, new LogMessage(typeof(Board), e)).ConfigureAwait(false);
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
			}
			catch (Exception e)
			{
				if (OnException != null)
					await OnException.Invoke(this, new LogMessage(typeof(Profile), e)).ConfigureAwait(false);
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
			}
			catch (Exception e)
			{
				if (OnException != null)
					await OnException.Invoke(this, new LogMessage(typeof(Profile), e)).ConfigureAwait(false);
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
			}
			catch (Exception e)
			{
				if (OnException != null)
					await OnException.Invoke(this, new LogMessage(typeof(Aggregated), e)).ConfigureAwait(false);
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
			}
			catch (Exception e)
			{
				if (OnException != null)
					await OnException.Invoke(this, new LogMessage(typeof(byte[]), e)).ConfigureAwait(false);
			}
			return result;
		}

		public Task<bool> ResetCacheTimer()
		{
			if (_autoCache)
				_timer = new Timer(TimerCallback, _autoCache, (int)_cacheResetTime, (int)_cacheResetTime);
			return Task.FromResult(_autoCache);
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

			// Make sure that time and cache exist
			// This will start the timer when invoking the first api request
			_timer = _timer ?? new Timer(TimerCallback, _autoCache, (int)_cacheResetTime, (int)_cacheResetTime);
			_cache = _cache ?? new Cache();

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
			_client.Dispose();
			// Stop auto cache, because callback can still happen after disposing
			_autoCache = default(bool);
			_timer?.Dispose();
			_cache = null;
			OnException = null;
			Parameters = null;
		}
	}
}