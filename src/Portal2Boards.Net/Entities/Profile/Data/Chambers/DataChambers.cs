using System.Collections.Generic;
using Portal2Boards.API;
using Model = System.Collections.Generic.IReadOnlyDictionary<uint, Portal2Boards.API.ProfileTimesMapModel>;

namespace Portal2Boards
{
    public class DataChambers : IDataChambers
    {
        public IReadOnlyDictionary<uint, IMapData> Data { get; private set; }

        internal static DataChambers Create(Model model)
        {
            if (model == null) return default;

            var data = new Dictionary<uint, IMapData>();
            foreach (var item in model)
                data.Add(item.Key, MapData.Create(item.Value));

            return new DataChambers()
            {
                Data = data
            };
        }
    }
}
