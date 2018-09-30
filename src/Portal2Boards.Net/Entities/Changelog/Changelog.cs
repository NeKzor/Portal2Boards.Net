using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Model = System.Collections.Generic.IReadOnlyCollection<Portal2Boards.API.ChangelogEntryModel>;

namespace Portal2Boards
{
    [DebuggerDisplay("Count = {Entries.Count,nq}")]
    public class Changelog : IChangelog, IUpdatable
    {
        public string Query { get; private set; }
        public IReadOnlyCollection<IChangelogEntry> Entries { get; private set; }

        internal Portal2BoardsClient Client { get; private set; }

        public async Task UpdateAsync(bool ignoreCache = false)
        {
            var changelog = await Client.GetChangelogAsync(Query, ignoreCache).ConfigureAwait(false);
            Entries = changelog.Entries;
        }

        internal static Changelog Create(Portal2BoardsClient client, string query, Model model)
        {
            var entries = new List<IChangelogEntry>();
            foreach (var item in model)
                entries.Add(ChangelogEntry.Create(client, item));

            return new Changelog
            {
                Query = query,
                Entries = entries,
                Client = client
            };
        }
    }
}
