using System.Collections.Generic;

namespace Portal2Boards
{
    public interface IChangelog
	{
		IReadOnlyCollection<IChangelogEntry> Entries { get; set; }
	}
}