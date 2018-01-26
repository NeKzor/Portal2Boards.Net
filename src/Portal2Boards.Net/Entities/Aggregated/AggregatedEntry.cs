using System.Diagnostics;
using System.Threading.Tasks;
using Model = Portal2Boards.API.AggregatedEntryDataModel;

namespace Portal2Boards
{
	[DebuggerDisplay("{Id,nq}")]
    public class AggregatedEntry : IEntity<ulong>, IAggregatedEntry
	{
		public ulong Id { get; private set; }
		public ISteamUser Player { get; private set; }
		public uint Score { get; private set; }
		public uint PlayerRank { get; private set; }
		public uint ScoreRank { get; private set; }

		internal Portal2BoardsClient Client { get; private set; }

		public async Task<IProfile> GetProfileAsync()
			=> await Client.GetProfileAsync(Id);

		internal static AggregatedEntry Create(Portal2BoardsClient client, ulong id, Model model)
		{
			return new AggregatedEntry()
			{
				Id = id,
				Player = SteamUser.Create(client, id, model.UserData.BoardName, model.UserData.Avatar),
				Score = uint.Parse(model.ScoreData.Score),
				PlayerRank = uint.Parse(model.ScoreData.PlayerRank),
				ScoreRank = uint.Parse(model.ScoreData.ScoreRank),
				Client = client
			};
		}
	}
}