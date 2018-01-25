using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Model = Portal2Boards.API.ChangelogEntryModel;

namespace Portal2Boards
{
    [DebuggerDisplay("{Id,nq}")]
	public class ChangelogEntry : IEntity<ulong>, IChangelogEntry
	{
		public ulong Id { get; private set; }
		public DateTime? Date { get; private set; }
		public uint MapId { get; private set; }
		public uint ChapterId { get; private set; }
		public string Name { get; private set; }
		public IEntryData Score { get; private set; }
		public IEntryData Rank { get; private set; }
		public IEntryData Points { get; private set; }
		public ISteamUser Player { get; private set; }
		public bool IsBanned { get; private set; }
		public bool IsSubmission { get; private set; }
		public bool IsWorldRecord { get; private set; }
		public bool DemoExists { get; private set; }
		public string YouTubeId { get; private set; }
		public string Comment { get; private set; }

		public bool CommentExists
			=> !(string.IsNullOrEmpty(Comment));
		public bool VideoExists
			=> !(string.IsNullOrEmpty(YouTubeId));
		
		public string DemoLink
			=> $"https://board.iverb.me/getDemo?id={Id}";
		public string VideoLink
			=> $"https://youtu.be/{YouTubeId}";
		public string Link
			=> $"https://board.iverb.me/chamber/{MapId}";
		public string ImageLink
			=> $"https://board.iverb.me/images/chambers/{MapId}.jpg";
		public string ImageLinkFull
			=> $"https://board.iverb.me/images/chambers_full/{MapId}.jpg";
		public string SteamLink
			=> $"https://steamcommunity.com/stats/Portal2/leaderboards/{MapId}";
		
		internal Portal2BoardsClient Client { get; private set; }

		public async Task<IProfile> GetProfileAsync()
			=> await Client.GetProfileAsync(Player.Id);
		public async Task<IChangelog> GetChangelogAsync()
			=> await Client.GetChangelogAsync($"?profileNumber={Player.Id}");
		public async Task<byte[]> GetDemoContentAsync()
			=> await Client.GetDemoContentAsync(Id);
		
		internal static ChangelogEntry Create(Portal2BoardsClient client, Model model)
		{
			return new ChangelogEntry()
			{
				Id = model.Id,
				Date = (string.IsNullOrEmpty(model.TimeGained))
					? default
					: DateTime.Parse(model.TimeGained),
				MapId = model.MapId,
				ChapterId = model.ChapterId,
				Name = model.ChamberName,
				Score = EntryScore.Create(model.Score, model.PreviousScore, model.Improvement),
				Rank = EntryRank.Create(model.PostRank, model.PreRank, model.RankImprovement),
				Points = EntryPoints.Create(model.PostPoints, model.PrePoints, model.PointImprovement),
				Player = SteamUser.Create(client, model.ProfileNumber, model.PlayerName, model.Avatar),
				IsBanned = model.Banned == "1",
				IsSubmission = model.Submission == "1",
				IsWorldRecord = model.WrGain == "1",
				DemoExists = model.HasDemo == "1",
				YouTubeId = model.YouTubeId,
				Comment = WebUtility.HtmlDecode(model.Note),
				Client = client
			};
		}
	}
}