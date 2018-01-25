using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using Portal2Boards.API;
using Model = Portal2Boards.API.ProfileModel;

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
		
		internal static Profile Create(Model model)
		{
			var chapters = new Dictionary<Chapter, IPoints>();
			if (model.Points?.Chapters != null)
			{
				foreach (var item in model.Points.Chapters)
				{
					Enum.TryParse<Chapter>(item.Key, out var chapter);
					chapters.Add(chapter, Portal2Boards.Points.Create(item.Value));
				}
			}
			
			return new Profile()
			{
				SteamId = model.ProfileNumber,
				IsRegistered = model.IsRegistered == "1",
				HasRecords = model.HasRecords == "1",
				DisplayName = WebUtility.HtmlDecode(model.UserData.DisplayName),
				BoardName = WebUtility.HtmlDecode(model.UserData.BoardName),
				SteamName = WebUtility.HtmlDecode(model.UserData.SteamName),
				IsBanned = model.UserData.Banned == "1",
				SteamAvatarLink = model.UserData.Avatar,
				TwitchLink = model.UserData.Twitch,
				YouTubeLink = model.UserData.YouTube,
				Title = model.UserData.Title,
				IsAdmin = model.UserData.Admin == "1",
				Points = DataPoints.Create
				(
					Portal2Boards.Points.Create(model.Points.Sp),
					Portal2Boards.Points.Create(model.Points.Coop),
					Portal2Boards.Points.Create(model.Points.Global),
					chapters
				),
				Times = DataTimes.Create(model.Times),
				DemoCount = model.Times.NumDemos,
				YouTubeVideoCount = model.Times.NumYouTubeVideos,
				BestScore = DataScore.Create(model.Times.BestRank),
				WorstScore = DataScore.Create(model.Times.WorstRank),
				OldestScore = DataScore.Create(model.Times.OldestScore),
				NewestScore = DataScore.Create(model.Times.NewestScore),
				WorldRecordCount = model.Times.NumWrs,
				GlobalAveragePlace = model.Times.GlobalAveragePlace
			};
		}
	}
}