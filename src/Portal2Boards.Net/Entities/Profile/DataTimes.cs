using System;
using System.Collections.Generic;
using System.Linq;
using Portal2Boards.API;
using Portal2Boards.Extensions;
using Model = Portal2Boards.API.ProfileTimesDataModel;

namespace Portal2Boards
{
    public class DataTimes : IDataTimes
	{
		public IPoints SinglePlayer { get; private set; }
		public IPoints Cooperative { get; private set; }
		public IPoints Global { get; private set; }
		public IDataChapters SinglePlayerChapters { get; private set; }
		public IDataChapters CooperativeChapters { get; private set; }
		public IReadOnlyDictionary<Chapter, IPoints> Chapters { get; private set; }

		public IMapData GetMapData(Portal2Map map)
			=> SinglePlayerChapters.Chambers
				.FirstOrDefault(chapter => chapter.Key == (Chapter)map.ChapterId).Value?.Data
				.FirstOrDefault(chamber => chamber.Key == map.BestTimeId).Value
			?? CooperativeChapters.Chambers
				.FirstOrDefault(chapter => chapter.Key == (Chapter)map.ChapterId).Value?.Data
				.FirstOrDefault(chamber => chamber.Key == map.BestTimeId).Value;
		
		internal static DataTimes Create(Model model)
		{
			var chapters = new Dictionary<Chapter, IPoints>();
			foreach (var item in model.Chapters)
			{
				Enum.TryParse<Chapter>(item.Key, out var chapter);
				chapters.Add(chapter, Points.Create(item.Value));
			}
			
			return new DataTimes()
			{
				SinglePlayer = Points.Create(model.Sp),
				Cooperative = Points.Create(model.Coop),
				Global = Points.Create(model.Global),
				SinglePlayerChapters = DataChapters.Create(model.Sp.Chambers),
				CooperativeChapters = DataChapters.Create(model.Coop.Chambers),
				Chapters = chapters
			};
		}
	}
}