using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using Portal2Boards.API.Models;
using Portal2Boards.Extensions;

namespace Portal2Boards
{
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
			if (data != default(ProfilePoints))
			{
				Score = data.Score;
				PlayerRank = data.PlayerRank;
				ScoreRank = data.ScoreRank;
				DeltaToWorldRecord = data.WrDiff;
				DeltaToNextRank = data.NextRankDiff;
			}
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
			if (data != default(ProfileTimesData))
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
	}

	public class DataScore
	{
		public string MapId { get; set; }
		public string Comment { get; set; }
		public bool IsSubmission { get; set; }
		public uint Id { get; set; }
		public uint? PlayerRank { get; set; }
		public uint? ScoreRank { get; set; }
		public uint? Score { get; set; }
		public DateTime? Date { get; set; }
		public bool DemoExists { get; set; }
		public string YouTubeId { get; set; }
		public bool CommentExists
			=> !(string.IsNullOrEmpty(Comment));
		public string DemoLink
			=> $"https://board.iverb.me/getDemo?id={Id}";
		public bool VideoExists
			=> !(string.IsNullOrEmpty(YouTubeId));
		public string VideoLink
			=> $"https://youtu.be/{YouTubeId}";
		public Map ParsedMap
			=> Portal2.GetMapById((MapId != "several chambers") ? uint.Parse(MapId) : 0);
		public string Link
			=> $"https://board.iverb.me/chamber/{ParsedMap?.BestTimeId}";
		public string ImageLink
			=> $"https://board.iverb.me/images/chambers/{ParsedMap?.BestTimeId}.jpg";
		public string ImageLinkFull
			=> $"https://board.iverb.me/images/chambers_full/{ParsedMap?.BestTimeId}.jpg";

		public DataScore()
		{
		}
		public DataScore(ProfileScore data)
		{
			if (data != default(ProfileScore))
			{
				MapId = data.Map;
				Comment = WebUtility.HtmlDecode(data.ScoreData.Note);
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
	}

	public class Times : Points
	{
		public DataChapters Chapters { get; set; }

		public Times()
		{
		}
		public Times(ProfileTimes data)
		{
			if (data != default(ProfileTimes))
			{
				Score = data.Score;
				PlayerRank = data.PlayerRank;
				ScoreRank = data.ScoreRank;
				DeltaToWorldRecord = data.WrDiff;
				DeltaToNextRank = data.NextRankDiff;
				if (data.Chambers != default(ProfileTimesChamberData))
					Chapters = new DataChapters(data.Chambers);
			}
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
			if (data != default(ProfileTimesChamberData))
			{
				WorldRecordCount = data.NumWrs;
				RankSum = data.RankSum;
				MapCount = data.MapCount;
				BestScore = new DataScore(data.BestRank);
				WorstScore = new DataScore(data.WorstRank);
				OldestScore = new DataScore(data.OldestScore);
				NewestScore = new DataScore(data.NewestScore);
				AveragePlace = data.AveragePlace;
				if (data.Chamber != default(IReadOnlyDictionary<uint, IReadOnlyDictionary<uint, ProfileTimesMapData>>))
				{
					var temp = new Dictionary<Chapter, DataChambers>();
					foreach (var item in data.Chamber)
					{
						Enum.TryParse<Chapter>(item.Key.ToString(), out var chapter);
						temp.Add(chapter, new DataChambers(item.Value));
					}
					Chambers = temp;
				}
			}
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
			if (data != default(IReadOnlyDictionary<uint, ProfileTimesMapData>))
			{
				var temp = new Dictionary<uint, MapData>();
				foreach (var item in data)
					temp.Add(item.Key, new MapData(item.Value));
				Data = temp;
			}
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
			if (data != default(ProfileTimesMapData))
			{
				Comment = WebUtility.HtmlDecode(data.Note);
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
}