using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Model = System.Collections.Generic.IReadOnlyDictionary<ulong, Portal2Boards.API.ChamberEntryModel>;

namespace Portal2Boards
{
    [DebuggerDisplay("[{Id,nq}] Entries = {Entries.Count,nq}")]
	public class Chamber : IEntity<ulong>, IChamber, IUpdatable
    {
		public ulong Id { get; private set; }
		public IReadOnlyCollection<IChamberEntry> Entries { get; private set; }

		internal Portal2BoardsClient Client { get; private set; }

		public async Task UpdateAsync()
		{
			var chamber = await Client.GetChamberAsync(Id);
			Entries = chamber.Entries;
		}

		public async Task<IChangelog> GetChangelogAsync()
			=> await Client.GetChangelogAsync($"?chamber={Id}");

		internal static Chamber Create(Portal2BoardsClient client, ulong id, Model model)
		{
			var entries = new List<IChamberEntry>();
			foreach (var item in model)
				entries.Add(ChamberEntry.Create(client, item.Key, item.Value));
			
			return new Chamber
			{
				Id = id,
				Entries = entries,
				Client = client
			};
		}
	}
}