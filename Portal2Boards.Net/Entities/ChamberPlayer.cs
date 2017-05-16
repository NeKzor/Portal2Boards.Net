namespace Portal2Boards.Net.Entities
{
	public class ChamberPlayer
	{
		public string Name { get; set; }
		public string SteamAvatarLink { get; set; }
		public ulong SteamId { get; set; }

		public ChamberPlayer(
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