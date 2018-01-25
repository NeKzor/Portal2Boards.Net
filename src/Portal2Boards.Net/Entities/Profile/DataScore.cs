using System;
using System.Net;
using Portal2Boards.API;

namespace Portal2Boards
{
    public class DataScore : IDataScore
	{
		public string MapId { get; protected set; }
		public string Comment { get; protected set; }
		public bool IsSubmission { get; protected set; }
		public uint Id { get; protected set; }
		public uint? PlayerRank { get; protected set; }
		public uint? ScoreRank { get; protected set; }
		public uint? Score { get; protected set; }
		public DateTime? Date { get; protected set; }
		public bool DemoExists { get; protected set; }
		public string YouTubeId { get; protected set; }

		public bool CommentExists
			=> !(string.IsNullOrEmpty(Comment));
		public string DemoLink
			=> $"https://board.iverb.me/getDemo?id={Id}";
		public bool VideoExists
			=> !(string.IsNullOrEmpty(YouTubeId));
		public string VideoLink
			=> $"https://youtu.be/{YouTubeId}";
		
		internal static DataScore Create(ProfileScoreModel data)
		{
			return new DataScore()
			{
				MapId = data.Map,
				Comment = WebUtility.HtmlDecode(data.ScoreData.Note),
				IsSubmission = data.ScoreData.Submission == "1",
				Id = data.ScoreData.ChangelogId,
				PlayerRank = data.ScoreData.PlayerRank,
				ScoreRank = data.ScoreData.ScoreRank,
				Score = data.ScoreData.Score,
				Date = (string.IsNullOrEmpty(data.ScoreData.Date))
					? default
					: DateTime.Parse(data.ScoreData.Date),
				DemoExists = data.ScoreData.HasDemo == "1",
				YouTubeId = data.ScoreData.YouTubeId
			};
		}
	}
}