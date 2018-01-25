namespace Portal2Boards
{
    public interface IMapData : IDataScore
	{
		float? DeltaToWorldRecord { get; }
		int? DeltaToNextRank { get; }
	}
}