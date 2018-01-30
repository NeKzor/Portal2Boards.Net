using System.Collections.Generic;

namespace Portal2Boards
{
	public interface IDataTimes
	{
		IDataScoreInfo SinglePlayer { get; }
		IDataScoreInfo Cooperative { get; }
		IDataScoreInfo Global { get; }
		IDataChapters SinglePlayerChapters { get; }
		IDataChapters CooperativeChapters { get; }
		IReadOnlyDictionary<ChapterType, IDataScoreInfo> Chapters { get; }
	}
}