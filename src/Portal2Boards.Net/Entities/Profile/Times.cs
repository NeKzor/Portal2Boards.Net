/* using Portal2Boards.API;

namespace Portal2Boards
{
    public class Times : Points, ITimes
	{
		public IDataChapters Chapters { get; private set; }

		internal static Times Create(ProfileTimesModel data)
		{
			return new Times()
			{
				Score = data.Score,
				PlayerRank = data.PlayerRank,
				ScoreRank = data.ScoreRank,
				DeltaToWorldRecord = data.WrDiff,
				DeltaToNextRank = data.NextRankDiff,
				Chapters = DataChapters.Create(data.Chambers)
			};
		}
	}
} */