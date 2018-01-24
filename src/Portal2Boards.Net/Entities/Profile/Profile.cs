using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using Portal2Boards.API.Models;

namespace Portal2Boards
{
	public interface IProfile
	{
		ulong SteamId { get; set; }
		string DisplayName { get; set; }
		string BoardName { get; set; }
		string SteamName { get; set; }
		bool IsBanned { get; set; }
		bool IsRegistered { get; set; }
		bool HasRecords { get; set; }
		string SteamAvatarLink { get; set; }
		string TwitchLink { get; set; }
		string YouTubeLink { get; set; }
		string Title { get; set; }
		bool IsAdmin { get; set; }
		DataPoints Points { get; set; }
		DataTimes Times { get; set; }
	}

	[DebuggerDisplay("{SteamId,nq}")]
	public class Profile : IProfile
	{
		public ulong SteamId { get; set; }
		public string DisplayName { get; set; }
		public string BoardName { get; set; }
		public string SteamName { get; set; }
		public bool IsBanned { get; set; }
		public bool IsRegistered { get; set; }
		public bool HasRecords { get; set; }
		public string SteamAvatarLink { get; set; }
		public string TwitchLink { get; set; }
		public string YouTubeLink { get; set; }
		public string Title { get; set; }
		public bool IsAdmin { get; set; }
		public DataPoints Points { get; set; }
		public DataTimes Times { get; set; }

		public string Link
			=> $"https://board.iverb.me/profile/{SteamId}";
		public string SteamLink
			=> $"https://steamcommunity.com/profiles/{SteamId}";
		
		internal static Profile Create(ProfileModel data)
		{
			var chapters = new Dictionary<Chapter, Points>();
			foreach (var item in data.Points.Chapters)
			{
				Enum.TryParse<Chapter>(item.Key, out var chapter);
				chapters.Add(chapter, new Points(item.Value));
			}
			
			return new Profile()
			{
				SteamId = data.ProfileNumber,
				IsRegistered = data.IsRegistered == "1",
				HasRecords = data.HasRecords == "1",
				DisplayName = WebUtility.HtmlDecode(data.UserData.DisplayName),
				BoardName = WebUtility.HtmlDecode(data.UserData.BoardName),
				SteamName = WebUtility.HtmlDecode(data.UserData.SteamName),
				IsBanned = data.UserData.Banned == "1",
				SteamAvatarLink = data.UserData.Avatar,
				TwitchLink = data.UserData.Twitch,
				YouTubeLink = data.UserData.YouTube,
				Title = data.UserData.Title,
				IsAdmin = data.UserData.Admin == "1",
				Points = new DataPoints
				{
					SinglePlayer = new Points(data.Points.Sp),
					Cooperative = new Points(data.Points.Coop),
					Global = new Points(data.Points.Global),
					Chapters = chapters
				},
				Times = new DataTimes(data.Times)
			};
		}
	}
}