using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Portal2Boards.Net.Entities;

namespace Portal2Boards.Net.API.Models
{
	[DebuggerDisplay("Count = {Data.Count,nq}")]
	public sealed class Changelog : IModel, IEnumerable<EntryData>
    {
		public IReadOnlyCollection<ChangelogData> Data { get; set; }
		public string ApiRequestUrl { get; internal set; }
		public string RequestUrl
			=> ApiRequestUrl.Remove(ApiRequestUrl.IndexOf("/json"), "/json".Length);

		public Changelog()
		{
		}
		public Changelog(IReadOnlyCollection<ChangelogData> data, string url)
		{
			Data = data;
			ApiRequestUrl = url;
		}

		public IReadOnlyCollection<EntryData> ConvertToEntries(IEnumerable<ChangelogData> data = default(IEnumerable<ChangelogData>))
		{
			var temp = new List<EntryData>();
			foreach (EntryData entry in data ?? Data)
				temp.Add(entry);
			return temp;
		}

		IEnumerator IEnumerable.GetEnumerator()
			=> Data.GetEnumerator();
		IEnumerator<EntryData> IEnumerable<EntryData>.GetEnumerator()
			=> ConvertToEntries(Data).GetEnumerator();
	}
}