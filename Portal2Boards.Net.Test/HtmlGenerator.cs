using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Portal2Boards.Net.Entities;
using Portal2Boards.Net.Extensions;

namespace Portal2Boards.Net.Test
{
	// Example
	internal static class HtmlGenerator
	{
		private static readonly List<string> _page = new List<string>();
		// It's recommended to use a static client if you use it multiple times
		private static readonly Portal2BoardsClient _client = new Portal2BoardsClient();
		private const string _path = @"coop.html";

		public static async Task GeneratePages()
		{
#if RELEASE
			if (File.Exists(_path))
				File.Delete(_path);
#endif
			_page.Clear();

			var watch = Stopwatch.StartNew();
			// Head
			_page.Add("<!DOCTYPE html>");
			_page.Add("<html>");
			_page.Add("<head>");
			_page.Add("<title>Portal2Records</title>");
			_page.Add("<link href=\"https://fonts.googleapis.com/css?family=Roboto\" rel=\"stylesheet\">");
			_page.Add("<style>table,td,th{border-collapse:collapse;border:1px solid #ddd;text-align: left;}table.wrs{width:50%;}table.wrholders{width:20%;}th,td{padding: 1px;}</style>");
			_page.Add("</head>");

			// Body
			_page.Add("<body style=\"font-family: 'Roboto', sans-serif; \">");
			_page.Add("<div>");
			_page.Add("<h2 align=\"center\">Portal 2 Challenge Mode World Records</h2>");

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
			//var totalscorewithnoncm = 0u;
			var wrholders = new Dictionary<string, UserStats>();
			var maps = Portal2.CooperativeMaps
				//.Where(m => m.BestTimeId != null)
				.Where(m => m.IsOfficial)
				.OrderBy(m => m.Index)
				.ToList();
			foreach (var map in maps)
			{
				var changelog = await _client.GetChangelogAsync($"?wr=1&chamber={map.BestTimeId}");
				var latestwr = changelog.First(e => !e.IsBanned);
				if (map.IsOfficial)
					totalscore += latestwr.Score.Current ?? 0;
				//totalscorewithnoncm += latestwr.Score.Current ?? 0;

				var wrs = changelog
					.Where(e => e.Score.Current == latestwr.Score.Current)
					.ToList();
				var once = false;
				foreach (var wr in wrs)
				{
					var duration = await GetDuration(wr.Date);
					if (!wrholders.Keys.Contains(wr.Player.Name))
						wrholders.Add(wr.Player.Name, new UserStats());

					wrholders[wr.Player.Name] = new UserStats
					{
						//OfficialDuration = wrholders[wr.Player.Name].OfficialDuration + ((map.IsOfficial) ? duration ?? 0 : 0),
						//OfficialWorldRecords = wrholders[wr.Player.Name].OfficialWorldRecords + ((map.IsOfficial) ? 1u : 0),
						TotalDuration = wrholders[wr.Player.Name].TotalDuration + duration ?? 0,
						TotalWorldRecords = wrholders[wr.Player.Name].TotalWorldRecords + 1,
						Player = wr.Player
					};

					_page.Add("<tr>");
					if (!once)
					{
						_page.Add($"<td rowspan=\"{wrs.Count}\" align=\"center\"><a href=\"{map.Link}\" title=\"{map.Name}\">{map.Alias}</a>{((map.IsOfficial) ? string.Empty : "*")}</td>");
						_page.Add($"<td rowspan=\"{wrs.Count}\">{wr.Score.Current.AsTimeToString() ?? "Error :("}</td>");
						once = true;
					}
					_page.Add($"<td><a href=\"{wr.Player.Link}\">{wr.Player.Name}</a></td>");
					_page.Add($"<td>{wr.Date?.ToString("yyyy-MM-dd") ?? "Unknown"}</td>");
					_page.Add($"<td>{duration?.ToString() ?? "<1"}</td>");
					_page.Add((wr.DemoExists) ? $"<td><a href=\"{wr.DemoLink}\">Download</a></td>" : "<td></td>");
					_page.Add((string.IsNullOrEmpty(wr.YouTubeId)) ? "<td></td>" : $"<td><a href=\"{wr.VideoLink}\">Watch</a></td>");
					_page.Add("</tr>");
				}
			}
			_page.Add("<tr>");
			_page.Add("<td><b>Total</b></td>");
			_page.Add($"<td><b>{totalscore.AsTimeToString()}</b></td>");
			_page.Add("</tr>");
			_page.Add("</tbody>");
			_page.Add("</table>");
			_page.Add("</div>");

			// Second table
			_page.Add("<div>");
			_page.Add("<br><h3 align=\"center\">World Record Holders</h3>");
			_page.Add("<table align=\"center\" class=\"wrholders\">");
			_page.Add("<thead><tr>");
			_page.Add("<th>Player</th>");
			_page.Add("<th>Records</th>");
			_page.Add("<th>Percentage</th>");
			_page.Add("</tr></thead>");
			_page.Add("<tbody>");
			foreach (var player in wrholders.OrderByDescending(p => p.Value.TotalWorldRecords))
			{
				_page.Add("<tr>");
				_page.Add($"<td><a href=\"{player.Value.Player.Link}\">{player.Key}</a></td>");
				_page.Add($"<td>{player.Value.TotalWorldRecords}</td>");
				_page.Add($"<td>{(int)(Math.Round((decimal)player.Value.TotalWorldRecords / maps.Count, 2) * 100)}%</td>");
				_page.Add("</tr>");
			}
			_page.Add("</tbody>");
			_page.Add("</table>");
			_page.Add("</div>");

			// Third table
			_page.Add("<div>");
			_page.Add("<br><h3 align=\"center\">Duration</h3>");
			_page.Add("<table align=\"center\" class=\"wrholders\">");
			_page.Add("<thead><tr>");
			_page.Add("<th>Player</th>");
			_page.Add("<th>Total</th>");
			_page.Add("</tr></thead>");
			_page.Add("<tbody>");
			foreach (var player in wrholders.OrderByDescending(p => p.Value.TotalDuration))
			{
				_page.Add("<tr>");
				_page.Add($"<td><a href=\"{player.Value.Player.Link}\">{player.Key}</a></td>");
				_page.Add($"<td>{player.Value.TotalDuration}</td>");
				_page.Add("</tr>");
			}
			_page.Add("</tbody>");
			_page.Add("</table>");
			_page.Add("</div>");

			watch.Stop();
			// Footer
			_page.Add($"<br>Generated page in {watch.Elapsed.TotalSeconds.ToString("N3")} seconds.");
			_page.Add("<br><a href=\"https://github.com/NeKzor/Portal2Boards.Net\">Portal2Boards.Net</a> example made by NeKz.");
			_page.Add($"<br>{DateTime.UtcNow.ToString("yyyy-MM-dd hh:mm:ss")}");

			// Rest
			_page.Add("</body>");
			_page.Add("</html>");
#if RELEASE
			File.AppendAllLines(_path, _page);
#endif
		}

		private static Task<uint?> GetDuration(DateTime? time)
		{
			if (time == default(DateTime?))
				return Task.FromResult(default(uint?));

			var duration = Math.Abs((DateTime.UtcNow - time.Value.ToUniversalTime()).TotalDays);
			return Task.FromResult((duration < 1) ? default(uint?) : (uint)duration);
		}
    }

	internal class UserStats
	{
		//public uint OfficialWorldRecords { get; set; } = 0;
		public uint TotalWorldRecords { get; set; } = 0;
		//public uint OfficialDuration { get; set; } = 0;
		public uint TotalDuration { get; set; } = 0;
		public User Player { get; set; }
	}
}