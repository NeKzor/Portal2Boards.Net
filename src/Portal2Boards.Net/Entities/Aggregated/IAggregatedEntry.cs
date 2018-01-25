namespace Portal2Boards
{
    public interface IAggregatedEntry
	{
		ISteamUser User { get; }
		uint Score { get; }
		uint PlayerRank { get; }
		uint ScoreRank { get; }
	}
}