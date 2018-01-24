using System.Threading.Tasks;
using Model = Portal2Boards.API.Models.AggregatedEntryDataModel;

namespace Portal2Boards
{
    public class AggregatedEntry : IEntity, IAggregatedEntry
	{
		public ulong Id { get; private set; }
		public IUser Player { get; private set; }
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
				Player = User.Create(model.UserData.BoardName, model.UserData.Avatar, id),
				Score = uint.Parse(model.ScoreData.Score),
				PlayerRank = uint.Parse(model.ScoreData.PlayerRank),
				ScoreRank = uint.Parse(model.ScoreData.ScoreRank),
				Client = client
			};
		}
	}
}