using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using Portal2Boards.API;

namespace Portal2Boards
{
    [DebuggerDisplay("{SteamId,nq}")]
	public class Profile : IProfile
	{
		public ulong SteamId { get; private set; }
		public string DisplayName { get; private set; }
		public string BoardName { get; private set; }
		public string SteamName { get; private set; }
		public bool IsBanned { get; private set; }
		public bool IsRegistered { get; private set; }
		public bool HasRecords { get; private set; }
		public string SteamAvatarLink { get; private set; }
		public string TwitchLink { get; private set; }
		public string YouTubeLink { get; private set; }
		public string Title { get; private set; }
		public bool IsAdmin { get; private set; }
		public IDataPoints Points { get; private set; }
		public IDataTimes Times { get; private set; }
		public uint DemoCount { get; private set; }
		public uint YouTubeVideoCount { get; private set; }
		public IDataScore BestScore { get; private set; }
		public IDataScore WorstScore { get; private set; }
		public IDataScore OldestScore { get; private set; }
		public IDataScore NewestScore { get; private set; }
		public uint WorldRecordCount { get; private set; }
		public float? GlobalAveragePlace { get; private set; }

		public string Link
			=> $"https://board.iverb.me/profile/{SteamId}";
		public string SteamLink
			=> $"https://steamcommunity.com/profiles/{SteamId}";
		
		internal static Profile Create(ProfileModel data)
		{
			var chapters = new Dictionary<Chapter, IPoints>();
			foreach (var item in data.Points.Chapters)
			{
				Enum.TryParse<Chapter>(item.Key, out var chapter);
				chapters.Add(chapter, Portal2Boards.Points.Create(item.Value));
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
				Points = DataPoints.Create
				(
					Portal2Boards.Points.Create(data.Points.Sp),
					Portal2Boards.Points.Create(data.Points.Coop),
					Portal2Boards.Points.Create(data.Points.Global),
					chapters
				),
				Times = DataTimes.Create(data.Times),
				DemoCount = data.Times.NumDemos,
				YouTubeVideoCount = data.Times.NumYouTubeVideos,
				BestScore = DataScore.Create(data.Times.BestRank),
				WorstScore = DataScore.Create(data.Times.WorstRank),
				OldestScore = DataScore.Create(data.Times.OldestScore),
				NewestScore = DataScore.Create(data.Times.NewestScore),
				WorldRecordCount = data.Times.NumWrs,
				GlobalAveragePlace = data.Times.GlobalAveragePlace
			};
		}
	}
}