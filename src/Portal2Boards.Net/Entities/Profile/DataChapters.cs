using System;
using System.Collections.Generic;
using Portal2Boards.API;
using Model = Portal2Boards.API.ProfileTimesChamberData;

namespace Portal2Boards
{
    public class DataChapters : IDataChapters
	{
		public uint WorldRecordCount { get; private set; }
		public uint RankSum { get; private set; }
		public uint MapCount { get; private set; }
		public IDataScore BestScore { get; private set; }
		public IDataScore WorstScore { get; private set; }
		public IDataScore OldestScore { get; private set; }
		public IDataScore NewestScore { get; private set; }
		public uint DemoCount { get; private set; }
		public uint YouTubeVideoCount { get; private set; }
		public float? AveragePlace { get; private set; }
		public IReadOnlyDictionary<Chapter, IDataChambers> Chambers { get; private set; }

		internal static DataChapters Create(Model data)
		{
			// Global chapters don't exist
			if (data == null) return default;
			
			var chambers = new Dictionary<Chapter, IDataChambers>();
			foreach (var item in data.Chamber)
			{
				Enum.TryParse<Chapter>(item.Key.ToString(), out var chapter);
				chambers.Add(chapter, DataChambers.Create(item.Value));
			}
			
			return new DataChapters()
			{
				WorldRecordCount = data.NumWrs,
				RankSum = data.RankSum,
				MapCount = data.MapCount,
				BestScore = DataScore.Create(data.BestRank),
				WorstScore = DataScore.Create(data.WorstRank),
				OldestScore = DataScore.Create(data.OldestScore),
				NewestScore = DataScore.Create(data.NewestScore),
				AveragePlace = data.AveragePlace,
				Chambers = chambers
			};
		}
	}
}