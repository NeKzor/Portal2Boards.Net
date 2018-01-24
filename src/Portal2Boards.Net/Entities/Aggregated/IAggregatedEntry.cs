namespace Portal2Boards
{
    public interface IAggregatedEntry
	{
		IUser Player { get; }
		uint Score { get; }
		uint PlayerRank { get; }
		uint ScoreRank { get; }
	}
}