#define RELEASE
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
	internal static class HtmlGenerator
	{
		private static readonly List<string> _page = new List<string>();
		private static readonly Portal2BoardsClient _client = new Portal2BoardsClient();

		public static async Task GeneratePage(string path, MapType mode)
		{
#if RELEASE
			if (File.Exists(path))
				File.Delete(path);
#endif
			_page.Clear();

			var watch = Stopwatch.StartNew();
			// Head
			_page.Add("<!DOCTYPE html>");
			_page.Add("<html>");
			_page.Add("<head>");
			_page.Add("<title>Portal2Records</title>");
			_page.Add("<link href=\"https://fonts.googleapis.com/css?family=Roboto\" rel=\"stylesheet\">");
			_page.Add("<style>table,td,th{border-collapse:collapse;border:1px solid #ddd;text-align: left;}table.wrs{width:50%;}table.wrholders{width:20%;}th,td{padding: 1px;}a{color:inherit;text-decoration:none;}a:hover{color:#FF8C00;}</style>");
			_page.Add("</head>");

			// Body
			_page.Add("<body style=\"font-family:'Roboto',sans-serif;color:rgba(200,200,200,1);background-color:rgba(0,0,0,0.9);\">");
			_page.Add("<div>");
			_page.Add("<h2 align=\"center\">Portal 2 Challenge Mode World Records</h2>");
			_page.Add($"<h4 align=\"center\">{mode.ToString().ToTitle()}</h4>");

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
			var maps = ((mode == MapType.SinglePlayer)
				? Portal2.SinglePlayerMaps
					.Where(m => m.BestTimeId != null)
				: Portal2.CooperativeMaps
					.Where(m => m.IsOfficial))
				.OrderBy(m => m.Index)
				.ToList();
			foreach (var map in maps)
			{
				var changelog = await _client.GetChangelogAsync($"?wr=1&chamber={map.BestTimeId}");
				var latestwr = changelog.First(e => !e.IsBanned);
				if (map.IsOfficial)
					totalscore += latestwr.Score.Current ?? 0;
				if (mode == MapType.SinglePlayer)
					totalscorewithnoncm += latestwr.Score.Current ?? 0;

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
						OfficialDuration = wrholders[wr.Player.Name].OfficialDuration + ((map.IsOfficial) ? duration ?? 0 : 0),
						OfficialWorldRecords = wrholders[wr.Player.Name].OfficialWorldRecords + ((map.IsOfficial) ? 1u : 0),
						TotalDuration = wrholders[wr.Player.Name].TotalDuration + duration ?? 0,
						TotalWorldRecords = wrholders[wr.Player.Name].TotalWorldRecords + 1,
						Player = wr.Player
					};

					_page.Add("<tr>");
					if (!once)
					{
						_page.Add($"<td rowspan=\"{wrs.Count}\" align=\"center\"><a href=\"{map.Link}\" title=\"{map.Name}\">{map.Alias}</a>{((map.IsOfficial) ? string.Empty : "<sup>1</sup>")}</td>");
						_page.Add($"<td rowspan=\"{wrs.Count}\">{wr.Score.Current.AsTimeToString() ?? "Error :("}</td>");
						once = true;
					}
					_page.Add($"<td><a href=\"{wr.Player.Link}\">{wr.Player.Name}</a></td>");
					_page.Add($"<td title=\"{wr.Date?.DateTimeToString()}\">{wr.Date?.ToString("yyyy-MM-dd") ?? "Unknown"}</td>");
					_page.Add($"<td>{duration?.ToString() ?? "<1"}</td>");
					_page.Add((wr.DemoExists) ? $"<td><a href=\"{wr.DemoLink}\">Download</a></td>" : "<td></td>");
					_page.Add((wr.VideoExists) ? $"<td><a href=\"{wr.VideoLink}\">Watch</a></td>" : "<td></td>");
					_page.Add("</tr>");
				}
			}
			if (mode == MapType.SinglePlayer)
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

			// Second table
			_page.Add("<div>");
			_page.Add($"<br><h3 align=\"center\">{((mode == MapType.SinglePlayer) ? "Official " : string.Empty)}World Record Holders</h3>");
			_page.Add("<table align=\"center\" class=\"wrholders\">");
			_page.Add("<thead><tr>");
			_page.Add("<th>Player</th>");
			if (mode == MapType.SinglePlayer)
				_page.Add("<th>Official</th>");
			_page.Add("<th>Total</th>");
			if (mode == MapType.Cooperative)
				_page.Add("<th>Percentage</th>");
			_page.Add("</tr></thead>");
			_page.Add("<tbody>");
			foreach (var player in wrholders.OrderByDescending(p => (mode == MapType.SinglePlayer) ? p.Value.OfficialWorldRecords : p.Value.TotalWorldRecords))
			{
				_page.Add("<tr>");
				_page.Add($"<td><a href=\"{player.Value.Player.Link}\">{player.Key}</a></td>");
				if (mode == MapType.SinglePlayer)
					_page.Add($"<td title=\"{(int)(Math.Round((decimal)player.Value.OfficialWorldRecords / maps.Count, 2) * 100)}%\">{player.Value.OfficialWorldRecords}</td>");
				var totalpercentage = (int)(Math.Round((decimal)player.Value.TotalWorldRecords / maps.Count, 2) * 100);
				_page.Add($"<td title=\"{totalpercentage}%\">{player.Value.TotalWorldRecords}</td>");
				if (mode == MapType.Cooperative)
					_page.Add($"<td>{totalpercentage}%</td>");
				_page.Add("</tr>");
			}
			_page.Add("</tbody>");
			_page.Add("</table>");
			_page.Add("</div>");

			// Third table
			_page.Add("<div>");
			_page.Add($"<br><h3 align=\"center\">{((mode == MapType.SinglePlayer) ? "Official " : string.Empty)}Duration</h3>");
			_page.Add("<table align=\"center\" class=\"wrholders\">");
			_page.Add("<thead><tr>");
			_page.Add("<th>Player</th>");
			if (mode == MapType.SinglePlayer)
				_page.Add("<th>Official</th>");
			_page.Add("<th>Total</th>");
			_page.Add("</tr></thead>");
			_page.Add("<tbody>");
			foreach (var player in wrholders.OrderByDescending(p => (mode == MapType.SinglePlayer) ? p.Value.OfficialDuration : p.Value.TotalDuration))
			{
				_page.Add("<tr>");
				_page.Add($"<td><a href=\"{player.Value.Player.Link}\">{player.Key}</a></td>");
				if (mode == MapType.SinglePlayer)
					_page.Add($"<td>{player.Value.OfficialDuration}</td>");
				_page.Add($"<td>{player.Value.TotalDuration}</td>");
				_page.Add("</tr>");
			}
			_page.Add("</tbody>");
			_page.Add("</table>");
			_page.Add("</div>");

			watch.Stop();
			// Footer
			if (mode == MapType.SinglePlayer)
				_page.Add("<br><sup>1</sup> Unofficial challenge mode map.");
			_page.Add($"<br>Generated page in {watch.Elapsed.TotalSeconds.ToString("N3")} seconds.");
			_page.Add("<br><a href=\"https://github.com/NeKzor/Portal2Boards.Net\">Portal2Boards.Net</a> example made by NeKz.");
			_page.Add($"<br>{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}");

			// Rest
			_page.Add("</body>");
			_page.Add("</html>");
#if RELEASE
			File.AppendAllLines(path, _page);
#endif
		}

		private static Task<uint?> GetDuration(DateTime? time)
		{
			if (time == default(DateTime?))
				return Task.FromResult(default(uint?));

			var duration = Math.Abs((DateTime.UtcNow - time.Value.ToUniversalTime()).TotalDays);
			return Task.FromResult((duration < 1) ? default(uint?) : (uint)duration);
		}

		private static string ToTitle(this string str)
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

	internal class UserStats
	{
		public uint OfficialWorldRecords { get; set; } = 0;
		public uint TotalWorldRecords { get; set; } = 0;
		public uint OfficialDuration { get; set; } = 0;
		public uint TotalDuration { get; set; } = 0;
		public User Player { get; set; }
	}
}