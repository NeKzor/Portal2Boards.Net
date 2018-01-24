using System;

namespace Portal2Boards
{
    public interface IChangelogEntry
	{
		uint Id { get; set; }
		DateTime? Date { get; set; }
		EntryMap Map { get; set; }
		EntryScore Score { get; set; }
		EntryRank Rank { get; set; }
		EntryPoints Points { get; set; }
		IUser Player { get; set; }
		bool IsBanned { get; set; }
		bool IsSubmission { get; set; }
		bool IsWorldRecord { get; set; }
		bool DemoExists { get; set; }
		string YouTubeId { get; set; }
		string Comment { get; set; }
	}
}