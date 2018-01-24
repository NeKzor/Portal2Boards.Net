using System.Collections.Generic;

namespace Portal2Boards
{
    public interface IAggregated
	{
		IReadOnlyCollection<IAggregatedEntry> Points { get; }
		IReadOnlyCollection<IAggregatedEntry> Times { get; }
	}
}