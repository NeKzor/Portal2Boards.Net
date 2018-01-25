using System.Collections.Generic;

namespace Portal2Boards
{
    public interface IChangelog
	{
		string Query { get; }
		IReadOnlyCollection<IChangelogEntry> Entries { get; }
	}
}