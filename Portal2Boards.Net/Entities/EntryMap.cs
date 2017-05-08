namespace Portal2Boards.Net.Entities
{
	public class EntryMap
	{
		public uint SteamId { get; set; }
		public uint ChapterId { get; set; }
		public string Name { get; set; }

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