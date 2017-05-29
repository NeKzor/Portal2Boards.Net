using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Portal2Boards.Net.Utilities
{
	internal class Cache
	{
		private ConcurrentDictionary<string, object> _appCache { get; }

		public Cache() => _appCache = new ConcurrentDictionary<string, object>();

		public Task<T> Get<T>(string key)
		{
			if (_appCache.ContainsKey(key))
				if (_appCache.TryGetValue(key, out var cache))
					return Task.FromResult((T)cache);
			return Task.FromResult(default(T));
		}
		public Task<bool> AddOrUpdate(string key, object cache)
		{
			if (cache is null)
				return Task.FromResult(false);
			if (!_appCache.ContainsKey(key))
				return Task.FromResult(_appCache.TryAdd(key, cache));
			return Task.FromResult(_appCache.TryUpdate(key, cache, cache));
		}
	}
}