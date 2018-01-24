using System;
using System.Diagnostics;
using System.Net;
using Model = Portal2Boards.API.Models.ChangelogEntryModel;

namespace Portal2Boards
{
    [DebuggerDisplay("{Id,nq}")]
	public class ChangelogEntry : IChangelogEntry
	{
		public uint Id { get; set; }
		public DateTime? Date { get; set; }
		public EntryMap Map { get; set; }
		public EntryScore Score { get; set; }
		public EntryRank Rank { get; set; }
		public EntryPoints Points { get; set; }
		public IUser Player { get; set; }
		public bool IsBanned { get; set; }
		public bool IsSubmission { get; set; }
		public bool IsWorldRecord { get; set; }
		public bool DemoExists { get; set; }
		public string YouTubeId { get; set; }
		public string Comment { get; set; }

		public bool CommentExists
			=> !(string.IsNullOrEmpty(Comment));
		public bool VideoExists
			=> !(string.IsNullOrEmpty(YouTubeId));
		
		public string DemoLink
			=> $"https://board.iverb.me/getDemo?id={Id}";
		public string VideoLink
			=> $"https://youtu.be/{YouTubeId}";
		public string Link
			=> $"https://board.iverb.me/chamber/{Map.SteamId}";
		
		public string ImageLink
			=> $"https://board.iverb.me/images/chambers/{Map.SteamId}.jpg";
		public string ImageLinkFull
			=> $"https://board.iverb.me/images/chambers_full/{Map.SteamId}.jpg";

		internal static ChangelogEntry Create(Model model)
		{
			return new ChangelogEntry()
			{
				Id = model.Id,
				Date = (string.IsNullOrEmpty(model.TimeGained))
					? default(DateTime?)
					: DateTime.Parse(model.TimeGained),
				Map = new EntryMap(model.MapId, model.ChapterId, model.ChamberName),
				Score = new EntryScore(model.Score, model.PreviousScore, model.Improvement),
				Rank = new EntryRank(model.PostRank, model.PreRank, model.RankImprovement),
				Points = new EntryPoints(model.PostPoints, model.PrePoints, model.PointImprovement),
				Player = User.Create(model.PlayerName, model.Avatar, model.ProfileNumber),
				IsBanned = model.Banned == "1",
				IsSubmission = model.Submission == "1",
				IsWorldRecord = model.WrGain == "1",
				DemoExists = model.HasDemo == "1",
				YouTubeId = model.YouTubeId,
				Comment = WebUtility.HtmlDecode(model.Note)
			};
		}
	}
}