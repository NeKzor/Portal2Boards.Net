namespace Portal2Boards.Net.Entities
{
	public class User
	{
		public string Name { get; set; }
		public string SteamAvatarLink { get; set; }
		public ulong SteamId { get; set; }
		public string Link
			=> $"http://board.iverb.me/profile/{SteamId}";
		public string SteamLink
			=> $"http://steamcommunity.com/profiles/{SteamId}";

		public User(
			string name = default(string),
			string steamAvatarLink = default(string),
			ulong steamId = default(ulong))
		{
			Name = name;
			SteamAvatarLink = steamAvatarLink;
			SteamId = steamId;
		}
	}
}