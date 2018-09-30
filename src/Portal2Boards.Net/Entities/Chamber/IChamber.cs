using System.Collections.Generic;

namespace Portal2Boards
{
    public interface IChamber
    {
        IReadOnlyCollection<IChamberEntry> Entries { get; }
    }
}
