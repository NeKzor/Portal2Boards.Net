//#define GEN_SP
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Portal2Boards.Net.API;
using Portal2Boards.Net.Entities;
using Portal2Boards.Net.Extensions;
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

		private static void Main()
		{
			GetAggregated();
			GetLeaderboard();
			GetChangelog();
			GetProfile();
			GetDemo();
			GenSpPage();
			GenMpPage();
			StartTwBot();
		}

		[Conditional("AGG")]
		internal static void GetAggregated()
		{
			using (var client = new Portal2BoardsClient())
			{
				var aggregated = client.GetAggregatedAsync().GetAwaiter().GetResult();

				WriteLine("Global points:");
				foreach (var points in aggregated.DataPoints.Take(10))
				{
					WriteLine($"[{points.Key}]\t{points.Value.UserData.BoardName} : {points.Value.ScoreData.Score}");
				}

				WriteLine("Global times:");
				foreach (var points in aggregated.DataTimes.Take(10))
				{
					WriteLine($"[{points.Key}]\t{points.Value.UserData.BoardName} : {points.Value.ScoreData.Score}");
				}
			}
		}
		[Conditional("LB")]
		internal static void GetLeaderboard()
		{
			using (var client = new Portal2BoardsClient())
			{
				var board = client.GetLeaderboardAsync(47458).GetAwaiter().GetResult();

				WriteLine($"Fetched {board.Data.Count} entries.");
				foreach (var entry in board.Take(10))
				{
					WriteLine($"[{entry.Id}]\t" +
							  $"[{entry.Date?.ToString("s") ?? "Unknown"}] " +
							  $"{((float?)entry.Score / 100)?.ToString("N2") ?? "Unknown"} by " +
							  $"{entry.Player.Name}");
				}
			}
		}
		[Conditional("CLOG"), Conditional("CACHE")]
		internal static void GetChangelog()
		{
			using (var client = new Portal2BoardsClient(_latestWorldRecords, cacheResetTime: 1))
			{
				// Event test
				client.OnException += LogException;

				var changelog = client.GetChangelogAsync().GetAwaiter().GetResult();
#if CACHE
				// Cache test 1
				changelog = client.GetChangelogAsync().GetAwaiter().GetResult();
#endif

				WriteLine($"Fetched {changelog.Data.Count} entries.");
				foreach (var entry in changelog)
				{
					WriteLine($"[{entry.Id}]\t" +
							  $"[{entry.Date?.ToString("s") ?? "Unknown"}] " +
							  $"{entry.Map.Name} in " +
							  $"{((float?)entry.Score.Current / 100)?.ToString("N2") ?? "Unknown"} by " +
							  $"{entry.Player.Name}");
				}
#if CACHE
				// Cache test 2
				client.ClearCache().GetAwaiter().GetResult();
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
#endif
			}
		}
		[Conditional("PRO_EX"), Conditional("PRO")]
		internal static void GetProfile()
		{
			using (var client = new Portal2BoardsClient())
			{
				var profile = client.GetProfileAsync("Nik :D <2").GetAwaiter().GetResult();
#if PRO_EX
				// Inject exception
				profile.Data.Times.BestRank.ScoreData.Date = "AYY LMAO";
#endif
				var user = (UserData)profile.Data;

				WriteLine($"User profile of: {user.SteamName}");
				WriteLine($"User profile of: {user.DisplayName}");
				WriteLine($"User profile of: {user.BoardName}");
				foreach (var chapter in user.Times.SinglePlayer.Chapters.Chambers)
				{
					WriteLine($"[{Enum.GetName(typeof(Chapter), chapter.Key).FormatChapterTitle()}]");
					foreach (var chamber in chapter.Value.Data)
					{
						WriteLine($"[{chamber.Key}]\t{chamber.Value.MapId} in {chamber.Value.Score} : Rank {chamber.Value.PlayerRank}");
					}
				}
			}
		}
		[Conditional("DEM")]
		internal static void GetDemo()
		{
			using (var client = new Portal2BoardsClient())
			{
				var content = client.GetDemoContentAsync(64230).GetAwaiter().GetResult();
				// Do stuff with content
				using (var stream = new FileStream("not_rank_2.dem", FileMode.Create))
				using (var memory = new MemoryStream(content))
					memory.CopyToAsync(stream).GetAwaiter().GetResult();
			}
		}

		// Logger test
		internal static Task LogException(object _, LogMessage message)
		{
			WriteLine(message.ToString());
			return Task.CompletedTask;
		}

		// Example 1 (HtmlGenerator.cs)
		[Conditional("GEN_SP")]
		internal static void GenSpPage()
			=> HtmlGenerator.GeneratePage(@"index.html", MapType.SinglePlayer).GetAwaiter().GetResult();
		[Conditional("GEN_MP")]
		internal static void GenMpPage()
			=> HtmlGenerator.GeneratePage(@"coop.html", MapType.Cooperative).GetAwaiter().GetResult();

		// Example 2 (TwitterBot.cs)
		[Conditional("TWBOT")]
		internal static void StartTwBot()
		{
			TwitterBot.InitAsync().GetAwaiter().GetResult();
			TwitterBot.RunAsync().GetAwaiter().GetResult();
		}

		// Helper
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