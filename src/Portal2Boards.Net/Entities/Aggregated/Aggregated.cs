using System.Collections.Generic;
using Model = Portal2Boards.API.Models.AggregatedModel;

namespace Portal2Boards
{
    public class Aggregated : IAggregated
    {
		public IReadOnlyCollection<IAggregatedEntry> Points { get; private set; }
		public IReadOnlyCollection<IAggregatedEntry> Times { get; private set; }
		
		internal static Aggregated Create(Portal2BoardsClient client, Model model)
		{
			var points = new List<IAggregatedEntry>();
			var times = new List<IAggregatedEntry>();
			foreach (var item in model.Points)
				points.Add(AggregatedEntry.Create(client, item.Key, item.Value));
			foreach (var item in model.Times)
				times.Add(AggregatedEntry.Create(client, item.Key, item.Value));
			return new Aggregated()
			{
				Points = points,
				Times = times
			};
		}
	}
}