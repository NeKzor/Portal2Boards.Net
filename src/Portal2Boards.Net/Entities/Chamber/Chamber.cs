using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Model = System.Collections.Generic.IReadOnlyDictionary<ulong, Portal2Boards.API.Models.ChamberEntryModel>;

namespace Portal2Boards
{
    [DebuggerDisplay("Count = {Entries.Count,nq}")]
	public sealed class Chamber : IEntity, IChamber, IUpdatable
    {
		public ulong Id { get; private set; }
		public IReadOnlyCollection<IChamberEntry> Entries { get; private set; }

		internal Portal2BoardsClient _client;

		public async Task UpdateAsync()
		{
			var chamber = await _client.GetChamberAsync(Id);
			Entries = chamber.Entries;
		}

		internal static Chamber Create(
			Portal2BoardsClient client,
			ulong id,
			Model model)
		{
			var entries = new List<IChamberEntry>();
			foreach (var item in model)
				entries.Add(ChamberEntry.Create(client, item.Key, item.Value));
			
			return new Chamber
			{
				Id = id,
				Entries = entries,
				_client = client
			};
		}
	}
}