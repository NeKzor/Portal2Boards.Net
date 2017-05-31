using System.Net;

namespace Portal2Boards.Net.Entities
{
	public class User
	{
		public string Name { get; set; }
		public string SteamAvatarLink { get; set; }
		public ulong SteamId { get; set; }
		public string Link
			=> $"https://board.iverb.me/profile/{SteamId}";
		public string SteamLink
			=> $"https://steamcommunity.com/profiles/{SteamId}";

		public User(
			string name = default(string),
			string steamAvatarLink = default(string),
			ulong steamId = default(ulong))
		{
			Name = WebUtility.HtmlDecode(name);
			SteamAvatarLink = steamAvatarLink;
			SteamId = steamId;
		}
	}
}