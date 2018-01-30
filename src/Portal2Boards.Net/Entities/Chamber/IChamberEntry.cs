using System;

namespace Portal2Boards
{
	public interface IChamberEntry
	{
		uint ChangelogId { get; }
		DateTime? Date { get; }
		ISteamUser Player { get; }
		uint? PlayerRank { get; }
		uint? ScoreRank { get; }
		uint? Score { get; }
		bool DemoExists { get; }
		string YouTubeId { get; }
		bool IsSubmission { get; }
		string Comment { get; }
	}
}