namespace Portal2Boards
{
	public class EntryMap
	{
		public uint SteamId { get; set; }
		public uint ChapterId { get; set; }
		public string Name { get; set; }
		public string Link
			=> $"https://board.iverb.me/chamber/{SteamId}";
		public string SteamLink
			=> $"https://steamcommunity.com/stats/Portal2/leaderboards/{SteamId}";
		public string ImageLink
			=> $"https://board.iverb.me/images/chambers/{SteamId}.jpg";
		public string ImageLinkFull
			=> $"https://board.iverb.me/images/chambers_full/{SteamId}.jpg";

		public EntryMap(
			uint steamId = default(uint),
			uint chapterId = default(uint),
			string name = default(string))
		{
			SteamId = steamId;
			ChapterId = chapterId;
			Name = name;
		}
	}
}