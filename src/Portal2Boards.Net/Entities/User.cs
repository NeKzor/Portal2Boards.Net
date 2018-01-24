using System.Net;

namespace Portal2Boards
{
	public interface IUser
	{
		string Name { get; set; }
		string SteamAvatarLink { get; set; }
		ulong SteamId { get; set; }
	}

	public class User : IUser
	{
		public string Name { get; set; }
		public string SteamAvatarLink { get; set; }
		public ulong SteamId { get; set; }

		public string Link
			=> $"https://board.iverb.me/profile/{SteamId}";
		public string SteamLink
			=> $"https://steamcommunity.com/profiles/{SteamId}";

		internal static User Create(string name = default, string steamAvatarLink = default, ulong steamId = default)
		{
			return new User()
			{
				Name = WebUtility.HtmlDecode(name),
				SteamAvatarLink = steamAvatarLink,
				SteamId = steamId
			};
		}
	}
}