using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Portal2Boards.API;
using Portal2Boards.Extensions;

namespace Portal2Boards
{
    public sealed class Portal2BoardsClient : IDisposable
    {
        public string BaseApiUrl => $"http{((NoSsl) ? string.Empty : "s")}://board.iverb.me";
        public event Func<object, LogMessage, Task> Log;

        // Config
        public bool NoSsl { get; set; }
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

        private ApiClient _client;
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
            _client = new ApiClient(userAgent);
            _autoCache = autoCache;
            CacheResetTime = cacheResetTime ?? 5;
            NoSsl = noSsl;
        }

        public async Task<IChangelog> GetChangelogAsync(string query = "", bool ignoreCache = false)
        {
            var result = default(IChangelog);
            try
            {
                var get = $"/changelog/json{query}";
                var model = await GetCacheOrFetch<ChangelogEntryModel[]>(get, ignoreCache).ConfigureAwait(false);
                result = await Task.Run(() => Changelog.Create(this, query, model)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (Log != null)
                    await Log.Invoke(this, new LogMessage(typeof(IChangelog), ex)).ConfigureAwait(false);
            }
            return result;
        }
        public async Task<IChangelog> GetChangelogAsync(Action<ChangelogQuery> setChangelog, bool ignoreCache = false)
        {
            var result = default(IChangelog);
            try
            {
                var temp = new ChangelogQuery();
                await Task.Run(() => setChangelog.Invoke(temp)).ConfigureAwait(false);

                var query = await Task.Run(() => temp.GetString()).ConfigureAwait(false);
                var get = $"/changelog/json{query}";
                var model = await GetCacheOrFetch<ChangelogEntryModel[]>(get, ignoreCache).ConfigureAwait(false);
                result = await Task.Run(() => Changelog.Create(this, query, model)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (Log != null)
                    await Log.Invoke(this, new LogMessage(typeof(IChangelog), ex)).ConfigureAwait(false);
            }
            return result;
        }
        public async Task<IChangelog> GetChangelogAsync(Func<ChangelogQuery> setChangelog, bool ignoreCache = false)
        {
            var result = default(IChangelog);
            try
            {
                var query = await Task.Run(() => setChangelog.Invoke().GetString()).ConfigureAwait(false);
                var get = $"/changelog/json{query}";
                var model = await GetCacheOrFetch<ChangelogEntryModel[]>(get, ignoreCache).ConfigureAwait(false);
                result = await Task.Run(() => Changelog.Create(this, query, model)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (Log != null)
                    await Log.Invoke(this, new LogMessage(typeof(IChangelog), ex)).ConfigureAwait(false);
            }
            return result;
        }
        public async Task<IChamber> GetChamberAsync(ulong chamberId, bool ignoreCache = false)
        {
            var result = default(IChamber);
            try
            {
                var get = $"/chamber/{chamberId}/json";
                var model = await GetCacheOrFetch<IReadOnlyDictionary<ulong, ChamberEntryModel>>(get, ignoreCache).ConfigureAwait(false);
                result = await Task.Run(() => Chamber.Create(this, chamberId, model)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (Log != null)
                    await Log.Invoke(this, new LogMessage(typeof(IChamber), ex)).ConfigureAwait(false);
            }
            return result;
        }
        public async Task<IChamber> GetChamberAsync(Portal2Map map, bool ignoreCache = false)
        {
            if (!map.Exists)
                throw new InvalidOperationException("Map does not have a leaderboard.");

            var result = default(IChamber);
            try
            {
                var get = $"/chamber/{map.BestTimeId}/json";
                var model = await GetCacheOrFetch<IReadOnlyDictionary<ulong, ChamberEntryModel>>(get, ignoreCache).ConfigureAwait(false);
                result = await Task.Run(() => Chamber.Create(this, (ulong)map.BestTimeId, model)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (Log != null)
                    await Log.Invoke(this, new LogMessage(typeof(IChamber), ex)).ConfigureAwait(false);
            }
            return result;
        }
        public async Task<IProfile> GetProfileAsync(string boardName, bool ignoreCache = false)
        {
            var result = default(IProfile);
            try
            {
                var get = $"/profile/{boardName.Replace(" ", string.Empty)}/json";
                var model = await GetCacheOrFetch<ProfileModel>(get, ignoreCache).ConfigureAwait(false);
                result = await Task.Run(() => Profile.Create(this, model)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (Log != null)
                    await Log.Invoke(this, new LogMessage(typeof(IProfile), ex)).ConfigureAwait(false);
            }
            return result;
        }
        public async Task<IProfile> GetProfileAsync(ulong steamId, bool ignoreCache = false)
        {
            var result = default(IProfile);
            try
            {
                var get = $"/profile/{steamId}/json";
                var model = await GetCacheOrFetch<ProfileModel>(get, ignoreCache).ConfigureAwait(false);
                result = await Task.Run(() => Profile.Create(this, model)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (Log != null)
                    await Log.Invoke(this, new LogMessage(typeof(IProfile), ex)).ConfigureAwait(false);
            }
            return result;
        }
        public async Task<Aggregated> GetAggregatedAsync(AggregatedMode mode = default, bool ignoreCache = false)
        {
            var url = "/overall";
            switch (mode)
            {
                case AggregatedMode.SinglePlayer:
                    url = "/sp";
                    break;
                case AggregatedMode.Cooperative:
                    url = "/coop";
                    break;
                case AggregatedMode.Chapter:
                    throw new InvalidOperationException("Invalid mode. Use ChapterType instead of AggregatedMode.");
            }

            var result = default(Aggregated);
            try
            {
                var get = $"/aggregated{url}/json";
                var model = await GetCacheOrFetch<AggregatedModel>(get, ignoreCache).ConfigureAwait(false);
                result = await Task.Run(() => Aggregated.Create(this, mode, ChapterType.None, model)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (Log != null)
                    await Log.Invoke(this, new LogMessage(typeof(Aggregated), ex)).ConfigureAwait(false);
            }
            return result;
        }
        public async Task<Aggregated> GetAggregatedAsync(ChapterType chapter, bool ignoreCache = false)
        {
            if (chapter == ChapterType.None)
                throw new InvalidOperationException("Invalid chapter.");

            var result = default(Aggregated);
            try
            {
                var get = $"/aggregated/chapter/{(int)chapter}/json";
                var model = await GetCacheOrFetch<AggregatedModel>(get, ignoreCache).ConfigureAwait(false);
                result = await Task.Run(() => Aggregated.Create(this, AggregatedMode.Chapter, chapter, model)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (Log != null)
                    await Log.Invoke(this, new LogMessage(typeof(Aggregated), ex)).ConfigureAwait(false);
            }
            return result;
        }
        public async Task<byte[]> GetDemoContentAsync(ulong changelogId, bool ignoreCache = false)
        {
            var result = default(byte[]);
            try
            {
                var get = $"/getDemo?id={changelogId}";
                if (!_autoCache || ignoreCache)
                {
                    result = await _client.GetBytesAsync(BaseApiUrl + get).ConfigureAwait(false);
                }
                else
                {
                    _timer = _timer ?? new Timer(TimerCallback, _autoCache, (int)_cacheResetTime, (int)_cacheResetTime);
                    _cache = _cache ?? new Cache();

                    result = await _cache.Get<byte[]>(get).ConfigureAwait(false)
                        ?? await _client.GetBytesAsync(BaseApiUrl + get).ConfigureAwait(false);
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
        public async Task<IEnumerable<ISteamUser>> GetWallOfShameAsync(bool ignoreCache = false)
        {
            var result = default(IEnumerable<ISteamUser>);
            try
            {
                var get = $"/wallofshame/json";
                var model = await GetCacheOrFetch<WallOfShameModel[]>(get, ignoreCache).ConfigureAwait(false);
                result = await Task.Run(() =>
                {
                    var users = new List<ISteamUser>();
                    foreach (var user in model)
                    {
                        users.Add(SteamUser.Create(this, user.ProfileNumber, user.PlayerName, user.Avatar));
                    }
                    return users;
                }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (Log != null)
                    await Log.Invoke(this, new LogMessage(typeof(IEnumerable<ISteamUser>), ex)).ConfigureAwait(false);
            }
            return result;
        }
        public async Task<IEnumerable<IDonator>> GetDonatorsAsync(bool ignoreCache = false)
        {
            var result = default(IEnumerable<IDonator>);
            try
            {
                var get = $"/donators/json";
                var model = await GetCacheOrFetch<DonatorModel[]>(get, ignoreCache).ConfigureAwait(false);
                result = await Task.Run(() =>
                {
                    var donators = new List<IDonator>();
                    foreach (var donator in model)
                    {
                        donators.Add(Donator.Create(this, donator));
                    }
                    return donators;
                }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (Log != null)
                    await Log.Invoke(this, new LogMessage(typeof(IEnumerable<IDonator>), ex)).ConfigureAwait(false);
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
            return Task.FromResult(false);
        }

        internal async Task<T> GetCacheOrFetch<T>(string url, bool ignoreCache)
            where T : class
        {
            if (!_autoCache || ignoreCache)
                return await _client.GetJsonObjectAsync<T>(BaseApiUrl + url).ConfigureAwait(false);

            // Make sure that time and cache exist
            // This will start the timer when invoking at first api request
            _timer = _timer ?? new Timer(TimerCallback, _autoCache, (int)_cacheResetTime, (int)_cacheResetTime);
            _cache = _cache ?? new Cache();

            var model = await _cache.Get<T>(url).ConfigureAwait(false)
                ?? await _client.GetJsonObjectAsync<T>(BaseApiUrl + url).ConfigureAwait(false);

            // Don't cache BaseApiUrl which ignores changing
            // http/https (why would you do that anyway)
            await _cache.AddOrUpdate(url, model).ConfigureAwait(false);
            return model;
        }

        internal void TimerCallback(Object stateInfo)
        {
            if ((bool)stateInfo)
                _cache = new Cache();
        }

        public void Dispose()
        {
            _client.Dispose();
            // Stop auto cache, because callback can still happen after disposing
            _autoCache = default;
            _timer?.Dispose();
            _cache = null;
            Log = null;
        }
    }
}