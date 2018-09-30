using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Model = Portal2Boards.API.AggregatedModel;

namespace Portal2Boards
{
    [DebuggerDisplay("Points = {Points.Count,nq}, Times = {Points.Count,nq}")]
    public class Aggregated : IAggregated, IUpdatable
    {
        public AggregatedMode Mode { get; private set; }
        public ChapterType Chapter { get; private set; }
        public IReadOnlyCollection<IAggregatedEntry> Points { get; private set; }
        public IReadOnlyCollection<IAggregatedEntry> Times { get; private set; }

        internal Portal2BoardsClient Client { get; private set; }

        public async Task UpdateAsync(bool ignoreCache = false)
        {
            var aggregated = default(IAggregated);
            if (Mode != AggregatedMode.Chapter)
                aggregated = await Client.GetAggregatedAsync(Mode, ignoreCache).ConfigureAwait(false);
            else if (Chapter != ChapterType.None)
                aggregated = await Client.GetAggregatedAsync(Chapter, ignoreCache).ConfigureAwait(false);
            else
                throw new InvalidOperationException("Mode and chapter are not set.");

            Points = aggregated.Points;
            Times = aggregated.Times;
        }

        internal static Aggregated Create(
            Portal2BoardsClient client,
            AggregatedMode mode,
            ChapterType chapter,
            Model model)
        {
            var points = new List<IAggregatedEntry>();
            var times = new List<IAggregatedEntry>();
            foreach (var item in model.Points)
                points.Add(AggregatedEntry.Create(client, item.Key, item.Value));
            foreach (var item in model.Times)
                times.Add(AggregatedEntry.Create(client, item.Key, item.Value));

            return new Aggregated()
            {
                Mode = mode,
                Chapter = chapter,
                Points = points,
                Times = times,
                Client = client
            };
        }
    }
}
