namespace Portal2Boards.Extensions
{
	public sealed class Map
	{
		public static uint Count = 0;
		
		public uint Index;
		public string Name { get; set; }
		public string Alias { get; set; }
		public MapType Type { get; set; }
		public ulong? BestTimeId { get; set; }
		public ulong? BestPortalsId { get; set; }
		public uint? ChapterId { get; set; }

		public bool IsOfficial
			=> ((Type == MapType.SinglePlayer) || Type == (MapType.Cooperative))
				&& (BestTimeId != default(ulong?))
				&& (BestPortalsId != default(ulong?));
		public bool Exists
			=> ((Type == MapType.SinglePlayer) || Type == (MapType.Cooperative))
				&& (BestTimeId != default(ulong?));
		
		public string Link
			=> $"https://board.iverb.me/chamber/{BestTimeId}";
		public string ImageLink
			=> $"https://board.iverb.me/images/chambers/{BestTimeId}.jpg";
		public string ImageLinkFull
			=> $"https://board.iverb.me/images/chambers_full/{BestTimeId}.jpg";
		public string BestTimeSteamLink
			=> $"https://steamcommunity.com/stats/Portal2/leaderboards/{BestTimeId}";
		public string BestPortalsSteamLink
			=> $"https://steamcommunity.com/stats/Portal2/leaderboards/{BestPortalsId}";

		public Map(
			string name = default,
			string alias = default,
			MapType type = default,
			uint? bestTimeId = default,
			uint? bestPortalsId = default,
			uint? chapterId = default)
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