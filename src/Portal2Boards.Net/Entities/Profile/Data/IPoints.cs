namespace Portal2Boards
{
    public interface IPoints
	{
		uint? Score { get; }
		uint? PlayerRank { get; }
		uint? ScoreRank { get; }
		float? DeltaToWorldRecord { get; }
		int? DeltaToNextRank { get; }
	}
}