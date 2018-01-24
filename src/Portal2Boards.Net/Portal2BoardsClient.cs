using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Portal2Boards.API;
using Portal2Boards.API.Models;
using Portal2Boards.Extensions;
using Portal2Boards.Utilities;

namespace Portal2Boards
{
	public sealed class Portal2BoardsClient : IDisposable
	{
		public string BaseApiUrl => $"http{((NoSsl) ? string.Empty : "s")}://board.iverb.me";
		public event Func<object, LogMessage, Task> Log;

		// Config
		public bool NoSsl { get; set; }
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

		private WebClient _client;
		private Cache _cache;
		private Timer _timer;
		private bool _autoCache;
		private uint _cacheResetTime;

		public Portal2BoardsClient(
			string userAgent = "",
			bool autoCache = true,
			uint? cacheResetTime = default,
			bool noSsl = false)
		{
			_client = new WebClient(userAgent);
			_autoCache = autoCache;
			CacheResetTime = cacheResetTime ?? 5;
			NoSsl = noSsl;
		}

		public async Task<IChangelog> GetChangelogAsync()
		{
			if (Parameters == null)
				throw new InvalidOperationException("Empty query string and/or parameters are not set.");
			
			var result = default(IChangelog);
			try
			{
				var query = await Parameters.ToQuery().ConfigureAwait(false);
				var get = $"/changelog/json{query}";
				var model = await GetCacheOrFetch<ChangelogEntryModel[]>(get).ConfigureAwait(false);
				result = Changelog.Create(model);
			}
			catch (Exception ex)
			{
				if (Log != null)
					await Log.Invoke(this, new LogMessage(typeof(IChangelog), ex)).ConfigureAwait(false);
			}
			return result;
		}

		public async Task<IChangelog> GetChangelogAsync(string query = "?")
		{
			if (string.IsNullOrEmpty(query))
				throw new InvalidOperationException("Query string is null or empty.");
			if (!query.StartsWith("?"))
				throw new InvalidOperationException("Missing \"?\" in query string.");
			if (query.IndexOf(" ") != -1)
				throw new InvalidOperationException("Query string not escaped.");

			var result = default(IChangelog);
			try
			{
				var get = $"/changelog/json{query}";
				var model = await GetCacheOrFetch<ChangelogEntryModel[]>(get).ConfigureAwait(false);
				result = Changelog.Create(model);
			}
			catch (Exception ex)
			{
				if (Log != null)
					await Log.Invoke(this, new LogMessage(typeof(IChangelog), ex)).ConfigureAwait(false);
			}
			return result;
		}
		public async Task<IChamber> GetChamberAsync(ulong chamberId)
		{
			var result = default(IChamber);
			try
			{
				var get = $"/chamber/{chamberId}/json";
				var model = await GetCacheOrFetch<IReadOnlyDictionary<ulong, ChamberEntryModel>>(get).ConfigureAwait(false);
				result = Chamber.Create(this, chamberId, model);
			}
			catch (Exception ex)
			{
				if (Log != null)
					await Log.Invoke(this, new LogMessage(typeof(IChamber), ex)).ConfigureAwait(false);
			}
			return result;
		}
		public async Task<IChamber> GetChamberAsync(Map map)
		{
			var result = default(IChamber);
			try
			{
				if (!map.Exists)
					throw new InvalidOperationException("Map does not have a leaderboard.");
				
				var get = $"/chamber/{map.BestTimeId}/json";
				var model = await GetCacheOrFetch<IReadOnlyDictionary<ulong, ChamberEntryModel>>(get).ConfigureAwait(false);
				result = Chamber.Create(this, (ulong)map.BestTimeId, model);
			}
			catch (Exception ex)
			{
				if (Log != null)
					await Log.Invoke(this, new LogMessage(typeof(IChamber), ex)).ConfigureAwait(false);
			}
			return result;
		}
		public async Task<IProfile> GetProfileAsync(string boardName)
		{
			var result = default(IProfile);
			try
			{
				var get = $"/profile/{boardName.Replace(" ", string.Empty)}/json";
				var model = await GetCacheOrFetch<ProfileModel>(get).ConfigureAwait(false);
				result = Profile.Create(model);
			}
			catch (Exception ex)
			{
				if (Log != null)
					await Log.Invoke(this, new LogMessage(typeof(IProfile), ex)).ConfigureAwait(false);
			}
			return result;
		}
		public async Task<IProfile> GetProfileAsync(ulong steamId)
		{
			var result = default(IProfile);
			try
			{
				var get = $"/profile/{steamId}/json";
				var model = await GetCacheOrFetch<ProfileModel>(get).ConfigureAwait(false);
				result = Profile.Create(model);
			}
			catch (Exception ex)
			{
				if (Log != null)
					await Log.Invoke(this, new LogMessage(typeof(IProfile), ex)).ConfigureAwait(false);
			}
			return result;
		}
		public async Task<Aggregated> GetAggregatedAsync(Chapter id = default(Chapter))
		{
			var result = default(Aggregated);
			try
			{
				var mode = await GetMode(id).ConfigureAwait(false);
				var model = await GetCacheOrFetch<AggregatedModel>($"/aggregated/{mode}/json").ConfigureAwait(false);
				result = Aggregated.Create(this, model);
			}
			catch (Exception ex)
			{
				if (Log != null)
					await Log.Invoke(this, new LogMessage(typeof(Aggregated), ex)).ConfigureAwait(false);
			}
			return result;
		}
		public async Task<byte[]> GetDemoContentAsync(uint changelogId)
		{
			var result = default(byte[]);
			try
			{
				var get = $"/getDemo?id={changelogId}";
				if (!_autoCache)
				{
					result = await _client.GetBytesAsync(get).ConfigureAwait(false);
				}
				else
				{
					result = await _cache.Get<byte[]>(get).ConfigureAwait(false) ?? await _client.GetBytesAsync(get).ConfigureAwait(false);
					await _cache.AddOrUpdate(get, result).ConfigureAwait(false);
				}
			}
			catch (Exception ex)
			{
				if (Log != null)
					await Log.Invoke(this, new LogMessage(typeof(byte[]), ex)).ConfigureAwait(false);
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
				return await _client.GetJsonObjectAsync<T>(BaseApiUrl + url).ConfigureAwait(false);

			// Make sure that time and cache exist
			// This will start the timer when invoking at first api request
			_timer = _timer ?? new Timer(TimerCallback, _autoCache, (int)_cacheResetTime, (int)_cacheResetTime);
			_cache = _cache ?? new Cache();

			var model = await _cache.Get<T>(url).ConfigureAwait(false)
				?? await _client.GetJsonObjectAsync<T>(url).ConfigureAwait(false);
			await _cache.AddOrUpdate(url, model).ConfigureAwait(false);
			return model;
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
			Log = null;
			Parameters = null;
		}
	}
}