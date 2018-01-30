using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;

namespace Portal2Boards
{
	[DebuggerDisplay("{Id,nq}")]
	public class SteamUser : IEntity<ulong>, ISteamUser
	{
		public ulong Id { get; private set; }
		public string Name { get; private set; }
		public string AvatarUrl { get; private set; }

		public string Url
			=> $"https://steamcommunity.com/profiles/{Id}";

		internal Portal2BoardsClient Client { get; private set; }

		public async Task<IProfile> GetProfileAsync(bool ignoreCache = false)
			=> await Client.GetProfileAsync(Id, ignoreCache).ConfigureAwait(false);

		internal static SteamUser Create(
			Portal2BoardsClient client,
			ulong id = default,
			string name = default,
			string avatarUrl = default)
		{
			return new SteamUser()
			{
				Id = id,
				Name = WebUtility.HtmlDecode(name),
				AvatarUrl = avatarUrl,
				Client = client
			};
		}
	}
}