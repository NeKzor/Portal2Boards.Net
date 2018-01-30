using System.Collections.Generic;

namespace Portal2Boards
{
	public interface IDataPoints
	{
		IDataScoreInfo SinglePlayer { get; }
		IDataScoreInfo Cooperative { get; }
		IDataScoreInfo Global { get; }
		IReadOnlyDictionary<ChapterType, IDataScoreInfo> Chapters { get; }
	}
}