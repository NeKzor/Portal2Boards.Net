using System;
using System.Diagnostics;
using Portal2Boards.Net.API.Models;

namespace Portal2Boards.Net.Entities
{
	[DebuggerDisplay("{EntryId,nq}")]
	public class EntryData
	{
		public uint Id { get; set; }
		public DateTime? Date { get; set; }
		public EntryMap Map { get; set; }
		public EntryScore Score { get; set; }
		public EntryRank Rank { get; set; }
		public EntryPoints Points { get; set; }
		public EntryPlayer Player { get; set; }
		public bool IsBanned { get; set; }
		public bool IsSubmission { get; set; }
		public bool IsWorldRecord { get; set; }
		public bool DemoExists { get; set; }
		public string YouTubeId { get; set; }
		public string Comment { get; set; }
		public string DemoLink
			=> $"https://board.iverb.me/getDemo?id={Id}";

		public EntryData()
		{
		}
		public EntryData(ChangelogData changelog)
		{
			if (changelog != default(ChangelogData))
			{
				Id = changelog.Id;
				Date = (string.IsNullOrEmpty(changelog.TimeGained)) ? default(DateTime?) : DateTime.Parse(changelog.TimeGained);
				Map = new EntryMap(changelog.MapId, changelog.ChapterId, changelog.ChamberName);
				Score = new EntryScore(changelog.Score, changelog.PreviousScore, changelog.Improvement);
				Rank = new EntryRank(changelog.PostRank, changelog.PreRank, changelog.RankImprovement);
				Points = new EntryPoints(changelog.PostPoints, changelog.PrePoints, changelog.PointImprovement);
				Player = new EntryPlayer(changelog.PlayerName, changelog.Avatar, changelog.ProfileNumber);
				IsBanned = changelog.Banned == "1";
				IsSubmission = changelog.Submission == "1";
				IsWorldRecord = changelog.WrGain == "1";
				DemoExists = changelog.HasDemo == "1";
				YouTubeId = changelog.YouTubeId;
				Comment = changelog.Note;
			}
		}
	}
}