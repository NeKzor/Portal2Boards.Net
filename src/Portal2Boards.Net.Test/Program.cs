using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Portal2Boards.API;
using Portal2Boards.Extensions;
using Portal2Boards.Test.Examples;
using static System.Console;

namespace Portal2Boards.Test
{
	internal class Program
	{
		private static readonly ChangelogQuery _latestWorldRecords = new ChangelogQueryBuilder()
			.WithWorldRecord(true)
			.Build();
		
		private static void Main()
		{
			GetAggregated();
			GetChamber();
			GetChangelog();
			GetProfile();
			GetDemo();
			GenerateSpPage();
			GenerateMpPage();
			GenerateStatsPage();
			StartTwitterBot();
		}

		[Conditional("AGG")]
		internal static void GetAggregated()
		{
			using (var client = new Portal2BoardsClient())
			{
				client.Log += LogPortal2Boards;

				WriteLine("Fetching aggregated...");
				var aggregated = client.GetAggregatedAsync().GetAwaiter().GetResult();

				WriteLine("Global points:");
				foreach (var points in aggregated.Points.Take(10))
				{
					WriteLine($"[{(points.Player as IEntity<ulong>).Id}]\t{points.Player.Name} : {points.Score}");
				}

				WriteLine("Global times:");
				foreach (var points in aggregated.Times.Take(10))
				{
					WriteLine($"[{(points.Player as IEntity<ulong>).Id}]\t{points.Player.Name} : {points.Score}");
				}
			}
		}
		[Conditional("CHA")]
		internal static void GetChamber()
		{
			using (var client = new Portal2BoardsClient())
			{
				client.Log += LogPortal2Boards;

				WriteLine("Fetching chamber...");
				var chamber = client.GetChamberAsync(47458).GetAwaiter().GetResult();

				WriteLine($"Fetched {chamber.Entries.Count} entries.");
				foreach (var entry in chamber.Entries.Take(10))
				{
					WriteLine
					(
						$"[{entry.ChangelogId}]\t" +
						$"[{entry.Date?.ToString("s") ?? "Unknown"}] " +
						$"{((float?)entry.Score / 100)?.ToString("N2") ?? "Unknown"} by " +
						$"{entry.Player.Name}"
					);
				}
			}
		}
		[Conditional("CLOG"), Conditional("CACHE")]
		internal static void GetChangelog()
		{
			using (var client = new Portal2BoardsClient(cacheResetTime: 1))
			{
				client.Log += LogPortal2Boards;

				WriteLine("Fetching changelog...");
				var watch = Stopwatch.StartNew();
				var changelog = client.GetChangelogAsync(() => _latestWorldRecords).GetAwaiter().GetResult();
				watch.Stop();
				WriteLine(watch.ElapsedMilliseconds);
#if CACHE
				// Cache test 1
				WriteLine("Cache test 1...");
				watch = Stopwatch.StartNew();
				changelog = client.GetChangelogAsync(() => _latestWorldRecords).GetAwaiter().GetResult();
				watch.Stop();
				WriteLine(watch.ElapsedMilliseconds);
#endif

				WriteLine($"Fetched {changelog.Entries.Count} entries.");
				foreach (var entry in changelog.Entries.Take(3))
				{
					WriteLine
					(
						$"[{(entry as ChangelogEntry).Id}]\t" +
						$"[{entry.Date?.ToString("s") ?? "Unknown"}] " +
						$"{entry.Name} in " +
						$"{((float?)entry.Score.Current / 100)?.ToString("N2") ?? "Unknown"} by " +
						$"{entry.Player.Name}"
					);
				}
#if CACHE
				// Cache test 2
				WriteLine("Cache test 2...");
				client.ClearCache();
				watch = Stopwatch.StartNew();
				changelog = client.GetChangelogAsync(q => q.WorldRecord = true).GetAwaiter().GetResult();
				watch.Stop();
				WriteLine(watch.ElapsedMilliseconds);

				WriteLine($"Fetched {changelog.Entries.Count} entries.");
				foreach (var entry in changelog.Entries.Take(3))
				{
					WriteLine
					(
						$"[{(entry as ChangelogEntry).Id}]\t" +
						$"[{entry.Date?.ToString("s") ?? "Unknown"}] " +
						$"{entry.Name} in " +
						$"{((float?)entry.Score.Current / 100)?.ToString("N2") ?? "Unknown"} by " +
						$"{entry.Player.Name}"
					);
				}
#endif
			}
		}
		[Conditional("PRO")]
		internal static void GetProfile()
		{
			// Helper
			string FormatChapterTitle(string str)
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

			using (var client = new Portal2BoardsClient())
			{
				client.Log += LogPortal2Boards;

				WriteLine("Fetching profile...");
				var profile = client.GetProfileAsync("Xinera").GetAwaiter().GetResult();

				WriteLine($"User profile of: {profile.SteamName}");
				WriteLine($"User profile of: {profile.DisplayName}");
				WriteLine($"User profile of: {profile.BoardName}");
				foreach (var chapter in profile.Times.SinglePlayerChapters.Chambers)
				{
					WriteLine($"[{FormatChapterTitle(chapter.Key.ToString("G"))}]");
					foreach (var chamber in chapter.Value.Data)
					{
						WriteLine
						(
							$"[{chamber.Key}]\t{(chamber.Value as IEntity<ulong>).Id} " +
							$"in {chamber.Value.Score} : Rank {chamber.Value.PlayerRank}");
					}
				}
			}
		}
		[Conditional("DEM")]
		internal static void GetDemo()
		{
			using (var client = new Portal2BoardsClient())
			{
				client.Log += LogPortal2Boards;

				WriteLine("Fetching demo content...");
				var content = client.GetDemoContentAsync(79120).GetAwaiter().GetResult();
				
				WriteLine($"Bytes: {content?.Length ?? 0}");
			}
		}

		// Logger test
		internal static Task LogPortal2Boards(object _, LogMessage message)
		{
			WriteLine(message.ToString());
			return Task.CompletedTask;
		}

		// Example 1 (LeaderboardWebPage.cs)
		[Conditional("GEN_SP")]
		internal static void GenerateSpPage()
			=> LeaderboardWebPage.GeneratePage("sp.html", Portal2MapType.SinglePlayer).GetAwaiter().GetResult();
		[Conditional("GEN_MP")]
		internal static void GenerateMpPage()
			=> LeaderboardWebPage.GeneratePage("coop.html", Portal2MapType.Cooperative).GetAwaiter().GetResult();
		[Conditional("GEN_STATS")]
		internal static void GenerateStatsPage()
			=> LeaderboardWebPage.GenerateStatsPage("stats.html").GetAwaiter().GetResult();

		// Example 2 (TwitterBot.cs)
		[Conditional("TWBOT")]
		internal static void StartTwitterBot()
		{
			var bot = new TwitterBot();
			_ = bot.InitAsync();
			_ = bot.RunAsync();
		}
	}
}