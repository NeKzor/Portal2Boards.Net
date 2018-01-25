using System.Collections.Generic;

namespace Portal2Boards
{
    public class DataPoints : IDataPoints
	{
		public IPoints SinglePlayer { get; private set; }
		public IPoints Cooperative { get; private set; }
		public IPoints Global { get; private set; }
		public IReadOnlyDictionary<Chapter, IPoints> Chapters { get; private set; }

		internal static DataPoints Create(
			Points singlePlayer,
			Points cooperative,
			Points global,
			IReadOnlyDictionary<Chapter, IPoints> chapters)
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