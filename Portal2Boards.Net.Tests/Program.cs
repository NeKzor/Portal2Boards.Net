using System;
using Portal2Boards.Net.API;

namespace Portal2Boards.Net.Test
{
	internal static class Program
    {
		private static readonly ChangelogParameters _latestWorldRecords = new ChangelogParameters()
		{
			//[Parameters.MaxDaysAgo] = 7,
			//[Parameters.WorldRecord] = 1,
			[Parameters.MapId] = 47458
		};

		private static void Main(string[] args)
		{
			using (var client = new Portal2BoardsClient(_latestWorldRecords))
			{
				//var changelog = client.GetChangelogAsync().GetAwaiter().GetResult();
				//Console.WriteLine($"Fetched {changelog.Data.Count} entries.");
				//foreach (var entry in changelog)
				//{
				//	Console.WriteLine($"[{entry.Id}]\t" +
				//					  $"[{entry.Date?.ToString("s") ?? "Unknown"}] " +
				//					  $"{entry.Map.Name} in " +
				//					  $"{((float?)entry.Score.Current / 100)?.ToString("N2") ?? "Unknown"} by " +
				//					  $"{entry.Player.Name}");
				//}
				var board = client.GetLeaderboardAsync(47458).GetAwaiter().GetResult();
				Console.WriteLine($"Fetched {board.Data.Count} entries.");
				foreach (var entry in board)
				{
					Console.WriteLine($"[{entry.Id}]\t" +
									  $"[{entry.Date?.ToString("s") ?? "Unknown"}] " +
									  $"{((float?)entry.Score / 100)?.ToString("N2") ?? "Unknown"} by " +
									  $"{entry.Player.Name}");
				}
				//var aggregated = client.GetAggregatedAsync().GetAwaiter().GetResult();
				//var profile = client.GetProfileAsync("Jetwash_787").GetAwaiter().GetResult();
				Console.ReadKey();
			}
		}
    }
}