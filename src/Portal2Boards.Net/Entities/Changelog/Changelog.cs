using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Model = System.Collections.Generic.IReadOnlyCollection<Portal2Boards.API.Models.ChangelogEntryModel>;

namespace Portal2Boards
{
    [DebuggerDisplay("Count = {Entries.Count,nq}")]
	public sealed class Changelog : IChangelog
    {
		public IReadOnlyCollection<IChangelogEntry> Entries { get; set; }

		internal static Changelog Create(Model model)
		{
			var entries = new List<IChangelogEntry>();
			foreach (var item in model)
				entries.Add(ChangelogEntry.Create(item));
			return new Changelog { Entries = entries };
		}
	}
}