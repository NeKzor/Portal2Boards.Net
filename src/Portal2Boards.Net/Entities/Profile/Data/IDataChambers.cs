using System.Collections.Generic;

namespace Portal2Boards
{
    public interface IDataChambers
	{
		IReadOnlyDictionary<uint, IMapData> Data { get; }
	}
}