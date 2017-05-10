using System;
using System.Diagnostics;
using Portal2Boards.Net.API.Models;

namespace Portal2Boards.Net.Entities
{
	[DebuggerDisplay("{EntryId,nq}")]
	public class EntryData
	{
		public uint EntryId { get; set; }
		public DateTime? Date { get; set; }
		public EntryMap Map { get; set; }
		public EntryScore Score { get; set; }
		public EntryRank Rank { get; set; }
		public EntryPoints Points { get; set; }
		public EntryPlayer Player { get; set; }
		public bool IsBanned { get; set; }
		public bool IsSubmission { get; set; }
		public bool IsWorldRecord { get; set; }
		public bool HasDemo { get; set; }
		public string YouTubeId { get; set; }
		public string Comment { get; set; }

		public EntryData()
		{
		}
		public EntryData(ChangelogData data)
		{
			if (data != default(ChangelogData))
			{
				EntryId = data.Id;
				Date = (data.TimeGained != default(string)) ? DateTime.Parse(data.TimeGained) : default(DateTime?);
				Map = new EntryMap(data.MapId, data.ChapterId, data.ChamberName);
				Score = new EntryScore(data.Score, data.PreviousScore, data.Improvement);
				Rank = new EntryRank(data.PostRank, data.PreRank, data.RankImprovement);
				Points = new EntryPoints(data.PostPoints, data.PrePoints, data.PointImprovement);
				Player = new EntryPlayer(data.PlayerName, data.Avatar, data.ProfileNumber);
				IsBanned = data.Banned == "1";
				IsSubmission = data.Submission == "1";
				IsWorldRecord = data.WrGain == "1";
				HasDemo = data.HasDemo == "1";
				YouTubeId = data.YouTubeId;
				Comment = data.Note;
			}
		}
	}
}