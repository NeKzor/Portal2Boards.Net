using System.Collections.Generic;

namespace Portal2Boards
{
    public interface IDataTimes
	{
		IPoints SinglePlayer { get; }
		IPoints Cooperative { get; }
		IPoints Global { get; }
		IDataChapters SinglePlayerChapters { get; }
		IDataChapters CooperativeChapters { get; }
		IReadOnlyDictionary<Chapter, IPoints> Chapters { get; }
	}
}