using System;
using System.Diagnostics;
using Portal2Boards.Net.API.Models;
using Portal2Boards.Net.Utilities;

namespace Portal2Boards.Net.Entities
{
	[DebuggerDisplay("{Id,nq}")]
	public class EntryData : IEntity
	{
		public uint Id { get; set; }
		public DateTime? Date { get; set; }
		public EntryMap Map { get; set; }
		public EntryScore Score { get; set; }
		public EntryRank Rank { get; set; }
		public EntryPoints Points { get; set; }
		public User Player { get; set; }
		public bool IsBanned { get; set; }
		public bool IsSubmission { get; set; }
		public bool IsWorldRecord { get; set; }
		public bool DemoExists { get; set; }
		public string YouTubeId { get; set; }
		public string Comment { get; set; }
		public bool CommentExists
			=> !(string.IsNullOrEmpty(Comment));
		public string DemoLink
			=> $"https://board.iverb.me/getDemo?id={Id}";
		public bool VideoExists
			=> !(string.IsNullOrEmpty(YouTubeId));
		public string VideoLink
			=> $"https://youtu.be/{YouTubeId}";
		public string Link
			=> $"https://board.iverb.me/chamber/{Map.SteamId}";
		public string ImageLink
			=> $"https://board.iverb.me/images/chambers/{Map.SteamId}.jpg";
		public string ImageLinkFull
			=> $"https://board.iverb.me/images/chambers_full/{Map.SteamId}.jpg";

		public EntryData()
		{
		}
		public EntryData(ChangelogData changelog)
		{
			try
			{
				if (changelog != default(ChangelogData))
				{
					Id = changelog.Id;
					Date = (string.IsNullOrEmpty(changelog.TimeGained)) ? default(DateTime?) : DateTime.Parse(changelog.TimeGained);
					Map = new EntryMap(changelog.MapId, changelog.ChapterId, changelog.ChamberName);
					Score = new EntryScore(changelog.Score, changelog.PreviousScore, changelog.Improvement);
					Rank = new EntryRank(changelog.PostRank, changelog.PreRank, changelog.RankImprovement);
					Points = new EntryPoints(changelog.PostPoints, changelog.PrePoints, changelog.PointImprovement);
					Player = new User(changelog.PlayerName, changelog.Avatar, changelog.ProfileNumber);
					IsBanned = changelog.Banned == "1";
					IsSubmission = changelog.Submission == "1";
					IsWorldRecord = changelog.WrGain == "1";
					DemoExists = changelog.HasDemo == "1";
					YouTubeId = changelog.YouTubeId;
					Comment = changelog.Note;
				}
			}
			catch (Exception e)
			{
				Logger.LogEntityException<EntryData>(e).GetAwaiter().GetResult();
			}
		}
	}
}