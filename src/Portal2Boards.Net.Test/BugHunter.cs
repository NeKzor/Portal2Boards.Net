
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Portal2Boards.Extensions;
using static System.Console;

namespace Portal2Boards.Test
{
	internal class TestPlayer
	{
		public ISteamUser User { get; set; }
		public Dictionary<ulong, List<IChangelogEntry>> Chambers { get; set; }
	}

	internal class BugHunter
	{
		// Finds every "wr" that shouldn't be a wr
		public async Task TestOne()
		{
			using (var client = new Portal2BoardsClient("BugHunter/1.0", false))
			{
				client.Log += Logger.LogPortal2Boards;

				var query = new ChangelogQueryBuilder()
					.WithWorldRecord(true)
					.WithBanned(false)
					.WithMaxDaysAgo(3333);
				
				var changelog = await client.GetChangelogAsync(() => query.Build());
				
				WriteLine("--- Invalid World Records ---");
				foreach (var entry in changelog.Entries
					.OrderBy(e => e.MapId)
					.ThenBy(e => e.Date))
				{
					// Make sure it's wr
					if (entry.Rank.Current == 1) continue;

					// Ehem?
					var map = Portal2Map.Search(entry.MapId);
					Write($"[{(entry as IEntity<ulong>).Id}] ");
					Write($"{map.Alias} ");
					Write($"in {entry.Score.Current.AsTimeToString()} ");
					Write($"by {entry.Player.Name} ");
					Write($"at {entry.Date?.ToString("yyyy-MM-dd")}");
					WriteLine();
				}
			}
		}

		// Finds every entry with a "positive" improvement
		public async Task TestTwo()
		{
			using (var client = new Portal2BoardsClient("BugHunter/1.0", false))
			{
				client.Log += Logger.LogPortal2Boards;

				var query = new ChangelogQueryBuilder()
					.WithBanned(false)
					.WithMaxDaysAgo(3333);
				
				var changelog = await client.GetChangelogAsync(() => query.Build());
				
				WriteLine("--- Invalid Rank Improvements ---");
				foreach (var entry in changelog.Entries
					.OrderBy(e => e.Date))
				{
					// Ehem?
					if (entry.Rank.Improvement < 0)
					{
						var map = Portal2Map.Search(entry.MapId);
						Write($"[{(entry as IEntity<ulong>).Id}] ");
						Write($"{map.Alias} ");
						Write($"in {entry.Score.Current.AsTimeToString()} ");
						Write($"by {entry.Player.Name} ");
						Write($"at {entry.Date?.ToString("yyyy-MM-dd")}");
						WriteLine();
					}
				}
				WriteLine("--- Invalid Score Improvements ---");
				foreach (var entry in changelog.Entries
					.OrderBy(e => e.Date))
				{
					// Ehem²?
					if (entry.Score.Improvement < 0)
					{
						var map = Portal2Map.Search(entry.MapId);
						Write($"[{(entry as IEntity<ulong>).Id}] ");
						Write($"{map.Alias} ");
						Write($"in {entry.Score.Current.AsTimeToString()} ");
						Write($"by {entry.Player.Name} ");
						Write($"at {entry.Date?.ToString("yyyy-MM-dd")}");
						WriteLine();
					}
				}
			}
		}

		// Finds every "pb" that shouldn't be a pb
		public async Task TestThree()
		{
			using (var client = new Portal2BoardsClient("BugHunter/1.0", false))
			{
				client.Log += Logger.LogPortal2Boards;

				var query = new ChangelogQueryBuilder()
					.WithBanned(false)
					.WithMaxDaysAgo(3333);
				
				var changelog = await client.GetChangelogAsync(() => query.Build());
				
				var players = new Dictionary<ulong, TestPlayer>();
				
				foreach (var entry in changelog.Entries
					.OrderBy(e => e.Date))
				{
					var id = (entry.Player as SteamUser).Id;
					if (!players.ContainsKey(id))
					{
						players.Add(id, new TestPlayer()
						{
							User = entry.Player,
							Chambers = new Dictionary<ulong, List<IChangelogEntry>>()
						});
					}
					
					var player = players[id];
					if (!player.Chambers.ContainsKey(entry.MapId))
					{
						player.Chambers.Add(entry.MapId, new List<IChangelogEntry>());
					}

					player.Chambers[entry.MapId].Add(entry);
				}

				WriteLine("--- Invalid Scores ---");
				foreach (var player in players
					.Select(p => p.Value))
				{
					foreach (var chamber in player.Chambers
						.Select(p => p.Value))
					{
						var pb = uint.MaxValue;
						foreach (var entry in chamber)
						{
							if (entry.Score.Current < pb)
							{
								pb = (uint)entry.Score.Current;
							}
							else
							{
								var map = Portal2Map.Search(entry.MapId);
								Write($"[{(entry as IEntity<ulong>).Id}] ");
								Write($"{map.Alias} ");
								Write($"in {entry.Score.Current.AsTimeToString()} ");
								Write($"by {entry.Player.Name} ");
								Write($"at {entry.Date?.ToString("yyyy-MM-dd")}");
								WriteLine();
							}
						}
					}
				}

#if TEST_PROFILES
				// TODO
				foreach (var player in players
					.Take(10)
					.Select(p => p.Value))
				{
					var id = (player.User as SteamUser).Id;
					WriteLine($"[{id}] {player.User.Name}");
					_ = await client.GetProfileAsync(id):
				}
#endif
			}
		}
	}
}