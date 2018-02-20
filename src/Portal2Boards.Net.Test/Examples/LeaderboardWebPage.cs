using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Portal2Boards;
using Portal2Boards.Extensions;

namespace Portal2Boards.Test.Examples
{
	internal class LeaderboardWebPage
	{
		private readonly List<string> _page;
		private readonly Portal2BoardsClient _client;

		public LeaderboardWebPage()
		{
			_page = new List<string>();
			_client = new Portal2BoardsClient("LeaderboardWebPage/2.0", false);
			_client.Log += Logger.LogPortal2Boards;
		}

		public async Task GeneratePage(string path, Portal2MapType mode)
		{
			// Local functions
			Task<uint?> GetDuration(DateTime? time)
			{
				if (time == default(DateTime?))
					return Task.FromResult(default(uint?));

				var duration = Math.Abs((DateTime.UtcNow - time.Value.ToUniversalTime()).TotalDays);
				return Task.FromResult((duration < 1) ? default(uint?) : (uint)duration);
			}

			if (File.Exists(path))
				File.Delete(path);
			
			_page.Clear();

			var watch = Stopwatch.StartNew();
			// Head
			_page.Add("<!DOCTYPE html>");
			_page.Add("<html>");
			_page.Add("<head>");
			if (mode == Portal2MapType.Cooperative)
				_page.Add("<title>Portal2Boards.Net | Cooperative WRs</title>");
			else
				_page.Add("<title>Portal2Boards.Net | Single Player WRs</title>");
			_page.Add("<link href=\"https://fonts.googleapis.com/css?family=Roboto\" rel=\"stylesheet\">");
			_page.Add("<style>table,td,th{border-collapse:collapse;border:1px solid #ddd;text-align: left;}table.wrs{width:50%;}table.wrholders{width:20%;}th,td{padding: 3px;}a{color:inherit;text-decoration:none;}a:hover{color:#FF8C00;}</style>");
			_page.Add("</head>");

			// Body
			_page.Add("<body style=\"font-family:'Roboto',sans-serif;color:#FFFFFF;background-color:#23272A;\">");
			_page.Add("<div>");
			_page.Add("<h1 align=\"center\"><a href=\"/Portal2Boards.Net\">Portal2Boards.Net</a></h1>");
			_page.Add
			(
				"<h4 align=\"center\">" +
				"<a href=\"/Portal2Boards.Net/sp\">Single Player WRs</a> | " +
				"<a href=\"/Portal2Boards.Net/mp\">Cooperative WRs</a> | " +
				"<a href=\"/Portal2Boards.Net/wrs\">WRs Statistics</a> | " +
				"<a href=\"/Portal2Boards.Net/stats\">Overall Statistics</a>" +
				"</h4>"
			);

			// First table
			_page.Add("<table align=\"center\" class=\"wrs\">");
			_page.Add("<thead><tr>");
			_page.Add("<th>Map</th>");
			_page.Add("<th>Time</th>");
			_page.Add("<th>Player</th>");
			_page.Add("<th>Date</th>");
			_page.Add("<th>Duration</th>");
			_page.Add("<th>Demo</th>");
			_page.Add("<th>Video</th>");
			_page.Add("</tr></thead>");
			_page.Add("<tbody>");

			var totalscore = 0u;
			var totalscorewithnoncm = 0u;
			var wrholders = new Dictionary<string, UserStats>();
			var maps = ((mode == Portal2MapType.SinglePlayer)
				? Portal2.SinglePlayerMaps
				: Portal2.CooperativeMaps)
				.Where(m => m.Exists)
				.OrderBy(m => m.Index)
				.ToList();
			
			var changelog = await _client.GetChangelogAsync(q =>
			{
				q.WorldRecord = true;
				q.Banned = false;
				q.MaxDaysAgo = 3333;
			});
			
			foreach (var map in maps)
			{
				var entries = changelog.Entries
					.Where(e => e.MapId == map.BestTimeId);
				
				var latestwr = entries
					.OrderByDescending(e => e.Date)
					.First(e => !e.IsBanned);
				
				if (map.IsOfficial)
					totalscore += latestwr.Score.Current ?? 0;
				if (mode == Portal2MapType.SinglePlayer)
					totalscorewithnoncm += latestwr.Score.Current ?? 0;

				var wrs = entries
					.Where(e => e.Score.Current == latestwr.Score.Current)
					.OrderBy(e => e.Date)
					.ToList();
				
				var once = false;
				foreach (var wr in wrs)
				{
					var duration = await GetDuration(wr.Date);
					if (!wrholders.Keys.Contains(wr.Player.Name))
						wrholders.Add(wr.Player.Name, new UserStats());
					
					wrholders[wr.Player.Name] = new UserStats()
					{
						OfficialDuration = wrholders[wr.Player.Name].OfficialDuration
							+ ((map.IsOfficial) ? duration ?? 0 : 0),
						OfficialWorldRecords = wrholders[wr.Player.Name].OfficialWorldRecords
							+ ((map.IsOfficial) ? 1u : 0),
						TotalDuration = wrholders[wr.Player.Name].TotalDuration + duration ?? 0,
						TotalWorldRecords = wrholders[wr.Player.Name].TotalWorldRecords + 1,
						Player = wr.Player as SteamUser
					};

					_page.Add("<tr>");
					if (!once)
					{
						_page.Add($"<td rowspan=\"{wrs.Count}\" align=\"center\"><a href=\"{map.Url}\" title=\"{map.Name}\">{map.Alias}</a>{((map.IsOfficial) ? string.Empty : "<sup>1</sup>")}</td>");
						_page.Add($"<td rowspan=\"{wrs.Count}\">{wr.Score.Current.AsTimeToString() ?? "Error :("}</td>");
						once = true;
					}
					_page.Add($"<td><a href=\"{(wr.Player as SteamUser).Url}\">{wr.Player.Name}</a></td>");
					_page.Add($"<td title=\"{wr.Date?.DateTimeToString() + " (CST)"}\">{((wr.Date != null) ? wr.Date?.ToString("yyyy-MM-dd") : "Unknown")}</td>");
					_page.Add($"<td>{duration?.ToString() ?? "<1"}</td>");
					_page.Add((wr.DemoExists) ? $"<td><a href=\"{(wr as ChangelogEntry).DemoUrl}\">Download</a></td>" : "<td></td>");
					_page.Add(((wr as ChangelogEntry).VideoExists) ? $"<td><a href=\"{(wr as ChangelogEntry).VideoUrl}\">Watch</a></td>" : "<td></td>");
					_page.Add("</tr>");
				}
			}
			if (mode == Portal2MapType.SinglePlayer)
			{
				_page.Add("<tr>");
				_page.Add("<td><b>Official Total</b></td>");
				_page.Add($"<td><b>{totalscorewithnoncm.AsTimeToString()}</b></td>");
				_page.Add("</tr>");
			}
			_page.Add("<tr>");
			_page.Add("<td><b>Total</b></td>");
			_page.Add($"<td><b>{totalscore.AsTimeToString()}</b></td>");
			_page.Add("</tr>");
			_page.Add("</tbody>");
			_page.Add("</table>");
			_page.Add("</div>");

			// Total wrs
			_page.Add("<div>");
			_page.Add($"<br><h3 align=\"center\">{((mode == Portal2MapType.SinglePlayer) ? "Official " : string.Empty)}World Record Holders</h3>");
			_page.Add("<table align=\"center\" class=\"wrholders\">");
			_page.Add("<thead><tr>");
			_page.Add("<th>Player</th>");
			if (mode == Portal2MapType.SinglePlayer)
				_page.Add("<th>Official</th>");
			_page.Add("<th>Total</th>");
			if (mode == Portal2MapType.Cooperative)
				_page.Add("<th>Percentage</th>");
			_page.Add("</tr></thead>");
			_page.Add("<tbody>");
			foreach (var player in wrholders
				.OrderByDescending(p => (mode == Portal2MapType.SinglePlayer)
					? p.Value.OfficialWorldRecords
					: p.Value.TotalWorldRecords))
			{
				_page.Add("<tr>");
				_page.Add($"<td><a href=\"{player.Value.Player.Url}\">{player.Key}</a></td>");
				if (mode == Portal2MapType.SinglePlayer)
					_page.Add($"<td title=\"{(int)(Math.Round((decimal)player.Value.OfficialWorldRecords / maps.Count, 2) * 100)}%\">{player.Value.OfficialWorldRecords}</td>");
				var totalpercentage = (int)(Math.Round((decimal)player.Value.TotalWorldRecords / maps.Count, 2) * 100);
				_page.Add($"<td title=\"{totalpercentage}%\">{player.Value.TotalWorldRecords}</td>");
				if (mode == Portal2MapType.Cooperative)
					_page.Add($"<td>{totalpercentage}%</td>");
				_page.Add("</tr>");
			}
			_page.Add("</tbody>");
			_page.Add("</table>");
			_page.Add("</div>");

			// Duration
			_page.Add("<div>");
			_page.Add($"<br><h3 align=\"center\">{((mode == Portal2MapType.SinglePlayer) ? "Official " : string.Empty)}Duration</h3>");
			_page.Add("<table align=\"center\" class=\"wrholders\">");
			_page.Add("<thead><tr>");
			_page.Add("<th>Player</th>");
			if (mode == Portal2MapType.SinglePlayer)
				_page.Add("<th>Official</th>");
			_page.Add("<th>Total</th>");
			_page.Add("</tr></thead>");
			_page.Add("<tbody>");
			foreach (var player in wrholders
				.OrderByDescending(p => (mode == Portal2MapType.SinglePlayer)
					? p.Value.OfficialDuration
					: p.Value.TotalDuration))
			{
				_page.Add("<tr>");
				_page.Add($"<td><a href=\"{player.Value.Player.Url}\">{player.Key}</a></td>");
				if (mode == Portal2MapType.SinglePlayer)
					_page.Add($"<td>{player.Value.OfficialDuration}</td>");
				_page.Add($"<td>{player.Value.TotalDuration}</td>");
				_page.Add("</tr>");
			}
			_page.Add("</tbody>");
			_page.Add("</table>");
			_page.Add("</div>");

			watch.Stop();
			// Footer
			if (mode == Portal2MapType.SinglePlayer)
				_page.Add("<br><sup>1</sup> Unofficial challenge mode map.");
			_page.Add($"<br>Generated static page in {watch.Elapsed.TotalSeconds.ToString("N3")} seconds.");
			_page.Add("<br><a href=\"https://github.com/NeKzor/Portal2Boards.Net\">Portal2Boards.Net</a> example made by NeKz.");
			_page.Add($"<br>Last update: {DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss '(UTC)'")}");

			// Rest
			_page.Add("</body>");
			_page.Add("</html>");

			File.AppendAllLines(path, _page);
		}

		public async Task GenerateWorldRecordStatsPage(string path)
		{
			if (File.Exists(path))
				File.Delete(path);
			
			_page.Clear();

			var watch = Stopwatch.StartNew();
			// Head
			_page.Add("<!DOCTYPE html>");
			_page.Add("<html>");
			_page.Add("<head>");
			_page.Add("<title>Portal2Boards.Net | WRs Statistics</title>");
			_page.Add("<link href=\"https://fonts.googleapis.com/css?family=Roboto\" rel=\"stylesheet\">");
			_page.Add("<style>table,td,th{border-collapse:collapse;border:1px solid #ddd;text-align: center;padding: 10px;}table.wrs{width:50%;}table.wrholders{margin:0px auto;}th,td{padding: 3px;}a{color:inherit;text-decoration:none;}a:hover{color:#FF8C00;}</style>");
			_page.Add("</head>");

			// Body
			_page.Add("<body style=\"font-family:'Roboto',sans-serif;color:#FFFFFF;background-color:#23272A;\">");
			_page.Add("<div>");
			_page.Add("<h1 align=\"center\"><a href=\"/Portal2Boards.Net\">Portal2Boards.Net</a></h1>");
			_page.Add
			(
				"<h4 align=\"center\">" +
				"<a href=\"/Portal2Boards.Net/sp\">Single Player WRs</a> | " +
				"<a href=\"/Portal2Boards.Net/mp\">Cooperative WRs</a> | " +
				"<a href=\"/Portal2Boards.Net/wrs\">WRs Statistics</a> | " +
				"<a href=\"/Portal2Boards.Net/stats\">Overall Statistics</a>" +
				"</h4>"
			);

			// Data, single API call
			var changelog = await _client.GetChangelogAsync(q =>
			{
				q.WorldRecord = true;
				q.Banned = false;
				q.MaxDaysAgo = 3333;
			});

			var wrh = new List<RecordHolder>();
			foreach (var entry in changelog.Entries)
			{
				var map = Portal2Map.Search(entry.MapId);
				var wr = new Record()
				{
					Map = map,
					Date = entry.Date,
					Entry = entry
				};
				
				var pro = wrh.FirstOrDefault(x => x.Player.Id == (entry.Player as SteamUser).Id);
				if (pro != null)
				{
					pro.Records.Add(wr);
				}
				else
				{
					pro = new RecordHolder()
					{
						Player = entry.Player as SteamUser,
						Records = new List<Record>()
					};
					pro.Records.Add(wr);
					wrh.Add(pro);
				}
			}

			var maps = new List<RecordMap>();
			foreach (var map in Portal2.CampaignMaps.Where(m => m.Exists))
			{
				var wrs = changelog.Entries
					.Where(e => e.MapId == map.BestTimeId)
					.OrderBy(e => e.Date)
					.ToList();
				
				maps.Add(new RecordMap()
				{
					Map = map,
					Records = wrs
				});
			}

			var durations = maps
				.SelectMany(m => m.GetDurations());
			var improvements = maps
				.SelectMany(m => m.GetImprovements());

			var wri = new List<(int Improvements, Portal2Map Map)>();
			foreach (var map in maps)
			{
				var imp = 0;
				var last = map.Records
					.OrderBy(r => r.Score.Current)
					.First();
				
				foreach (var wr in map.Records
					.OrderBy(r => r.Score.Current)
					.Skip(1))
				{
					if (last.Score.Current < wr.Score.Current)
						imp++;
					last = wr;
				}
				wri.Add((imp, map.Map));
			}

			// Most World Records
			_page.Add("<div>");
			_page.Add("<br><h3 align=\"center\">Most World Records</h3>");
			_page.Add("<table align=\"center\" class=\"wrholders\">");
			_page.Add("<thead><tr>");
			_page.Add("<th>Player</th>");
			_page.Add("<th>Total</th>");
			for (int year = 2013; year < 2019; year++)
				_page.Add($"<th>{year}</th>");
			_page.Add("</tr></thead>");
			_page.Add("<tbody>");
			foreach (var wr in wrh
				.OrderByDescending(h => h.Records.Count)
				.ThenBy(r => r.Records.First().Date))
			{
				_page.Add("<tr>");
				_page.Add($"<td><a href=\"{wr.Player.Url}\">{wr.Player.Name}</a></td>");
				_page.Add($"<td title=\"{wr.Records.Count(r => r.Map.IsOfficial)} Official\">{wr.Records.Count}</td>");
				for (int year = 2013; year < 2019; year++)
				{
					var total = wr.Records.Where(r => r.Date.Value.Year == year);
					var official = total.Where(r => r.Map.IsOfficial);
					_page.Add($"<td title=\"{official.Count()} Official\">{total.Count()}</td>");
				}
				_page.Add("</tr>");
			}
			_page.Add("</tbody>");
			_page.Add("</table>");
			_page.Add("</div>");

			// Unique World Records of Players
			_page.Add("<div>");
			_page.Add("<br><h3 align=\"center\">Unique World Records of Players</h3>");
			_page.Add("<table align=\"center\" class=\"wrholders\">");
			_page.Add("<thead><tr>");
			_page.Add("<th>Player</th>");
			_page.Add("<th>Total</th>");
			_page.Add("<th title=\"SP:COOP\">Mode Ratio</th>");
			_page.Add("</tr></thead>");
			_page.Add("<tbody>");
			foreach (var wr in wrh.OrderByDescending(h => h.UniqueRecords.Count))
			{
				var unique = wr.UniqueRecords;	
				var sp = unique.Count(r => r.Map.Type == Portal2MapType.SinglePlayer);
				var mp = unique.Count(r => r.Map.Type == Portal2MapType.Cooperative);
				var osp = unique.Count(r => r.Map.Type == Portal2MapType.SinglePlayer && r.Map.IsOfficial);
				var omp = unique.Count(r => r.Map.Type == Portal2MapType.Cooperative && r.Map.IsOfficial);

				_page.Add("<tr>");
				_page.Add($"<td><a href=\"{wr.Player.Url}\">{wr.Player.Name}</a></td>");
				_page.Add($"<td title=\"{unique.Count(r => r.Map.IsOfficial)} Official\">{unique.Count}</td>");
				_page.Add($"<td title=\"{osp}:{omp} Official\">{sp}:{mp}</td>");
				_page.Add("</tr>");
			}
			_page.Add("</tbody>");
			_page.Add("</table>");
			_page.Add("</div>");

			// World Records Activity
			_page.Add("<div>");
			_page.Add("<br><h3 align=\"center\">World Records Activity</h3>");
			_page.Add("<table align=\"center\" class=\"wrholders\">");
			_page.Add("<thead><tr>");
			_page.Add("<th>Year/Month</th>");
			_page.Add("<th>Total</th>");
			_page.Add("<th>Jan</th>");
			_page.Add("<th>Feb</th>");
			_page.Add("<th>Mar</th>");
			_page.Add("<th>Apr</th>");
			_page.Add("<th>May</th>");
			_page.Add("<th>Jun</th>");
			_page.Add("<th>Jul</th>");
			_page.Add("<th>Aug</th>");
			_page.Add("<th>Sep</th>");
			_page.Add("<th>Oct</th>");
			_page.Add("<th>Nov</th>");
			_page.Add("<th>Dec</th>");
			_page.Add("</tr></thead>");
			_page.Add("<tbody>");
			for (int i = 2013; i < 2019; i++)
			{
				var year = changelog.Entries
					.Where(r => r.Date.Value.Year == i)
					.ToList();
				var oyear = changelog.Entries
					.Where(r => Portal2Map.Search(r.MapId).IsOfficial)
					.Where(r => r.Date.Value.Year == i)
					.ToList();

				_page.Add("<tr>");
				_page.Add($"<td>{i}</td>");
				_page.Add($"<td title=\"{oyear.Count} Official\">{year.Count}</td>");
				for (int j = 1; j < 13; j++)
				{
					var month = year
						.Where(r => r.Date.Value.Month == j)
						.ToList();
					var omonth = year
						.Where(r => Portal2Map.Search(r.MapId).IsOfficial)
						.Where(r => r.Date.Value.Month == j)
						.ToList();

					_page.Add($"<td title=\"{omonth.Count} Official\">{month.Count}</td>");
				}
				_page.Add("</tr>");
			}
			_page.Add("</tbody>");
			_page.Add("</table>");
			_page.Add("</div>");

			// Most World Records per Year
			_page.Add("<div>");
			_page.Add("<br><h3 align=\"center\">Most World Records per Year</h3>");
			_page.Add("<table align=\"center\" class=\"wrholders\">");
			_page.Add("<thead><tr>");
			_page.Add("<th>Year</th>");
			_page.Add("<th>Player</th>");
			_page.Add("<th>Total</th>");
			_page.Add("</tr></thead>");
			_page.Add("<tbody>");
			for (int year = 2013; year < 2019; year++)
			{
				var players = wrh
					.OrderByDescending(rh => rh.Records
						.Count(r => r.Date.Value.Year == year));
				var most = players
					.First().Records
					.Count(r => r.Date.Value.Year == year);
				
				foreach (var player in players
					.Where(rh => rh.Records
						.Count(r => r.Date.Value.Year == year) == most))
				{
					var recs = player.Records.Count(r => r.Date.Value.Year == year);
					var off = player.Records.Count(r => r.Map.IsOfficial);

					_page.Add("<tr>");
					_page.Add($"<td>{year}</td>");
					_page.Add($"<td><a href=\"{player.Player.Url}\">{player.Player.Name}</a></td>");
					_page.Add($"<td title=\"{off} Official\">{recs}</td>");
					_page.Add("</tr>");
				}
			}
			_page.Add("</tbody>");
			_page.Add("</table>");
			_page.Add("</div>");

			// Longest Lasting World Records
			_page.Add("<div>");
			_page.Add("<br><h3 align=\"center\">Longest Lasting World Records</h3>");
			_page.Add("<table align=\"center\" class=\"wrholders\">");
			_page.Add("<thead><tr>");
			_page.Add("<th>Map</th>");
			_page.Add("<th>Time</th>");
			_page.Add("<th>Player</th>");
			_page.Add("<th>Duration</th>");
			_page.Add("<th>Start</th>");
			_page.Add("<th>End</th>");
			_page.Add("</tr></thead>");
			_page.Add("<tbody>");
			foreach (var stats in durations
				.OrderByDescending(d => d.Duration)
				.Take(20))
			{
				var (duration, current, previous, map) = stats;
				_page.Add("<tr>");
				_page.Add($"<td><a href=\"{map.Url}\">{map.Alias}</a></td>");
				_page.Add($"<td>{current.Score.Current.AsTimeToString()}</td>");
				_page.Add($"<td><a href=\"{(current.Player as SteamUser).Url}\">{current.Player.Name}</a></td>");
				_page.Add($"<td>{((duration < 1) ? "<1" : $"{duration}")}</td>");
				_page.Add($"<td title=\"{current.Date.DateTimeToString() + " (CST)"}\">{(current.Date?.ToString("yyy-MM-dd"))}</td>");
				if (previous != null)
					_page.Add($"<td title=\"{previous.Date.DateTimeToString() + " (CST)"}\">{(previous.Date?.ToString("yyy-MM-dd"))}</td>");
				else
					_page.Add("<td>Ongoing</td>");
				_page.Add("</tr>");
			}
			_page.Add("</tbody>");
			_page.Add("</table>");
			_page.Add("</div>");

			// Longest Lasting World Records History
			_page.Add("<div>");
			_page.Add("<br><h3 align=\"center\">Longest Lasting World Records History</h3>");
			_page.Add("<table align=\"center\" class=\"wrholders\">");
			_page.Add("<thead><tr>");
			_page.Add("<th>Map</th>");
			_page.Add("<th>Time</th>");
			_page.Add("<th>Player</th>");
			_page.Add("<th>Duration</th>");
			_page.Add("<th>Start</th>");
			_page.Add("<th>End</th>");
			_page.Add("</tr></thead>");
			_page.Add("<tbody>");
			var highest = default((int Duration, IChangelogEntry Current, IChangelogEntry Previous, Portal2Map Map));
			foreach (var stats in durations
				.OrderBy(d => d.Current.Date))
			{
				if (stats.Duration > highest.Duration)
				{
					var (duration, current, previous, map) = stats;
					_page.Add("<tr>");
					_page.Add($"<td><a href=\"{map.Url}\">{map.Alias}</a></td>");
					_page.Add($"<td>{current.Score.Current.AsTimeToString()}</td>");
					_page.Add($"<td><a href=\"{(current.Player as SteamUser).Url}\">{current.Player.Name}</a></td>");
					_page.Add($"<td>{((duration < 1) ? "<1" : $"{duration}")}</td>");
					_page.Add($"<td title=\"{current.Date.DateTimeToString() + " (CST)"}\">{(current.Date?.ToString("yyy-MM-dd"))}</td>");
					if (previous != null)
						_page.Add($"<td title=\"{previous.Date.DateTimeToString() + " (CST)"}\">{(previous.Date?.ToString("yyy-MM-dd"))}</td>");
					else
						_page.Add("<td>Ongoing</td>");
					_page.Add("</tr>");

					highest = stats;
				}
			}
			_page.Add("</tbody>");
			_page.Add("</table>");
			_page.Add("</div>");

			// Total Time of World Records
			_page.Add("<div>");
			_page.Add("<br><h3 align=\"center\">Total Time of World Records</h3>");
			_page.Add("<table align=\"center\" class=\"wrholders\">");
			_page.Add("<thead><tr>");
			_page.Add("<th>Year</th>");
			_page.Add("<th>Single Player</th>");
			_page.Add("<th>Cooperative</th>");
			_page.Add("</tr></thead>");
			_page.Add("<tbody>");
			for (int year = 2014; year < 2019; year++)
			{
				Func<RecordMap, long?> SumScores = (m) =>
				{
					if (!m.Records.Any(r => r.Date.Value.Year <= year))
						{
							Console.WriteLine($"Ignored {m.Map.Alias} at {year}.");
							return 0;
						}
						return m.Records
							.Where(r => r.Date.Value.Year <= year)
							.OrderByDescending(r => r.Date)
							.First()
							.Score.Current;
				};

				var sp = maps
					.Where(m => m.Map.Type == Portal2MapType.SinglePlayer)
					.Sum(SumScores);
				var mp = maps
					.Where(m => m.Map.Type == Portal2MapType.Cooperative)
					.Sum(SumScores);
				var osp = maps
					.Where(m => m.Map.Type == Portal2MapType.SinglePlayer)
					.Where(m => m.Map.IsOfficial)
					.Sum(SumScores);
				var omp = maps
					.Where(m => m.Map.Type == Portal2MapType.Cooperative)
					.Where(m => m.Map.IsOfficial)
					.Sum(SumScores);

				_page.Add("<tr>");
				_page.Add($"<td>{year}</td>");
				_page.Add($"<td title=\"{((uint)osp).AsTimeToString()} Official\">{((uint)sp).AsTimeToString()}</td>");
				_page.Add($"<td title=\"{((uint)omp).AsTimeToString()} Official\">{((uint)mp).AsTimeToString()}</td>");
				_page.Add("</tr>");
			}
			_page.Add("</tbody>");
			_page.Add("</table>");
			_page.Add("</div>");

			// Most World Record Improvements
			_page.Add("<div>");
			_page.Add("<br><h3 align=\"center\">Most World Record Improvements</h3>");
			_page.Add("<table align=\"center\" class=\"wrholders\">");
			_page.Add("<thead><tr>");
			_page.Add("<th>Map</th>");
			_page.Add("<th>World Records</th>");
			_page.Add("</tr></thead>");
			_page.Add("<tbody>");
			foreach (var improvement in wri
				.OrderByDescending(i => i.Improvements)
				.Take(20))
			{
				_page.Add("<tr>");
				_page.Add($"<td><a href=\"{improvement.Map.Url}\">{improvement.Map.Alias}</a></td>");
				_page.Add($"<td>{improvement.Improvements}</td>");
				_page.Add("</tr>");
			}
			_page.Add("</tbody>");
			_page.Add("</table>");
			_page.Add("</div>");

			// Largest World Record Improvement
			_page.Add("<div>");
			_page.Add("<br><h3 align=\"center\">Largest World Record Improvement</h3>");
			_page.Add("<table align=\"center\" class=\"wrholders\">");
			_page.Add("<thead><tr>");
			_page.Add("<th>Map</th>");
			_page.Add("<th>Time</th>");
			_page.Add("<th>Player</th>");
			_page.Add("<th>Improvement</th>");
			_page.Add("<th>Date Set</th>");
			_page.Add("</tr></thead>");
			_page.Add("<tbody>");
			foreach (var stats in improvements
				.OrderByDescending(d => d.Improvement)
				.ThenBy(d => d.Current.Date)
				.Take(20))
			{
				var (improvement, current, previous, map) = stats;
				_page.Add("<tr>");
				_page.Add($"<td><a href=\"{map.Url}\">{map.Alias}</a></td>");
				_page.Add($"<td>{current.Score.Current.AsTimeToString()}</td>");
				_page.Add($"<td><a href=\"{(current.Player as SteamUser).Url}\">{current.Player.Name}</a></td>");
				_page.Add($"<td>{((uint)improvement).AsTimeToString()}</td>");
				_page.Add($"<td title=\"{current.Date.DateTimeToString() + " (CST)"}\">{(current.Date?.ToString("yyy-MM-dd"))}</td>");
				_page.Add("</tr>");
			}
			_page.Add("</tbody>");
			_page.Add("</table>");
			_page.Add("</div>");

			// Others
			var longest = maps
				.SelectMany(m => m.Records)
				.Where(m => !string.IsNullOrEmpty(m.Comment))
				.OrderByDescending(c => c.Comment.Length);
			var shortest = maps
				.SelectMany(m => m.Records)
				.Where(m => !string.IsNullOrEmpty(m.Comment))
				.OrderBy(c => c.Comment.Length);
			
			_page.Add("<div>");
			_page.Add("<br><h3 align=\"center\">Longest World Record Comment</h3>");
			
			_page.Add("<table align=\"center\" class=\"wrholders\">");
			_page.Add("<thead><tr>");
			_page.Add("<th>Player</th>");
			_page.Add("<th>Length</th>");
			_page.Add("<th>Comment</th>");
			_page.Add("</tr></thead>");
			_page.Add("<tbody>");
			foreach (var others in longest.Take(5))
			{
				_page.Add("<tr>");
				_page.Add($"<td><a href=\"{(others.Player as SteamUser).Url}\">{others.Player.Name}</a></td>");
				_page.Add($"<td>{others.Comment.Length}</td>");
				_page.Add($"<td>{others.Comment}</td>");
				_page.Add("</tr>");
			}
			_page.Add("</tbody>");
			_page.Add("</table>");

			_page.Add("<br><h3 align=\"center\">Shortest World Record Comment</h3>");
			_page.Add("<table align=\"center\" class=\"wrholders\">");
			_page.Add("<thead><tr>");
			_page.Add("<th>Player</th>");
			_page.Add("<th>Length</th>");
			_page.Add("<th>Comment</th>");
			_page.Add("</tr></thead>");
			_page.Add("<tbody>");
			foreach (var others in shortest.Take(5))
			{
				_page.Add("<tr>");
				_page.Add($"<td><a href=\"{(others.Player as SteamUser).Url}\">{others.Player.Name}</a></td>");
				_page.Add($"<td>{others.Comment.Length}</td>");
				_page.Add($"<td>{others.Comment}</td>");
				_page.Add("</tr>");
			}
			_page.Add("</tbody>");
			_page.Add("</table>");

			_page.Add("<br><h3 align=\"center\">Average World Record Comment Length</h3>");
			var avgcomment = maps
				.SelectMany(m => m.Records)
				.Where(e => !string.IsNullOrEmpty(e.Comment))
				.Average(e => e.Comment.Length);
			_page.Add($"<br><h2 align=\"center\">{avgcomment.ToString("N2")}</h2>");

			_page.Add("<br><h3 align=\"center\">Percentage of World Record Proof</h3>");
			var since = DateTime.Parse("2017-05-11");
			var requireproof = maps
				.SelectMany(m => m.Records)
				.Where(e => e.Date >= since)
				.Where(e => e.Rank.Current <= 5);
			var proof = requireproof
				.Count(e => e.DemoExists || (e as ChangelogEntry).VideoExists);
			var proofornoproof = requireproof
				.Count();
			_page.Add($"<br><h2 align=\"center\">{((double)proof / proofornoproof * 100).ToString("N2")}%</h2>");
			_page.Add("</div>");

			watch.Stop();
			// Footer
			_page.Add($"<br>Generated static page in {watch.Elapsed.TotalSeconds.ToString("N3")} seconds.");
			_page.Add("<br><a href=\"https://github.com/NeKzor/Portal2Boards.Net\">Portal2Boards.Net</a> example made by NeKz.");
			_page.Add($"<br>Last update: {DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss '(UTC)'")}");

			// Rest
			_page.Add("</body>");
			_page.Add("</html>");

			File.AppendAllLines(path, _page);
		}

		public async Task GenerateStatsPage(string path)
		{
			if (File.Exists(path))
				File.Delete(path);
			
			_page.Clear();

			var watch = Stopwatch.StartNew();
			// Head
			_page.Add("<!DOCTYPE html>");
			_page.Add("<html>");
			_page.Add("<head>");
			_page.Add("<title>Portal2Boards.Net | Overall Statistics</title>");
			_page.Add("<link href=\"https://fonts.googleapis.com/css?family=Roboto\" rel=\"stylesheet\">");
			_page.Add("<style>table,td,th{border-collapse:collapse;border:1px solid #ddd;text-align: center;padding: 10px;}table.wrs{width:50%;}table.wrholders{margin:0px auto;}th,td{padding: 3px;}a{color:inherit;text-decoration:none;}a:hover{color:#FF8C00;}</style>");
			_page.Add("</head>");

			// Body
			_page.Add("<body style=\"font-family:'Roboto',sans-serif;color:#FFFFFF;background-color:#23272A;\">");
			_page.Add("<div>");
			_page.Add("<h1 align=\"center\"><a href=\"/Portal2Boards.Net\">Portal2Boards.Net</a></h1>");
			_page.Add
			(
				"<h4 align=\"center\">" +
				"<a href=\"/Portal2Boards.Net/sp\">Single Player WRs</a> | " +
				"<a href=\"/Portal2Boards.Net/mp\">Cooperative WRs</a> | " +
				"<a href=\"/Portal2Boards.Net/wrs\">WRs Statistics</a> | " +
				"<a href=\"/Portal2Boards.Net/stats\">Overall Statistics</a>" +
				"</h4>"
			);

			// Data, single API call
			var changelog = await _client.GetChangelogAsync(q =>
			{
				q.MaxDaysAgo = 3333;
			});

			var all = new List<RecordHolder>();
			foreach (var entry in changelog.Entries)
			{
				var map = Portal2Map.Search(entry.MapId);
				var rec = new Record()
				{
					Map = map,
					Date = entry.Date,
					Entry = entry
				};
				
				var pro = all.FirstOrDefault(x => x.Player.Id == (entry.Player as SteamUser).Id);
				if (pro != null)
				{
					if (!entry.IsBanned)
						pro.Records.Add(rec);
					else
						pro.BannedRecords.Add(rec);
				}
				else
				{
					pro = new RecordHolder()
					{
						Player = entry.Player as SteamUser,
						Records = new List<Record>(),
						BannedRecords = new List<Record>()
					};
					if (!entry.IsBanned)
						pro.Records.Add(rec);
					else
						pro.BannedRecords.Add(rec);
					all.Add(pro);
				}
			}

			var maps = new List<RecordMap>();
			var mapbans = new List<RecordMap>();
			foreach (var map in Portal2.CampaignMaps.Where(m => m.Exists))
			{
				var recs = changelog.Entries
					.Where(e => e.MapId == map.BestTimeId)
					.OrderBy(e => e.Date);
				
				maps.Add(new RecordMap()
				{
					Map = map,
					Records = recs.Where(e => !e.IsBanned).ToList()
				});
				mapbans.Add(new RecordMap()
				{
					Map = map,
					Records = recs.Where(e => e.IsBanned).ToList()
				});
			}

			// Most Personal Records
			_page.Add("<div>");
			_page.Add("<br><h3 align=\"center\">Most Personal Records</h3>");
			_page.Add("<table align=\"center\" class=\"wrholders\">");
			_page.Add("<thead><tr>");
			_page.Add("<th>Player</th>");
			_page.Add("<th>Total</th>");
			for (int year = 2013; year < 2019; year++)
				_page.Add($"<th>{year}</th>");
			_page.Add("</tr></thead>");
			_page.Add("<tbody>");
			var always = new List<RecordHolder>();
			foreach (var player in all
				.OrderByDescending(h => h.Records.Count)
				.Take(100))
			{
				var recs = player.Records.Count;
				var off = player.Records.Count(r => r.Map.IsOfficial);

				_page.Add("<tr>");
				_page.Add($"<td><a href=\"{player.Player.Url}\">{player.Player.Name}</a></td>");
				_page.Add($"<td title=\"{off} Official\">{recs}</td>");
				var active = 0;
				for (int year = 2013; year < 2019; year++)
				{
					var total = player.Records.Where(r => r.Date.Value.Year == year);
					var official = total.Where(r => r.Map.IsOfficial);
					if (total.Count() > 0) active++;
					_page.Add($"<td title=\"{official.Count()} Official\">{total.Count()}</td>");
				}
				if (active == 6)
					always.Add(player);
				_page.Add("</tr>");
			}
			_page.Add("</tbody>");
			_page.Add("</table>");
			_page.Add("</div>");

			// Activity
			_page.Add("<div>");
			_page.Add("<br><h3 align=\"center\">Activity</h3>");
			_page.Add("<table align=\"center\" class=\"wrholders\">");
			_page.Add("<thead><tr>");
			_page.Add("<th>Year/Month</th>");
			_page.Add("<th>Total</th>");
			_page.Add("<th>Jan</th>");
			_page.Add("<th>Feb</th>");
			_page.Add("<th>Mar</th>");
			_page.Add("<th>Apr</th>");
			_page.Add("<th>May</th>");
			_page.Add("<th>Jun</th>");
			_page.Add("<th>Jul</th>");
			_page.Add("<th>Aug</th>");
			_page.Add("<th>Sep</th>");
			_page.Add("<th>Oct</th>");
			_page.Add("<th>Nov</th>");
			_page.Add("<th>Dec</th>");
			_page.Add("</tr></thead>");
			_page.Add("<tbody>");
			for (int i = 2013; i < 2019; i++)
			{
				var year = changelog.Entries
					.Where(e => !e.IsBanned)
					.Where(r => r.Date.Value.Year == i)
					.ToList();
				var oyear = changelog.Entries
					.Where(e => !e.IsBanned)
					.Where(r => Portal2Map.Search(r.MapId).IsOfficial)
					.Where(r => r.Date.Value.Year == i)
					.ToList();

				_page.Add("<tr>");
				_page.Add($"<td>{i}</td>");
				_page.Add($"<td title=\"{oyear.Count} Official\">{year.Count}</td>");
				for (int j = 1; j < 13; j++)
				{
					var month = year
						.Where(r => r.Date.Value.Month == j)
						.ToList();
					var omonth = year
						.Where(r => Portal2Map.Search(r.MapId).IsOfficial)
						.Where(r => r.Date.Value.Month == j)
						.ToList();

					_page.Add($"<td title=\"{omonth.Count} Official\">{month.Count}</td>");
				}
				_page.Add("</tr>");
			}
			_page.Add("</tbody>");
			_page.Add("</table>");
			_page.Add("</div>");

			// Most Records per Year
			_page.Add("<div>");
			_page.Add("<br><h3 align=\"center\">Most Records per Year</h3>");
			_page.Add("<table align=\"center\" class=\"wrholders\">");
			_page.Add("<thead><tr>");
			_page.Add("<th>Year</th>");
			_page.Add("<th>Player</th>");
			_page.Add("<th>Total</th>");
			_page.Add("</tr></thead>");
			_page.Add("<tbody>");
			for (int year = 2013; year < 2019; year++)
			{
				var players = all
					.OrderByDescending(rh => rh.Records
						.Count(r => r.Date.Value.Year == year));
				var most = players
					.First().Records
					.Count(r => r.Date.Value.Year == year);
				
				var once = false;
				var mostrecs = players
					.Where(rh => rh.Records.Count(r => r.Date.Value.Year == year) == most);
				foreach (var player in mostrecs)
				{
					var recs = player.Records.Count(r => r.Date.Value.Year == year);
					var off = player.Records.Count(r => r.Map.IsOfficial);

					_page.Add("<tr>");
					if (!once)
					{
						_page.Add($"<td rowspan=\"{mostrecs.Count()}\" align=\"center\">{year}</td>");
						once = true;
					}
					_page.Add($"<td><a href=\"{player.Player.Url}\">{player.Player.Name}</a></td>");
					_page.Add($"<td title=\"{off} Official\">{recs}</td>");
					_page.Add("</tr>");
				}
			}
			_page.Add("</tbody>");
			_page.Add("</table>");
			_page.Add("</div>");

			// At Least One Record Every Year
			_page.Add("<div>");
			_page.Add("<br><h3 align=\"center\">At Least One Record Every Year</h3>");
			_page.Add("<table align=\"center\" class=\"wrholders\">");
			_page.Add("<thead><tr>");
			_page.Add("<th>Player</th>");
			_page.Add("<th>Total</th>");
			for (int year = 2013; year < 2019; year++)
				_page.Add($"<th>{year}</th>");
			_page.Add("</tr></thead>");
			_page.Add("<tbody>");
			foreach (var player in always
				.OrderByDescending(h => h.Records.Count)
				.Take(10))
			{
				var recs = player.Records.Count;
				var off = player.Records.Count(r => r.Map.IsOfficial);

				_page.Add("<tr>");
				_page.Add($"<td><a href=\"{player.Player.Url}\">{player.Player.Name}</a></td>");
				_page.Add($"<td title=\"{off} Official\">{recs}</td>");
				for (int year = 2013; year < 2019; year++)
				{
					var total = player.Records.Where(r => r.Date.Value.Year == year);
					var official = total.Where(r => r.Map.IsOfficial);
					_page.Add($"<td title=\"{official.Count()} Official\">{total.Count()}</td>");
				}
				_page.Add("</tr>");
			}
			_page.Add("</tbody>");
			_page.Add("</table>");
			_page.Add("</div>");

			// New Players per Year
			_page.Add("<div>");
			_page.Add("<br><h3 align=\"center\">New Players per Year</h3>");
			_page.Add("<table align=\"center\" class=\"wrholders\">");
			_page.Add("<thead><tr>");
			_page.Add("<th>Year</th>");
			_page.Add("<th>Players</th>");
			_page.Add("<th>Total</th>");
			_page.Add("<th>Growth</th>");
			_page.Add("</tr></thead>");
			_page.Add("<tbody>");
			var peeps = all
				.Where(rh => rh.Records.Any(r => r.Date.Value.Year == 2013));
			var totalpeeps = peeps.Count();
			var lastpeeps = 0;
			for (int year = 2013; year < 2019; year++)
			{
				var players = all
					.Where(rh => rh.Records.Any(r => r.Date.Value.Year == year))
					.Where(rh => !peeps.Contains(rh))
					.Count();
				totalpeeps += players;

				_page.Add("<tr>");
				_page.Add($"<td>{year}</td>");
				_page.Add($"<td>{players}</td>");
				_page.Add($"<td>{totalpeeps}</td>");
				_page.Add($"<td>{((lastpeeps != 0) ? ((double)players / lastpeeps) * 100 : 0).ToString("N2")}%</td>");
				_page.Add("</tr>");
				lastpeeps = totalpeeps;
			}
			_page.Add("</tbody>");
			_page.Add("</table>");
			_page.Add("</div>");

			// Most Second Places
			_page.Add("<div>");
			_page.Add("<br><h3 align=\"center\">Most Second Places</h3>");
			_page.Add("<table align=\"center\" class=\"wrholders\">");
			_page.Add("<thead><tr>");
			_page.Add("<th>Year</th>");
			_page.Add("<th>Player</th>");
			_page.Add("<th>Total</th>");
			_page.Add("</tr></thead>");
			_page.Add("<tbody>");
			for (int year = 2013; year < 2019; year++)
			{
				var players = all
					.OrderByDescending(rh => rh.Records
						.Where(r => r.Entry.Rank.Current == 2)
						.Count(r => r.Date.Value.Year == year));
				var most = players
					.First().Records
					.Where(r => r.Entry.Rank.Current == 2)
					.Count(r => r.Date.Value.Year == year);
				
				var once = false;
				var rank2 = players
					.Where(rh => rh.Records
						.Where(r => r.Entry.Rank.Current == 2)
						.Count(r => r.Date.Value.Year == year) == most);
				foreach (var player in rank2)
				{
					var recs = player.Records
						.Where(r => r.Entry.Rank.Current == 2)
						.Count(r => r.Date.Value.Year == year);
					var off = player.Records
						.Where(r => r.Entry.Rank.Current == 2)
						.Where(r => r.Date.Value.Year == year)
						.Count(r => r.Map.IsOfficial);
					
					_page.Add("<tr>");
					if (!once)
					{
						_page.Add($"<td rowspan=\"{rank2.Count()}\" align=\"center\">{year}</td>");
						once = true;
					}
					_page.Add($"<td><a href=\"{player.Player.Url}\">{player.Player.Name}</a></td>");
					_page.Add($"<td title=\"{off} Official\">{recs}</td>");
					_page.Add("</tr>");
				}
			}
			_page.Add("</tbody>");
			_page.Add("</table>");
			_page.Add("</div>");

			// Most Records
			_page.Add("<div>");
			_page.Add("<br><h3 align=\"center\">Most Records</h3>");
			_page.Add("<table align=\"center\" class=\"wrholders\">");
			_page.Add("<thead><tr>");
			_page.Add("<th>Map</th>");
			_page.Add("<th>Records</th>");
			_page.Add("</tr></thead>");
			_page.Add("<tbody>");
			foreach (var map in maps
				.OrderByDescending(m => m.Records.Count)
				.Take(20))
			{
				_page.Add("<tr>");
				_page.Add($"<td><a href=\"{map.Map.Url}\">{map.Map.Alias}</a></td>");
				_page.Add($"<td>{map.Records.Count}</td>");
				_page.Add("</tr>");
			}
			_page.Add("</tbody>");
			_page.Add("</table>");
			_page.Add("</div>");

			// Least Records
			_page.Add("<div>");
			_page.Add("<br><h3 align=\"center\">Least Records</h3>");
			_page.Add("<table align=\"center\" class=\"wrholders\">");
			_page.Add("<thead><tr>");
			_page.Add("<th>Map</th>");
			_page.Add("<th>Records</th>");
			_page.Add("</tr></thead>");
			_page.Add("<tbody>");
			foreach (var map in maps
				.OrderBy(m => m.Records.Count)
				.Take(20))
			{
				_page.Add("<tr>");
				_page.Add($"<td><a href=\"{map.Map.Url}\">{map.Map.Alias}</a></td>");
				_page.Add($"<td>{map.Records.Count}</td>");
				_page.Add("</tr>");
			}
			_page.Add("</tbody>");
			_page.Add("</table>");
			_page.Add("</div>");

			// Others
			var longest = maps
				.SelectMany(m => m.Records)
				.Where(e => !string.IsNullOrEmpty(e.Comment))
				.OrderByDescending(e => e.Comment.Length)
				.ThenBy(e => e.Date);
			var shortest = maps
				.SelectMany(m => m.Records)
				.Where(e => !string.IsNullOrEmpty(e.Comment))
				.OrderBy(e => e.Comment.Length)
				.ThenBy(e => e.Date);
			
			_page.Add("<div>");
			_page.Add("<br><h3 align=\"center\">Longest Comment</h3>");
			
			_page.Add("<table align=\"center\" class=\"wrholders\">");
			_page.Add("<thead><tr>");
			_page.Add("<th>Player</th>");
			_page.Add("<th>Length</th>");
			_page.Add("<th>Comment</th>");
			_page.Add("</tr></thead>");
			_page.Add("<tbody>");
			foreach (var others in longest.Take(10))
			{
				_page.Add("<tr>");
				_page.Add($"<td><a href=\"{(others.Player as SteamUser).Url}\">{others.Player.Name}</a></td>");
				_page.Add($"<td>{others.Comment.Length}</td>");
				_page.Add($"<td>{others.Comment}</td>");
				_page.Add("</tr>");
			}
			_page.Add("</tbody>");
			_page.Add("</table>");

			_page.Add("<br><h3 align=\"center\">Shortest Comment</h3>");
			_page.Add("<table align=\"center\" class=\"wrholders\">");
			_page.Add("<thead><tr>");
			_page.Add("<th>Player</th>");
			_page.Add("<th>Length</th>");
			_page.Add("<th>Comment</th>");
			_page.Add("</tr></thead>");
			_page.Add("<tbody>");
			foreach (var others in shortest.Take(10))
			{
				_page.Add("<tr>");
				_page.Add($"<td><a href=\"{(others.Player as SteamUser).Url}\">{others.Player.Name}</a></td>");
				_page.Add($"<td>{others.Comment.Length}</td>");
				_page.Add($"<td>{others.Comment}</td>");
				_page.Add("</tr>");
			}
			_page.Add("</tbody>");
			_page.Add("</table>");

			_page.Add("<br><h3 align=\"center\">Average Comment Length</h3>");
			var avgcomment = maps
				.SelectMany(m => m.Records)
				.Where(e => !string.IsNullOrEmpty(e.Comment))
				.Average(e => e.Comment.Length);
			_page.Add($"<br><h2 align=\"center\">{avgcomment.ToString("N2")}</h2>");

			_page.Add("<br><h3 align=\"center\">Most Banned Records</h3>");
			_page.Add("<table align=\"center\" class=\"wrholders\">");
			_page.Add("<thead><tr>");
			_page.Add("<th>Map</th>");
			_page.Add("<th>Bans</th>");
			_page.Add("</tr></thead>");
			_page.Add("<tbody>");
			foreach (var others in mapbans
				.OrderByDescending(h => h.Records.Count)
				.Take(10))
			{
				_page.Add("<tr>");
				_page.Add($"<td><a href=\"{others.Map.Url}\">{others.Map.Alias}</a></td>");
				_page.Add($"<td>{others.Records.Count}</td>");
				_page.Add("</tr>");
			}
			_page.Add("</tbody>");
			_page.Add("</table>");

			_page.Add("<br><h3 align=\"center\">Most Banned Personal Records</h3>");
			_page.Add("<table align=\"center\" class=\"wrholders\">");
			_page.Add("<thead><tr>");
			_page.Add("<th>Player</th>");
			_page.Add("<th>Bans</th>");
			_page.Add("</tr></thead>");
			_page.Add("<tbody>");
			foreach (var others in all
				.OrderByDescending(h => h.BannedRecords.Count)
				.Take(20))
			{
				_page.Add("<tr>");
				_page.Add($"<td><a href=\"{others.Player.Url}\">{others.Player.Name}</a></td>");
				_page.Add($"<td>{others.BannedRecords.Count}</td>");
				_page.Add("</tr>");
			}
			_page.Add("</tbody>");
			_page.Add("</table>");

			_page.Add("<br><h3 align=\"center\">Percentage of Demo or Video Proof</h3>");
			var since = DateTime.Parse("2017-05-11");
			var requireproof = maps
				.SelectMany(m => m.Records)
				.Where(e => e.Date >= since)
				.Where(e => e.Rank.Current <= 5);
			var proof = requireproof
				.Count(e => e.DemoExists || (e as ChangelogEntry).VideoExists);
			var proofornoproof = requireproof
				.Count();
			_page.Add($"<br><h2 align=\"center\">{((double)proof / proofornoproof * 100).ToString("N2")}%</h2>");

			_page.Add("<br><h3 align=\"center\">Most Demo Uploads</h3>");
			var demos = all
				.Where(m => m.Records.Any(e => e.Entry.DemoExists))
				.OrderByDescending(m => m.Records.Count(e => e.Entry.DemoExists));
			_page.Add("<table align=\"center\" class=\"wrholders\">");
			_page.Add("<thead><tr>");
			_page.Add("<th>Player</th>");
			_page.Add("<th>Demos</th>");
			_page.Add("</tr></thead>");
			_page.Add("<tbody>");
			foreach (var demo in demos.Take(10))
			{
				_page.Add("<tr>");
				_page.Add($"<td><a href=\"{demo.Player.Url}\">{demo.Player.Name}</a></td>");
				_page.Add($"<td>{demo.Records.Count(e => e.Entry.DemoExists)}</td>");
				_page.Add("</tr>");
			}
			_page.Add("</tbody>");
			_page.Add("</table>");

			_page.Add("<br><h3 align=\"center\">Most Video Links</h3>");
			var videos = all
				.Where(m => m.Records.Any(e => (e.Entry as ChangelogEntry).VideoExists))
				.OrderByDescending(m => m.Records.Count(e => (e.Entry as ChangelogEntry).VideoExists));;
			_page.Add("<table align=\"center\" class=\"wrholders\">");
			_page.Add("<thead><tr>");
			_page.Add("<th>Player</th>");
			_page.Add("<th>Videos</th>");
			_page.Add("</tr></thead>");
			_page.Add("<tbody>");
			foreach (var video in videos.Take(10))
			{
				_page.Add("<tr>");
				_page.Add($"<td><a href=\"{video.Player.Url}\">{video.Player.Name}</a></td>");
				_page.Add($"<td>{video.Records.Count(e => (e.Entry as ChangelogEntry).VideoExists)}</td>");
				_page.Add("</tr>");
			}
			_page.Add("</tbody>");
			_page.Add("</table>");
			_page.Add("</div>");

			watch.Stop();
			// Footer
			_page.Add($"<br>Generated static page in {watch.Elapsed.TotalSeconds.ToString("N3")} seconds.");
			_page.Add("<br><a href=\"https://github.com/NeKzor/Portal2Boards.Net\">Portal2Boards.Net</a> example made by NeKz.");
			_page.Add($"<br>Last update: {DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss '(UTC)'")}");

			// Rest
			_page.Add("</body>");
			_page.Add("</html>");

			File.AppendAllLines(path, _page);
		}
	}

	internal class UserStats
	{
		public uint OfficialWorldRecords { get; set; } = 0;
		public uint TotalWorldRecords { get; set; } = 0;
		public uint OfficialDuration { get; set; } = 0;
		public uint TotalDuration { get; set; } = 0;
		public SteamUser Player { get; set; }
	}

	internal class RecordMap
	{
		public Portal2Map Map { get; set; }
		public List<IChangelogEntry> Records { get; set; }

		public IEnumerable<(int Duration, IChangelogEntry Current, IChangelogEntry Previous, Portal2Map Map)> GetDurations()
		{
			var now = DateTime.UtcNow.AddHours(-6); // CST

			var last = Records.First();
			foreach (ChangelogEntry rec in Records.Skip(1))
			{
				if (last.Score.Current == rec.Score.Current) continue;
				yield return ((int)(rec.Date - last.Date).Value.TotalDays, last, rec, Map);
				last = rec;
			}
			yield return ((int)(now.Date - last.Date).Value.TotalDays, last, null, Map);
		}

		public IEnumerable<(int Improvement, IChangelogEntry Current, IChangelogEntry Previous, Portal2Map Map)> GetImprovements()
		{
			var last = Records.First();
			foreach (ChangelogEntry rec in Records.Skip(1))
			{
				// Changelog bug???
				if (last.Score.Current <= rec.Score.Current) continue;
				yield return ((int)(last.Score.Current - rec.Score.Current), rec, last, Map);
				last = rec;
			}
		}
	}

	internal class Record
	{
		public Portal2Map Map { get; set; }
		public DateTime? Date { get; set; }
		public IChangelogEntry Entry { get; set; }
	}

	internal class RecordHolder
	{
		public SteamUser Player { get; set; }
		public List<Record> Records { get; set; }
		public List<Record> BannedRecords { get; set; }

		public List<Record> UniqueRecords
			=> Records
				.GroupBy(r => r.Map.Name)
				.Select(m => m.First())
				.ToList();
	}
}