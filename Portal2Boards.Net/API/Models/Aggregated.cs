using System;
using System.Collections.Generic;

namespace Portal2Boards.Net.API.Models
{
	public sealed class Aggregated : IModel
    {
		public IReadOnlyDictionary<ulong, AggregatedEntryData> DataPoints { get; set; }
		public IReadOnlyDictionary<ulong, AggregatedEntryData> DataTimes { get; set; }
		public string ApiRequestUrl { get; internal set; }
		public string RequestUrl
			=> ApiRequestUrl.Substring(0, ApiRequestUrl.IndexOf("/json"));

		public Aggregated()
		{
		}
		public Aggregated(AggregatedData data, string url)
		{
			DataPoints = data.Points;
			DataTimes = data.Times;
			ApiRequestUrl = url;
		}
	}
}