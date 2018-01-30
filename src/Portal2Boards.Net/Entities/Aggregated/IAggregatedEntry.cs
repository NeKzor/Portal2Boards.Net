namespace Portal2Boards
{
	public interface IAggregatedEntry
	{
		ISteamUser Player { get; }
		uint Score { get; }
		uint PlayerRank { get; }
		uint ScoreRank { get; }
	}
}