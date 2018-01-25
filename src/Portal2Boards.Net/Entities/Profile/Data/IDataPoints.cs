using System.Collections.Generic;

namespace Portal2Boards
{
    public interface IDataPoints
	{
		IPoints SinglePlayer { get; }
		IPoints Cooperative { get; }
		IPoints Global { get; }
		IReadOnlyDictionary<Chapter, IPoints> Chapters { get; }
	}
}