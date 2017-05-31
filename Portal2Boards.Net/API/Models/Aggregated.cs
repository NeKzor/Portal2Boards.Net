using System.Collections.Generic;

namespace Portal2Boards.Net.API.Models
{
	public sealed class Aggregated : IModel
    {
		public IReadOnlyDictionary<ulong, AggregatedEntryData> DataPoints { get; set; }
		public IReadOnlyDictionary<ulong, AggregatedEntryData> DataTimes { get; set; }
		public string ApiRequestUrl { get; internal set; }
		public string RequestUrl
			=> ApiRequestUrl.Remove(ApiRequestUrl.IndexOf("/json"), "/json".Length);

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