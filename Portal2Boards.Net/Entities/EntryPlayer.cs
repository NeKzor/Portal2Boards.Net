namespace Portal2Boards.Net.Entities
{
	public class EntryPlayer
	{
		public string Name { get; set; }
		public string SteamAvatarLink { get; set; }
		public ulong SteamId { get; set; }

		public EntryPlayer(
			string name = default(string),
			string steamAvatarLink = default(string),
			ulong steamId = default(uint))
		{
			Name = name;
			SteamAvatarLink = steamAvatarLink;
			SteamId = steamId;
		}
	}
}