using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Portal2Boards.API;
using Portal2Boards.Extensions;
using Portal2Boards.Test.Examples;
using Portal2Boards.Test.Examples.History;
using static System.Console;

namespace Portal2Boards.Test
{
    internal static class Logger
    {
        public static Task LogPortal2Boards(object _, LogMessage message)
        {
            WriteLine(message.ToString());
            return Task.CompletedTask;
        }
    }

    internal class Program
    {
        private static readonly ChangelogQuery _latestWorldRecords = new ChangelogQueryBuilder()
            .WithWorldRecord(true)
            .Build();

        private static void Main()
        {
            WriteLine();

            GetAggregated();
            GetChamber();
            GetChangelog();
            GetProfile();
            GetDemo();
            GetWoS();
            GetDonator();

            GenerateLeaderboardPage();
            GenerateHistoryPage();

            StartTwitterBot();

            BugTestOne();
            BugTestTwo();
            BugTestThree();
            TestRuleOne();
            TestRuleTwo();
        }

        [Conditional("AGG")]
        public static void GetAggregated()
        {
            using (var client = new Portal2BoardsClient())
            {
                client.Log += Logger.LogPortal2Boards;

                WriteLine("Fetching aggregated...");
                var aggregated = client.GetAggregatedAsync(ChapterType.HardLight).GetAwaiter().GetResult();

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
        public static void GetChamber()
        {
            using (var client = new Portal2BoardsClient())
            {
                client.Log += Logger.LogPortal2Boards;

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
        public static void GetChangelog()
        {
            using (var client = new Portal2BoardsClient(cacheResetTime: 1))
            {
                client.Log += Logger.LogPortal2Boards;

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
        public static void GetProfile()
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
                client.Log += Logger.LogPortal2Boards;

                WriteLine("Fetching profile...");
                var profile = client.GetProfileAsync("NeKz").GetAwaiter().GetResult();

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
        public static void GetDemo()
        {
            using (var client = new Portal2BoardsClient())
            {
                client.Log += Logger.LogPortal2Boards;

                WriteLine("Fetching demo content...");
                var content = client.GetDemoContentAsync(79120).GetAwaiter().GetResult();

                WriteLine($"Bytes: {content?.Length ?? 0}");
            }
        }
        [Conditional("WOS")]
        public static void GetWoS()
        {
            using (var client = new Portal2BoardsClient())
            {
                client.Log += Logger.LogPortal2Boards;

                WriteLine("Fetching wall of shame...");
                var wos = client.GetWallOfShameAsync().GetAwaiter().GetResult();

                foreach (SteamUser user in wos)
                    WriteLine($"[{user.Id}] {user.Name}");
            }
        }
        [Conditional("DON")]
        public static void GetDonator()
        {
            using (var client = new Portal2BoardsClient())
            {
                client.Log += Logger.LogPortal2Boards;

                WriteLine("Fetching donators...");
                var donators = client.GetDonatorsAsync().GetAwaiter().GetResult();

                foreach (Donator donator in donators)
                    WriteLine($"[{donator.Id}] {donator.Player.Name} -> €{donator.DonationAmount:N2}");
            }
        }

        // Example 1 (Leaderboard.cs)
        [Conditional("STATS")]
        public static void GenerateLeaderboardPage()
        {
            var lb = new Leaderboard();
            lb.Build().GetAwaiter().GetResult();
            lb.ExportPage("gh-pages/stats.html").GetAwaiter().GetResult();
        }

        // Example 2 (TwitterBot.cs)
        [Conditional("TWBOT")]
        public static void StartTwitterBot()
        {
            var bot = new TwitterBot();
            _ = bot.InitAsync();
            _ = bot.RunAsync();
        }

        // Example 3 (History.cs)
        [Conditional("HISTORY")]
        public static void GenerateHistoryPage()
        {
            var lb = new History();
            lb.Build().GetAwaiter().GetResult();
            lb.ExportPage("gh-pages/history.html").GetAwaiter().GetResult();
        }

        [Conditional("BUG_TEST")]
        public static void BugTestOne()
            => new BugHunter().TestOne().GetAwaiter().GetResult();
        [Conditional("BUG_TEST2")]
        public static void BugTestTwo()
            => new BugHunter().TestTwo().GetAwaiter().GetResult();
        [Conditional("BUG_TEST3")]
        public static void BugTestThree()
            => new BugHunter().TestThree().GetAwaiter().GetResult();
        [Conditional("RULE_TEST")]
        public static void TestRuleOne()
            => new BugHunter().TestRuleOne().GetAwaiter().GetResult();
        [Conditional("RULE_TEST2")]
        public static void TestRuleTwo()
            => new BugHunter().TestRuleTwo().GetAwaiter().GetResult();
    }
}
