using System;
using System.Collections.Generic;
using System.Net.Http;
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
		public const string BaseApiUrl = "https://board.iverb.me";
		public ChangelogParameters Parameters { get; set; }
		private WebClient _client { get; set; }

		public Portal2BoardsClient()
		{
			_client = new WebClient();
			Parameters = new ChangelogParameters();
		}
		public Portal2BoardsClient(HttpClient client = default(HttpClient))
		{
			_client = new WebClient(client);
			Parameters = new ChangelogParameters();
		}
		public Portal2BoardsClient(ChangelogParameters parameters = default(ChangelogParameters), HttpClient client = default(HttpClient))
		{
			_client = new WebClient(client);
			Parameters = parameters ?? new ChangelogParameters();
		}

		public async Task<Changelog> GetChangelogAsync(string query = default(string))
		{
			var result = default(Changelog);
			try
			{
				result = new Changelog(await _client.GetJsonObjectAsync<ChangelogData[]>($"{BaseApiUrl}/changelog/json{((string.IsNullOrEmpty(query)) ? await Parameters.ToQuery().ConfigureAwait(false) : query)}").ConfigureAwait(false));
			}
			catch (Exception e)
			{
				await Logger.LogException<Changelog>(e).ConfigureAwait(false);
			}
			return result;
		}
		public async Task<Board> GetLeaderboardAsync(uint chamberId)
		{
			var result = default(Board);
			try
			{
				result = new Board(await _client.GetJsonObjectAsync<Dictionary<ulong, BoardEntryData>>($"{BaseApiUrl}/chamber/{chamberId}/json").ConfigureAwait(false));
			}
			catch (Exception e)
			{
				await Logger.LogException<Board>(e).ConfigureAwait(false);
			}
			return result;
		}
		public async Task<Board> GetLeaderboardAsync(Map map)
		{
			var result = default(Board);
			try
			{
				result = new Board(await _client.GetJsonObjectAsync<Dictionary<ulong, BoardEntryData>>($"{BaseApiUrl}/chamber/{map.BestTimeId}/json").ConfigureAwait(false));
			}
			catch (Exception e)
			{
				await Logger.LogException<Board>(e).ConfigureAwait(false);
			}
			return result;
		}
		public async Task<Profile> GetProfileAsync(string boardName)
		{
			var result = default(Profile);
			try
			{
				result = new Profile(await _client.GetJsonObjectAsync<ProfileData>($"{BaseApiUrl}/profile/{boardName.Trim()}/json").ConfigureAwait(false));
			}
			catch (Exception e)
			{
				await Logger.LogException<Profile>(e).ConfigureAwait(false);
			}
			return result;
		}
		public async Task<Profile> GetProfileAsync(ulong steamId)
		{
			var result = default(Profile);
			try
			{
				result = new Profile(await _client.GetJsonObjectAsync<ProfileData>($"{BaseApiUrl}/profile/{steamId}/json").ConfigureAwait(false));
			}
			catch (Exception e)
			{
				await Logger.LogException<Profile>(e).ConfigureAwait(false);
			}
			return result;
		}
		public async Task<Aggregated> GetAggregatedAsync(Chapter id = default(Chapter))
		{
			var result = default(Aggregated);
			try
			{
				// TODO: wait for api fix
				result = new Aggregated(await _client.GetJsonObjectAsync<Dictionary<string, AggregatedEntryData>>($"{BaseApiUrl}/aggregated/{await GetMode(id).ConfigureAwait(false)}/json").ConfigureAwait(false));
			}
			catch (Exception e)
			{
				await Logger.LogException<Aggregated>(e).ConfigureAwait(false);
			}
			return result;
		}
		public async Task<T> Get<T>(dynamic obj)
			where T : class, IModel, new()
		{
			var result = new T();
			try
			{
				// Would look better with switch case, have to wait for c# 7.1 tho...
				if (result is Changelog)
				{
					result = new Changelog(await _client.GetJsonObjectAsync<ChangelogData[]>($"{BaseApiUrl}/changelog/json{((string.IsNullOrEmpty(obj)) ? await Parameters.ToQuery().ConfigureAwait(false) : obj)}").ConfigureAwait(false)) as T;
				}
				else if (result is Board)
				{
					result = new Board(await _client.GetJsonObjectAsync<Dictionary<ulong, BoardEntryData>>($"{BaseApiUrl}/chamber/{((obj is Map) ? obj.BestTimeId : obj)}/json").ConfigureAwait(false)) as T;
				}
				else if (result is Profile)
				{
					result = new Profile(await _client.GetJsonObjectAsync<ProfileData>($"{BaseApiUrl}/profile/{((obj is string) ? obj.Trim() : obj)}/json").ConfigureAwait(false)) as T;
				}
				else if (result is Aggregated)
				{
					Enum.TryParse<Chapter>(obj?.ToString(), out Chapter id);
					result = new Aggregated(await _client.GetJsonObjectAsync<Dictionary<string, AggregatedEntryData>>($"{BaseApiUrl}/aggregated/{await GetMode(id).ConfigureAwait(false)}/json").ConfigureAwait(false)) as T;
				}
			}
			catch (Exception e)
			{
				await Logger.LogException<T>(e).ConfigureAwait(false);
			}
			return result;
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
			GC.Collect();
		}
	}
}