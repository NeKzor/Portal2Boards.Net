using Portal2Boards.API;

namespace Portal2Boards
{
    public class Points : IPoints
	{
		public uint? Score { get; protected set; }
		public uint? PlayerRank { get; protected set; }
		public uint? ScoreRank { get; protected set; }
		public float? DeltaToWorldRecord { get; protected set; }
		public int? DeltaToNextRank { get; protected set; }

		internal static Points Create(ProfilePointsDataModel data)
		{
			return new Points()
			{
				Score = data.Score,
				PlayerRank = data.PlayerRank,
				ScoreRank = data.ScoreRank,
				DeltaToWorldRecord = data.WrDiff,
				DeltaToNextRank = data.NextRankDiff
			};
		}
	}
}