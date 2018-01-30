using System;
using System.Collections.Generic;
using Portal2Boards.API;
using Model = Portal2Boards.API.ProfileTimesChamberData;

namespace Portal2Boards
{
	public class DataChapters : IDataChapters
	{
		public uint WorldRecords { get; private set; }
		public uint SumOfRanks { get; private set; }
		public uint Maps { get; private set; }
		public IDataScore BestScore { get; private set; }
		public IDataScore WorstScore { get; private set; }
		public IDataScore OldestScore { get; private set; }
		public IDataScore NewestScore { get; private set; }
		public uint Demos { get; private set; }
		public uint YouTubeVideos { get; private set; }
		public float? AveragePlace { get; private set; }
		public IReadOnlyDictionary<ChapterType, IDataChambers> Chambers { get; private set; }

		internal static DataChapters Create(Portal2BoardsClient client, Model model)
		{
			if (model == null) return default;

			var chambers = new Dictionary<ChapterType, IDataChambers>();
			if (model.Chamber != null)
			{
				foreach (var item in model.Chamber)
				{
					chambers.Add((ChapterType)item.Key, DataChambers.Create(item.Value));
				}
			}

			return new DataChapters()
			{
				WorldRecords = model.NumWrs,
				SumOfRanks = model.RankSum,
				Maps = model.MapCount,
				BestScore = DataScore.Create(client, model.BestRank),
				WorstScore = DataScore.Create(client, model.WorstRank),
				OldestScore = DataScore.Create(client, model.OldestScore),
				NewestScore = DataScore.Create(client, model.NewestScore),
				AveragePlace = model.AveragePlace,
				Chambers = chambers
			};
		}
	}
}