using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Portal2Boards.Net.Entities;

namespace Portal2Boards.Net.API.Models
{
	[DebuggerDisplay("Count = {Data.Count,nq}")]
	public sealed class Board : IModel, IEnumerable<ChamberData>
    {
		public IReadOnlyCollection<BoardData> Data { get; set; }
		public string ApiRequestUrl { get; internal set; }
		public string RequestUrl
			=> ApiRequestUrl.Remove(ApiRequestUrl.LastIndexOf("/json"), "/json".Length);

		public Board()
		{
		}
		public Board(IReadOnlyDictionary<ulong, BoardEntryData> data, string url)
		{
			var temp = new List<BoardData>();
			foreach (var item in data)
				temp.Add(new BoardData(item.Key, item.Value));
			Data = temp;
			ApiRequestUrl = url;
		}

		public IReadOnlyCollection<ChamberData> ConvertToEntries(IReadOnlyCollection<BoardData> data = default(IReadOnlyCollection<BoardData>))
		{
			var temp = new List<ChamberData>();
			foreach (ChamberData entry in data ?? Data)
				temp.Add(entry);
			return temp;
		}

		IEnumerator IEnumerable.GetEnumerator()
			=> Data.GetEnumerator();
		IEnumerator<ChamberData> IEnumerable<ChamberData>.GetEnumerator()
			=> ConvertToEntries(Data).GetEnumerator();
	}
}