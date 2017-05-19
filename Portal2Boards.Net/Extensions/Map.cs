namespace Portal2Boards.Net.Extensions
{
	public sealed class Map
	{
		public static uint Count = 0;
		public uint Index;
		public string Name { get; set; }
		public string Alias { get; set; }
		public MapType Type { get; set; }
		public uint? BestTimeId { get; set; }
		public uint? BestPortalsId { get; set; }
		public uint? ChapterId { get; set; }
		public bool IsOfficial
			=> ((Type == MapType.SinglePlayer) || Type == (MapType.Cooperative))
				&& (BestTimeId != default(uint?))
				&& (BestPortalsId != default(uint?));

		public Map(
			string name = default(string),
			string alias = default(string),
			MapType type = default(MapType),
			uint? bestTimeId = default(uint?),
			uint? bestPortalsId = default(uint?),
			uint? chapterId = default(uint?))
		{
			Name = name;
			Alias = alias;
			Type = type;
			BestTimeId = bestTimeId;
			BestPortalsId = bestPortalsId;
			ChapterId = chapterId;
			Index = Count;
			Count++;
		}
	}
}