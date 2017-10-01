using System;
using System.Diagnostics;
using System.Net;
using Portal2Boards.Net.API.Models;

namespace Portal2Boards.Net.Entities
{
	[DebuggerDisplay("{Id,nq}")]
	public class ChamberData : IEntity
	{
		public uint Id { get; set; }
		public DateTime? Date { get; set; }
		public User Player { get; set; }
		public uint? PlayerRank { get; set; }
		public uint? ScoreRank { get; set; }
		public uint? Score { get; set; }
		public bool DemoExists { get; set; }
		public string YouTubeId { get; set; }
		public bool IsSubmission { get; set; }
		public string Comment { get; set; }
		public bool CommentExists
			=> !(string.IsNullOrEmpty(Comment));
		public string DemoLink
			=> $"https://board.iverb.me/getDemo?id={Id}";
		public bool VideoExists
			=> !(string.IsNullOrEmpty(YouTubeId));
		public string VideoLink
			=> $"https://youtu.be/{YouTubeId}";

		public ChamberData()
		{
		}
		public ChamberData(BoardData board)
		{
			if (board != default(BoardData))
			{
				Id = board.Data.Score.ChangelogId;
				Date = (string.IsNullOrEmpty(board.Data.Score.Date)) ? default(DateTime?) : DateTime.Parse(board.Data.Score.Date);
				Player = new User(board.Data.User.BoardName, board.Data.User.Avatar, board.Id);
				PlayerRank = board.Data.Score.PlayerRank;
				ScoreRank = board.Data.Score.ScoreRank;
				Score = board.Data.Score.Score;
				DemoExists = board.Data.Score.HasDemo == "1";
				YouTubeId = board.Data.Score.YouTubeId;
				IsSubmission = board.Data.Score.Submission == "1";
				Comment = WebUtility.HtmlDecode(board.Data.Score.Note);
			}
		}
	}
}