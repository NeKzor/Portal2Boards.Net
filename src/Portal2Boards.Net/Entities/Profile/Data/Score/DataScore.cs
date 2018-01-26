using System;
using System.Net;
using System.Threading.Tasks;
using Portal2Boards.API;
using Model = Portal2Boards.API.ProfileScoreModel;

namespace Portal2Boards
{
    public class DataScore : IEntity<ulong>, IDataScore
	{
		public ulong Id { get; protected set; }
		public string Comment { get; protected set; }
		public bool IsSubmission { get; protected set; }
		public uint ChangelogId { get; protected set; }
		public uint? PlayerRank { get; protected set; }
		public uint? ScoreRank { get; protected set; }
		public uint? Score { get; protected set; }
		public DateTime? Date { get; protected set; }
		public bool DemoExists { get; protected set; }
		public string YouTubeId { get; protected set; }

		public bool CommentExists
			=> !(string.IsNullOrEmpty(Comment));
		public bool VideoExists
			=> !(string.IsNullOrEmpty(YouTubeId));
		
		public string DemoUrl
			=> $"https://board.iverb.me/getDemo?id={Id}";
		public string VideoUrl
			=> $"https://youtu.be/{YouTubeId}";
		
		internal Portal2BoardsClient Client { get; private set; }

		public async Task<IChamber> GetChamberAsync()
			=> await Client.GetChamberAsync(Id);
		public async Task<IChangelog> GetChangelogAsync()
			=> await Client.GetChangelogAsync($"?chamber={Id}");
		public async Task<byte[]> GetDemoContentAsync()
			=> await Client.GetDemoContentAsync(Id);
		
		internal static DataScore Create(Portal2BoardsClient client, Model model)
		{
			if (model == null) return default;

			return new DataScore()
			{
				Id = model.Map,
				Comment = WebUtility.HtmlDecode(model.ScoreData.Note),
				IsSubmission = model.ScoreData.Submission == "1",
				ChangelogId = model.ScoreData.ChangelogId,
				PlayerRank = model.ScoreData.PlayerRank,
				ScoreRank = model.ScoreData.ScoreRank,
				Score = model.ScoreData.Score,
				Date = (!string.IsNullOrEmpty(model.ScoreData.Date)) 
					? DateTime.Parse(model.ScoreData.Date)
					: default,
				DemoExists = model.ScoreData.HasDemo == "1",
				YouTubeId = model.ScoreData.YouTubeId,
				Client = client
			};
		}
	}
}