using System.Collections.Generic;

namespace Portal2Boards
{
    public interface IDataChapters
	{
		uint WorldRecords { get; }
		uint SumOfRanks { get; }
		uint Maps { get; }
		IDataScore BestScore { get; }
		IDataScore WorstScore { get; }
		IDataScore OldestScore { get; }
		IDataScore NewestScore { get; }
		uint Demos { get; }
		uint YouTubeVideos { get; }
		float? AveragePlace { get; }
		IReadOnlyDictionary<ChapterType, IDataChambers> Chambers { get; }
	}
}