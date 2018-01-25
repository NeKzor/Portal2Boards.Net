using System;
using System.Net;
using Portal2Boards.API;
using Model = Portal2Boards.API.ProfileScoreModel;

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
		
		internal static DataScore Create(Model model)
		{
			if (model == null) return default;

			return new DataScore()
			{
				MapId = model.Map,
				Comment = WebUtility.HtmlDecode(model.ScoreData.Note),
				IsSubmission = model.ScoreData.Submission == "1",
				Id = model.ScoreData.ChangelogId,
				PlayerRank = model.ScoreData.PlayerRank,
				ScoreRank = model.ScoreData.ScoreRank,
				Score = model.ScoreData.Score,
				Date = (string.IsNullOrEmpty(model.ScoreData.Date))
					? default
					: DateTime.Parse(model.ScoreData.Date),
				DemoExists = model.ScoreData.HasDemo == "1",
				YouTubeId = model.ScoreData.YouTubeId
			};
		}
	}
}