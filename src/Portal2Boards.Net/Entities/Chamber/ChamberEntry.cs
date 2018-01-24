using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Model = Portal2Boards.API.Models.ChamberEntryModel;

namespace Portal2Boards
{
    [DebuggerDisplay("{Id,nq}")]
	public class ChamberEntry : IEntity, IChamberEntry
	{
		public ulong Id { get; private set; }
		public uint ChangelogId { get; private set; }
		public DateTime? Date { get; private set; }
		public IUser Player { get; private set; }
		public uint? PlayerRank { get; private set; }
		public uint? ScoreRank { get; private set; }
		public uint? Score { get; private set; }
		public bool DemoExists { get; private set; }
		public string YouTubeId { get; private set; }
		public bool IsSubmission { get; private set; }
		public string Comment { get; private set; }

		public string DemoLink
			=> $"https://board.iverb.me/getDemo?id={ChangelogId}";
		public string VideoLink
			=> $"https://youtu.be/{YouTubeId}";
		
		public bool CommentExists
			=> !(string.IsNullOrEmpty(Comment));
		public bool VideoExists
			=> !(string.IsNullOrEmpty(YouTubeId));
		
		internal Portal2BoardsClient Client { get; private set; }

		public async Task<IProfile> GetProfileAsync()
			=> await Client.GetProfileAsync(Id);
		public async Task<byte[]> GetDemoContentAsync()
			=> await Client.GetDemoContentAsync(ChangelogId);

		internal static ChamberEntry Create(Portal2BoardsClient client, ulong id, Model model)
		{
			return new ChamberEntry()
			{
				Id = id,
				ChangelogId = model.Score.ChangelogId,
				Date = (string.IsNullOrEmpty(model.Score.Date))
					? default(DateTime?)
					: DateTime.Parse(model.Score.Date),
				Player = User.Create(model.User.BoardName, model.User.Avatar, id),
				PlayerRank = model.Score.PlayerRank,
				ScoreRank = model.Score.ScoreRank,
				Score = model.Score.Score,
				DemoExists = model.Score.HasDemo == "1",
				YouTubeId = model.Score.YouTubeId,
				IsSubmission = model.Score.Submission == "1",
				Comment = WebUtility.HtmlDecode(model.Score.Note),
				Client = client
			};
		}
	}
}