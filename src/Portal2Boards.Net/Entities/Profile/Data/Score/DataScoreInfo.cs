using Portal2Boards.API;
using Model = Portal2Boards.API.ProfilePointsDataModel;

namespace Portal2Boards
{
	public class DataScoreInfo : IDataScoreInfo
	{
		public uint? Score { get; protected set; }
		public uint? PlayerRank { get; protected set; }
		public uint? ScoreRank { get; protected set; }
		public float? DeltaToWorldRecord { get; protected set; }
		public int? DeltaToNextRank { get; protected set; }

		internal static DataScoreInfo Create(Model model)
		{
			if (model == null) return default;

			return new DataScoreInfo()
			{
				Score = model.Score,
				PlayerRank = model.PlayerRank,
				ScoreRank = model.ScoreRank,
				DeltaToWorldRecord = model.WrDiff,
				DeltaToNextRank = model.NextRankDiff
			};
		}
	}
}