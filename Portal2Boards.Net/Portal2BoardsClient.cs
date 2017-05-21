﻿using System;
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
		public ResponseType LastResponse { get; internal set; } = ResponseType.Unknown;
		private WebClient _client { get; set; }

		public Portal2BoardsClient(HttpClient client = default(HttpClient))
		{
			_client = new WebClient(client);
		}
		public Portal2BoardsClient(ChangelogParameters parameters, HttpClient client = default(HttpClient))
		{
			_client = new WebClient(client);
			Parameters = parameters;
		}

		public async Task<Changelog> GetChangelogAsync(string query = default(string))
		{
			var result = default(Changelog);
			try
			{
				var url = $"{BaseApiUrl}/changelog/json{((string.IsNullOrEmpty(query)) ? await Parameters.ToQuery().ConfigureAwait(false) : query)}";
				var obj = await _client.GetJsonObjectAsync<ChangelogData[]>(url).ConfigureAwait(false);
				result = new Changelog(obj, url);
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
				var obj = await _client.GetJsonObjectAsync<IReadOnlyDictionary<ulong, BoardEntryData>>(url).ConfigureAwait(false);
				result = new Board(obj, url);
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
				var obj = await _client.GetJsonObjectAsync<IReadOnlyDictionary<ulong, BoardEntryData>>(url).ConfigureAwait(false);
				result = new Board(obj, url);
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
				var url = $"{BaseApiUrl}/profile/{boardName.Trim()}/json";
				var obj = await _client.GetJsonObjectAsync<ProfileData>(url).ConfigureAwait(false);
				result = new Profile(obj, url);
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
				var obj = await _client.GetJsonObjectAsync<ProfileData>(url).ConfigureAwait(false);
				result = new Profile(obj, url);
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
				var obj = await _client.GetJsonObjectAsync<AggregatedData>(url).ConfigureAwait(false);
				result = new Aggregated(obj, url);
				LastResponse = ResponseType.Success;
			}
			catch (Exception e)
			{
				LastResponse = await Logger.LogModelException<Aggregated>(e).ConfigureAwait(false);
			}
			return result;
		}
		public async Task<T> Get<T>(dynamic parameter)
			where T : class, IModel, new()
		{
			var result = new T();
			try
			{
				// Would look better with switch case, have to wait for c# 7.1 tho...
				if (result is Changelog)
				{
					var url = $"{BaseApiUrl}/changelog/json{((string.IsNullOrEmpty(parameter)) ? await Parameters.ToQuery().ConfigureAwait(false) : parameter)}";
					var obj = await _client.GetJsonObjectAsync<ChangelogData[]>(url).ConfigureAwait(false);
					result = new Changelog(obj, url) as T;
					LastResponse = ResponseType.Success;
				}
				else if (result is Board)
				{
					var url = $"{BaseApiUrl}/chamber/{((parameter is Map) ? parameter.BestTimeId : parameter)}/json";
					var obj = await _client.GetJsonObjectAsync<IReadOnlyDictionary<ulong, BoardEntryData>>(url).ConfigureAwait(false);
					result = new Board(obj, url) as T;
					LastResponse = ResponseType.Success;
				}
				else if (result is Profile)
				{
					var url = $"{BaseApiUrl}/profile/{((parameter is string) ? parameter.Trim() : parameter)}/json";
					var obj = await _client.GetJsonObjectAsync<ProfileData>(url).ConfigureAwait(false);
					result = new Profile(obj, url) as T;
					LastResponse = ResponseType.Success;
				}
				else if (result is Aggregated)
				{
					Enum.TryParse<Chapter>(parameter?.ToString(), out Chapter id);
					var url = $"{BaseApiUrl}/aggregated/{await GetMode(id).ConfigureAwait(false)}/json";
					var obj = await _client.GetJsonObjectAsync<AggregatedData>(url).ConfigureAwait(false);
					result = new Aggregated(obj, url) as T;
					LastResponse = ResponseType.Success;
				}
			}
			catch (Exception e)
			{
				LastResponse = await Logger.LogModelException<T>(e).ConfigureAwait(false);
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