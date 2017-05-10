using System;
using Portal2Boards.Net.API;

namespace Portal2Boards.Net.Test
{
	internal static class Program
    {
		private static readonly BoardParameters _latestWorldRecords = new BoardParameters()
		{
			//[Parameters.MaxDaysAgo] = 7,
			//[Parameters.WorldRecord] = 1,
			[Parameters.MapId] = 47458
		};

		private static void Main(string[] args)
		{
			using (var client = new Portal2BoardsClient(_latestWorldRecords))
			{
				var changelog = client.GetChangelogAsync().GetAwaiter().GetResult();
				Console.WriteLine($"Fetched {changelog.Data.Count} entries.");
				foreach (var entry in changelog)
				{
					Console.WriteLine($"[{entry.EntryId}]\t" +
									  $"[{entry.Date?.ToString("s") ?? "Unknown"}] " +
									  $"{entry.Map.Name} in " +
									  $"{((float?)entry.Score.Current / 100)?.ToString("N2") ?? "Unknown"} by " +
									  $"{entry.Player.Name}");
				}
				Console.ReadKey();
			}
		}
    }
}