using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Portal2Boards.API;
using Portal2Boards.Extensions;
using Model = Portal2Boards.API.ProfileTimesDataModel;

namespace Portal2Boards
{
	public class DataTimes : IDataTimes
	{
		public IDataScoreInfo SinglePlayer { get; private set; }
		public IDataScoreInfo Cooperative { get; private set; }
		public IDataScoreInfo Global { get; private set; }
		public IDataChapters SinglePlayerChapters { get; private set; }
		public IDataChapters CooperativeChapters { get; private set; }
		public IReadOnlyDictionary<ChapterType, IDataScoreInfo> Chapters { get; private set; }

		public IMapData GetMapData(Portal2Map map)
			=> SinglePlayerChapters.Chambers
				.FirstOrDefault(chapter => chapter.Key == (ChapterType)map.ChapterId).Value?.Data
				.FirstOrDefault(chamber => chamber.Key == map.BestTimeId).Value
			?? CooperativeChapters.Chambers
				.FirstOrDefault(chapter => chapter.Key == (ChapterType)map.ChapterId).Value?.Data
				.FirstOrDefault(chamber => chamber.Key == map.BestTimeId).Value;

		internal static DataTimes Create(Portal2BoardsClient client, Model model)
		{
			if (model == null) return default;

			var chapters = new Dictionary<ChapterType, IDataScoreInfo>();
			if (model.Chapters != null)
			{
				foreach (var item in model.Chapters)
				{
					if (Enum.TryParse<ChapterType>(item.Key, out var chapter))
						chapters.Add(chapter, Portal2Boards.DataScoreInfo.Create(item.Value));
				}
			}

			return new DataTimes()
			{
				SinglePlayer = DataScoreInfo.Create(model.Sp),
				Cooperative = DataScoreInfo.Create(model.Coop),
				Global = DataScoreInfo.Create(model.Global),
				SinglePlayerChapters = DataChapters.Create(client, model.Sp.Chambers),
				CooperativeChapters = DataChapters.Create(client, model.Coop.Chambers),
				Chapters = chapters
			};
		}
	}
}