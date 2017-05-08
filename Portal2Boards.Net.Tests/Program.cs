using System;
using Portal2Boards.Net.API;

namespace Portal2Boards.Net.Tests
{
	internal static class Program
    {
		private static readonly BoardParameters _latestWorldRecords = new BoardParameters()
		{
			[Parameters.MaxDaysAgo] = 7,
			[Parameters.WorldRecord] = 1
		};

		private static void Main(string[] args)
		{
			using (var client = new Portal2BoardsClient(_latestWorldRecords))
			{
				var changelog = client.GetChangelogAsync().GetAwaiter().GetResult();
				Console.WriteLine($"Fetched {changelog.Data.Count} entries.");
				foreach (var entry in changelog)
				{
					Console.WriteLine($"[{entry.EntryId}] " +
									  $"{entry.Map.Name} in " +
									  $"{((float?)entry.Score.Current / 100)?.ToString("N2") ?? "Unkown"} by " +
									  $"{entry.Player.Name}");
				}
				Console.ReadKey();
			}
		}
    }
}