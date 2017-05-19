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
		private const string _path = @"index.html";

		public static async Task GeneratePages()
		{
			if (File.Exists(_path))
				File.Delete(_path);
			_page.Clear();

			var watch = Stopwatch.StartNew();
			// Head
			_page.Add("<!DOCTYPE html>");
			_page.Add("<html>");
			_page.Add("<head>");
			_page.Add("<title>Portal2Records</title>");
			_page.Add("<link href=\"https://fonts.googleapis.com/css?family=Roboto\" rel=\"stylesheet\">");
			_page.Add("<style>table,td,th{border-collapse:collapse;border:1px solid #ddd;text-align: left;}table.wrs{width:50%;}table.wrholders{width:10%;}th,td{padding: 1px;}</style>");
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
			var wrholders = new Dictionary<string, Tuple<User, uint>>();
			foreach (var map in Portal2.SinglePlayerMaps.Where(m => m.IsOfficial).OrderBy(m => m.Index))
			{
				var changelog = await _client.GetChangelogAsync($"?wr=1&chamber={map.BestTimeId}");
				var latestwr = changelog.First();
				totalscore += latestwr.Score.Current ?? 0;

				var wrs = changelog.Where(e => e.Score.Current == latestwr.Score.Current).ToList();
				var once = false;
				foreach (var wr in wrs)
				{
					if (!wrholders.Keys.Contains(wr.Player.Name))
						wrholders.Add(wr.Player.Name, new Tuple<User, uint>(wr.Player, 1));
					else
						wrholders[wr.Player.Name] = new Tuple<User, uint>(wr.Player, wrholders[wr.Player.Name].Item2 + 1);

					_page.Add("<tr>");
					if (!once)
					{
						_page.Add($"<td rowspan=\"{wrs.Count}\" align=\"center\"><a href=\"https://board.iverb.me/chamber/{map.BestTimeId}\" title=\"{map.Name}\">{map.Alias}</a></td>");
						_page.Add($"<td rowspan=\"{wrs.Count}\">{await FormatTime(wr.Score.Current) ?? "Error :("}</td>");
						once = true;
					}
					_page.Add($"<td><a href=\"https://board.iverb.me/profile/{wr.Player.SteamId}\">{wr.Player.Name}</a></td>");
					_page.Add($"<td>{wr.Date?.ToString("yyyy-MM-dd") ?? "Unknown"}</td>");
					var duration = await GetDuration(wr.Date);
					_page.Add($"<td>{((string.IsNullOrEmpty(duration)) ? string.Empty : duration)}</td>");
					_page.Add((wr.DemoExists) ? $"<td><a href=\"{wr.DemoLink}\">Download</a></td>" : "<td></td>");
					_page.Add((string.IsNullOrEmpty(wr.YouTubeId)) ? "<td></td>" : $"<td><a href=\"https://youtu.be/{wr.YouTubeId}\">Watch</a></td>");
					_page.Add("</tr>");
				}
			}
			_page.Add("<tr>");
			_page.Add("<td><b>Total</b></td>");
			_page.Add($"<td><b>{await FormatTime(totalscore)}</b></td>");
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
			_page.Add("<th>WRs</th>");
			_page.Add("</tr></thead>");
			_page.Add("<tbody>");
			foreach (var player in wrholders.OrderByDescending(p => p.Value.Item2))
			{
				_page.Add("<tr>");
				_page.Add($"<td><a href=\"https://board.iverb.me/profile/{player.Value.Item1.SteamId}\">{player.Key}</a></td>");
				_page.Add($"<td>{player.Value.Item2}</td>");
				_page.Add("</tr>");
			}
			_page.Add("</tbody>");
			_page.Add("</table>");
			_page.Add("</div>");

			watch.Stop();
			// Footer
			_page.Add($"<br>Generated page in {watch.Elapsed.TotalSeconds.ToString("N3")} seconds.");
			_page.Add("<br><a href=\"https://github.com/NeKzor/Portal2Boards.Net\">Portal2Boards.Net</a> example made by NeKz.");

			// Rest
			_page.Add("</body>");
			_page.Add("</html>");
			File.AppendAllLines(_path, _page);
		}

		private static Task<string> GetDuration(DateTime? time)
		{
			if (time == default(DateTime?))
				return Task.FromResult(default(string));

			var duration = (DateTime.UtcNow - time.Value.ToUniversalTime()).TotalDays;
			return Task.FromResult((duration < 1) ? "< 1" : ((int)duration).ToString());
		}

		private static Task<string> FormatTime(uint? time)
		{
			if (time == default(uint?))
				return Task.FromResult("Error :(");

			var centi = time % 100;
			var totalsec = Math.Floor((decimal)time / 100);
			var sec = totalsec % 60;
			var min = Math.Floor(totalsec / 60);
			if (min > 0)
				return Task.FromResult($"{min}:{((sec < 10) ? $"0{sec}" : sec.ToString())}.{((centi < 10) ? $"0{centi}" : centi.ToString())}");
			else
				return Task.FromResult($"{sec}.{((centi < 10) ? $"0{centi}" : centi.ToString())}");
		}
    }
}