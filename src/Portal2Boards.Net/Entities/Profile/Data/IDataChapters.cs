using System.Collections.Generic;

namespace Portal2Boards
{
    public interface IDataChapters
	{
		uint WorldRecordCount { get; }
		uint RankSum { get; }
		uint MapCount { get; }
		IDataScore BestScore { get; }
		IDataScore WorstScore { get; }
		IDataScore OldestScore { get; }
		IDataScore NewestScore { get; }
		uint DemoCount { get; }
		uint YouTubeVideoCount { get; }
		float? AveragePlace { get; }
		IReadOnlyDictionary<Chapter, IDataChambers> Chambers { get; }
	}
}