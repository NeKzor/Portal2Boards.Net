using System;
using System.Collections.Generic;

namespace Portal2Boards.Net.API.Models
{
	public sealed class Aggregated : IModel
    {
		public IReadOnlyDictionary<ulong, AggregatedEntryData> DataPoints { get; set; }
		public IReadOnlyDictionary<ulong, AggregatedEntryData> DataTimes { get; set; }
		public string RequestUrl { get; internal set; }

		public Aggregated()
		{
		}
		public Aggregated(AggregatedData data, string url)
		{
			DataPoints = data.Points;
			DataTimes = data.Times;
			RequestUrl = url;
		}
	}
}