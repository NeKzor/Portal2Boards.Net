using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Portal2Boards.Net.Entities;

namespace Portal2Boards.Net.API.Models
{
	[DebuggerDisplay("Count = {Data.Count,nq}")]
	public sealed class Aggregated : IModel//, IEnumerable<ChamberData>
    {
		public IReadOnlyCollection<AggregatedData> Data { get; set; }

		public Aggregated()
		{
		}
		public Aggregated(Dictionary<string, AggregatedEntryData> data)
		{
			var temp = new List<AggregatedData>();
			foreach (var item in data)
				temp.Add(new AggregatedData(item.Key, item.Value));
			Data = temp;
		}

		//public IReadOnlyCollection<ChamberData> ConvertToEntries(IReadOnlyCollection<AggregatedData> data = default(IReadOnlyCollection<AggregatedData>))
		//{
		//	var temp = new List<ChamberData>();
		//	//foreach (ChamberData entry in data ?? Data)
		//	//	temp.Add(entry);
		//	return temp;
		//}

		//IEnumerator IEnumerable.GetEnumerator()
		//	=> Data.GetEnumerator();
		//IEnumerator<ChamberData> IEnumerable<ChamberData>.GetEnumerator()
		//	=> ConvertToEntries(Data).GetEnumerator();
	}
}