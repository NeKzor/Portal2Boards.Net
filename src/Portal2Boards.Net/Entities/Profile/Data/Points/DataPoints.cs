using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Portal2Boards
{
    public class DataPoints : IDataPoints
	{
		public IDataScoreInfo SinglePlayer { get; private set; }
		public IDataScoreInfo Cooperative { get; private set; }
		public IDataScoreInfo Global { get; private set; }
		public IReadOnlyDictionary<ChapterType, IDataScoreInfo> Chapters { get; private set; }

		internal static DataPoints Create(
			IDataScoreInfo singlePlayer,
			IDataScoreInfo cooperative,
			IDataScoreInfo global,
			IReadOnlyDictionary<ChapterType, IDataScoreInfo> chapters)
		{
			return new DataPoints()
			{
				SinglePlayer = singlePlayer,
				Cooperative = cooperative,
				Global = global,
				Chapters = chapters
			};
		}
	}
}