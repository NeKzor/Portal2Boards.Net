using System;
using System.Collections.Generic;
using System.Diagnostics;
using Portal2Boards.Net.API.Models;
using Portal2Boards.Net.Utilities;

namespace Portal2Boards.Net.Entities
{
	[DebuggerDisplay("{SteamId,nq}")]
	public class UserData : IEntity
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
			=> $"http://board.iverb.me/profile/{SteamId}";
		public string SteamLink
			=> $"http://steamcommunity.com/profiles/{SteamId}";

		public UserData()
		{
		}
		public UserData(ProfileData data)
		{
			try
			{
				if (data != default(ProfileData))
				{
					SteamId = data.ProfileNumber;
					IsRegistered = data.IsRegistered == "1";
					HasRecords = data.HasRecords == "1";
					DisplayName = data.UserData.DisplayName;
					BoardName = data.UserData.BoardName;
					SteamName = data.UserData.SteamName;
					IsBanned = data.UserData.Banned == "1";
					SteamAvatarLink = data.UserData.Avatar;
					TwitchLink = data.UserData.Twitch;
					YouTubeLink = data.UserData.YouTube;
					Title = data.UserData.Title;
					IsAdmin = data.UserData.Admin == "1";
					Points = new DataPoints
					{
						SinglePlayer = new Points(data.Points.Sp),
						Cooperative = new Points(data.Points.Coop),
						Global = new Points(data.Points.Global)
					};
					var temp = new Dictionary<Chapter, Points>();
					foreach (var item in data.Points.Chapters)
					{
						Enum.TryParse<Chapter>(item.Key, out var chapter);
						temp.Add(chapter, new Points(item.Value));
					}
					Points.Chapters = temp;
					Times = new DataTimes(data.Times);
				}
			}
			catch (Exception e)
			{
				Logger.LogEntityException<UserData>(e).GetAwaiter().GetResult();
			}
		}
	}

	public class DataPoints
	{
		public Points SinglePlayer { get; set; }
		public Points Cooperative { get; set; }
		public Points Global { get; set; }
		public IReadOnlyDictionary<Chapter, Points> Chapters { get; set; }
	}

	public class Points
	{
		public uint? Score { get; set; }
		public uint? PlayerRank { get; set; }
		public uint? ScoreRank { get; set; }
		public float? DeltaToWorldRecord { get; set; }
		public int? DeltaToNextRank { get; set; }

		public Points()
		{
		}
		public Points(ProfilePoints data)
		{
			Score = data.Score;
			PlayerRank = data.PlayerRank;
			ScoreRank = data.ScoreRank;
			DeltaToWorldRecord = data.WrDiff;
			DeltaToNextRank = data.NextRankDiff;
		}

	}

	public class DataTimes
	{
		public Times SinglePlayer { get; set; }
		public Times Cooperative { get; set; }
		public Times Global { get; set; }
		public IReadOnlyDictionary<Chapter, Points> Chapters { get; set; }
		public uint DemoCount { get; set; }
		public uint YouTubeVideoCount { get; set; }
		public DataScore BestScore { get; set; }
		public DataScore WorstScore { get; set; }
		public DataScore OldestScore { get; set; }
		public DataScore NewestScore { get; set; }
		public uint WorldRecordCount { get; set; }
		public float? GlobalAveragePlace { get; set; }

		public DataTimes()
		{
		}
		public DataTimes(ProfileTimesData data)
		{
			SinglePlayer = new Times(data.Sp);
			Cooperative = new Times(data.Coop);
			Global = new Times(data.Global);
			var temp = new Dictionary<Chapter, Points>();
			foreach (var item in data.Chapters)
			{
				Enum.TryParse<Chapter>(item.Key, out var chapter);
				temp.Add(chapter, new Points(item.Value));
			}
			Chapters = temp;
			DemoCount = data.NumDemos;
			YouTubeVideoCount = data.NumYouTubeVideos;
			BestScore = new DataScore(data.BestRank);
			WorstScore = new DataScore(data.WorstRank);
			OldestScore = new DataScore(data.OldestScore);
			NewestScore = new DataScore(data.NewestScore);
			WorldRecordCount = data.NumWrs;
			GlobalAveragePlace = data.GlobalAveragePlace;
		}
	}

	public class DataScore
	{
		public string MapName { get; set; }
		public string Comment { get; set; }
		public bool IsSubmission { get; set; }
		public uint Id { get; set; }
		public uint? PlayerRank { get; set; }
		public uint? ScoreRank { get; set; }
		public uint? Score { get; set; }
		public DateTime? Date { get; set; }
		public bool DemoExists { get; set; }
		public string YouTubeId { get; set; }
		public string DemoLink
			=> $"https://board.iverb.me/getDemo?id={Id}";
		public bool VideoExists
			=> !(string.IsNullOrEmpty(YouTubeId));
		public string VideoLink
			=> $"https://youtu.be/{YouTubeId}";
		public string Link
			=> $"https://board.iverb.me/chamber/{Id}";

		public DataScore()
		{
		}
		public DataScore(ProfileScore data)
		{
			MapName = data.Map;
			Comment = data.ScoreData.Note;
			IsSubmission = data.ScoreData.Submission == "1";
			Id = data.ScoreData.ChangelogId;
			PlayerRank = data.ScoreData.PlayerRank;
			ScoreRank = data.ScoreData.ScoreRank;
			Score = data.ScoreData.Score;
			Date = (string.IsNullOrEmpty(data.ScoreData.Date)) ? default(DateTime?) : DateTime.Parse(data.ScoreData.Date);
			DemoExists = data.ScoreData.HasDemo == "1";
			YouTubeId = data.ScoreData.YouTubeId;
		}
	}

	public class Times : Points
	{
		public DataChapters Chapters { get; set; }

		public Times()
		{
		}
		public Times(ProfileTimes data)
		{
			Score = data.Score;
			PlayerRank = data.PlayerRank;
			ScoreRank = data.ScoreRank;
			DeltaToWorldRecord = data.WrDiff;
			DeltaToNextRank = data.NextRankDiff;
			if (data.Chambers != null)
				Chapters = new DataChapters(data.Chambers);
		}
	}

	public class DataChapters
	{
		public uint WorldRecordCount { get; set; }
		public uint RankSum { get; set; }
		public uint MapCount { get; set; }
		public DataScore BestScore { get; set; }
		public DataScore WorstScore { get; set; }
		public DataScore OldestScore { get; set; }
		public DataScore NewestScore { get; set; }
		public uint DemoCount { get; set; }
		public uint YouTubeVideoCount { get; set; }
		public float? AveragePlace { get; set; }
		public IReadOnlyDictionary<Chapter, DataChambers> Chambers { get; set; }

		public DataChapters()
		{
		}
		public DataChapters(ProfileTimesChamberData data)
		{
			WorldRecordCount = data.NumWrs;
			RankSum = data.RankSum;
			MapCount = data.MapCount;
			BestScore = new DataScore(data.BestRank);
			WorstScore = new DataScore(data.WorstRank);
			OldestScore = new DataScore(data.OldestScore);
			NewestScore = new DataScore(data.NewestScore);
			AveragePlace = data.AveragePlace;
			var temp = new Dictionary<Chapter, DataChambers>();
			foreach (var item in data.Chamber)
			{
				Enum.TryParse<Chapter>(item.Key.ToString(), out var chapter);
				temp.Add(chapter, new DataChambers(item.Value));
			}
			Chambers = temp;
		}
	}

	public class DataChambers
	{
		public IReadOnlyDictionary<uint, MapData> Data { get; set; }

		public DataChambers()
		{
		}
		public DataChambers(IReadOnlyDictionary<uint, ProfileTimesMapData> data)
		{
			var temp = new Dictionary<uint, MapData>();
			foreach (var item in data)
				temp.Add(item.Key, new MapData(item.Value));
			Data = temp;
		}
	}

	public sealed class MapData : DataScore
	{
		public float? DeltaToWorldRecord { get; set; }
		public int? DeltaToNextRank { get; set; }

		public MapData()
		{
		}
		public MapData(ProfileTimesMapData data)
		{
			//MapName = data.Map;
			Comment = data.Note;
			IsSubmission = data.Submission == "1";
			Id = data.ChangelogId;
			PlayerRank = data.PlayerRank;
			ScoreRank = data.ScoreRank;
			Score = data.Score;
			Date = (string.IsNullOrEmpty(data.Date)) ? default(DateTime?) : DateTime.Parse(data.Date);
			DemoExists = data.HasDemo == "1";
			YouTubeId = data.YouTubeId;
			DeltaToWorldRecord = data.WrDiff;
			DeltaToNextRank = data.NextRankDiff;
		}
	}
}