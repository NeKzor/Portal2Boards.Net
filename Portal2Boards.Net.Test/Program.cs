using System;
using System.Linq;
using Portal2Boards.Net.API;
using Portal2Boards.Net.Entities;
using static System.Console;

namespace Portal2Boards.Net.Test
{
	internal static class Program
    {
		// Recommended
		private static readonly ChangelogParameters _latestWorldRecords = new ChangelogParameters()
		{
			[Parameters.MaxDaysAgo] = 7,
			[Parameters.WorldRecord] = 1
		};

		private static void Main(string[] args)
		{
			//GetAggregated();
			//GetLeaderboard();
			GetChangelog();
			//GetProfile();
			//HtmlGenerator.GeneratePages().GetAwaiter().GetResult();
		}

		internal static void GetAggregated()
		{
			using (var client = new Portal2BoardsClient())
			{
				var aggregated = client.GetAggregatedAsync().GetAwaiter().GetResult();

				WriteLine("Global points:");
				foreach (var points in aggregated.DataPoints)
				{
					WriteLine($"[{points.Key}]\t{points.Value.UserData.BoardName} : {points.Value.ScoreData.Score}");
				}

				WriteLine("Global times:");
				foreach (var points in aggregated.DataTimes)
				{
					WriteLine($"[{points.Key}]\t{points.Value.UserData.BoardName} : {points.Value.ScoreData.Score}");
				}
				ReadKey();
				Clear();
			}
		}

		internal static void GetLeaderboard()
		{
			using (var client = new Portal2BoardsClient())
			{
				var board = client.GetLeaderboardAsync(47458).GetAwaiter().GetResult();

				WriteLine($"Fetched {board.Data.Count} entries.");
				foreach (var entry in board)
				{
					WriteLine($"[{entry.Id}]\t" +
							  $"[{entry.Date?.ToString("s") ?? "Unknown"}] " +
							  $"{((float?)entry.Score / 100)?.ToString("N2") ?? "Unknown"} by " +
							  $"{entry.Player.Name}");
				}
				ReadKey();
				Clear();
			}
		}

		internal static void GetChangelog()
		{
			using (var client = new Portal2BoardsClient(_latestWorldRecords, cacheResetTime: 1))
			{
				var changelog = client.GetChangelogAsync().GetAwaiter().GetResult();

				WriteLine($"Fetched {changelog.Data.Count} entries.");
				foreach (var entry in changelog)
				{
					WriteLine($"[{entry.Id}]\t" +
							  $"[{entry.Date?.ToString("s") ?? "Unknown"}] " +
							  $"{entry.Map.Name} in " +
							  $"{((float?)entry.Score.Current / 100)?.ToString("N2") ?? "Unknown"} by " +
							  $"{entry.Player.Name}");
				}
				ReadKey();
				Clear();

				// Cache test
				//client.ClearCache().GetAwaiter().GetResult();
				changelog = client.GetChangelogAsync().GetAwaiter().GetResult();

				WriteLine($"Fetched {changelog.Data.Count} entries.");
				foreach (var entry in changelog)
				{
					WriteLine($"[{entry.Id}]\t" +
							  $"[{entry.Date?.ToString("s") ?? "Unknown"}] " +
							  $"{entry.Map.Name} in " +
							  $"{((float?)entry.Score.Current / 100)?.ToString("N2") ?? "Unknown"} by " +
							  $"{entry.Player.Name}");
				}
				ReadKey();
				Clear();
			}
		}

		internal static void GetProfile()
		{
			using (var client = new Portal2BoardsClient())
			{
				var profile = client.GetProfileAsync("cojosao").GetAwaiter().GetResult();
				var user = (UserData)profile.Data;

				WriteLine($"User profile of: {user.BoardName}");
				foreach (var chapter in user.Times.SinglePlayer.Chapters.Chambers)
				{
					WriteLine($"[{Enum.GetName(typeof(Chapter), chapter.Key).FormatChapterTitle()}]");
					foreach (var chamber in chapter.Value.Data)
					{
						WriteLine($"[{chamber.Key}]\t{chamber.Value.MapId} in {chamber.Value.Score} : Rank {chamber.Value.PlayerRank}");
					}
				}
				ReadKey();
				Clear();
			}
		}

		private static string FormatChapterTitle(this string str)
		{
			var output = string.Empty;
			if (str?.Length > 0)
			{
				output = $"{str[0]}";
				foreach (var c in str.Skip(1))
					output += char.IsLower(c) ? $"{c}" : $" {c}";
			}
			return output;
		}
	}
}