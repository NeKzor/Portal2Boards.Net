using System;

namespace Portal2Boards
{
    public interface IDataScore
	{
		string MapId { get; }
		string Comment { get; }
		bool IsSubmission { get; }
		uint Id { get; }
		uint? PlayerRank { get; }
		uint? ScoreRank { get; }
		uint? Score { get; }
		DateTime? Date { get; }
		bool DemoExists { get; }
		string YouTubeId { get; }
	}
}