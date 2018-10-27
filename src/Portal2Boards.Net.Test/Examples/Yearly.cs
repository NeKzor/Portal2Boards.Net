using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Portal2Boards;
using Portal2Boards.Extensions;

namespace Portal2Boards.Test.Examples.Yearly
{
    internal class YearlyMap
    {
        public Portal2Map Map { get; set; }
        public List<IChangelogEntry> Records { get; set; }

        public YearlyMap(Portal2Map map)
        {
            Map = map;
            Records = new List<IChangelogEntry>();
        }
        public void AddRecord(IChangelogEntry entry)
        {
            if (Records.Any())
            {
                if (entry.Score.Current < Records.First().Score.Current)
                {
                    Records.Clear();
                }
                else if (entry.Score.Current > Records.First().Score.Current)
                {
                    return;
                }
            }
            Records.Add(entry);
        }
    }
    internal class YearlyYear
    {
        public int Year { get; set; }
        public List<YearlyMap> Maps { get; set; }

        public YearlyYear(int year)
        {
            Year = year;
            Maps = new List<YearlyMap>();
        }
        public YearlyYear(YearlyYear history)
        {
            Year = history.Year + 1;
            Maps = new List<YearlyMap>();
        }
        public YearlyMap GetOrAddNew(Portal2Map map)
        {
            var result = Maps.FirstOrDefault(m => m.Map.BestTimeId == map.BestTimeId);
            if (result == null)
                Maps.Add(result = new YearlyMap(map));
            return result;
        }
    }
    internal class Yearly
    {
        public const uint Version = 1;

        private readonly List<string> _page;
        private readonly Portal2BoardsClient _client;

        public Yearly()
        {
            _page = new List<string>();
            _client = new Portal2BoardsClient($"Yearly/{Version}.0");
            _client.Log += Logger.LogPortal2Boards;
        }

        public Task ExportPage(string path)
        {
            if (File.Exists(path))
                File.Delete(path);
            File.AppendAllLines(path, _page);
            return Task.CompletedTask;
        }
        public async Task Build()
        {
            var watch = Stopwatch.StartNew();
            _page.Clear();
            await GenerateRecordsAsync(Portal2MapType.SinglePlayer, 2013, 2018);
            await GenerateRecordsAsync(Portal2MapType.Cooperative, 2013, 2018);
            StartPage();
            EndPage();
            _page.Insert(0, $"<!-- Generated static page in {watch.Elapsed.TotalSeconds} seconds. -->");
        }
        public async Task GenerateRecordsAsync(Portal2MapType mode, int from, int to)
        {
            var years = new List<YearlyYear>();
            var year = from;

            StartSection((mode == Portal2MapType.SinglePlayer) ? $"sp" : $"coop", (mode == Portal2MapType.SinglePlayer) ? "Single Player" : "Cooperative");
            var line = _page.Count();
            _sectionsOfSections.Clear();

            var changelog = await _client.GetChangelogAsync(q =>
            {
                q.Banned = false;
                q.MaxDaysAgo = 3333;
            });

            while (year <= to)
            {
                System.Console.WriteLine($"YEAR: {year}");

                // Local function
                Task<uint?> GetDuration(DateTime? time)
                {
                    if (time == default(DateTime?))
                        return Task.FromResult(default(uint?));

                    var duration = Math.Abs((((year == DateTime.Now.Year) ? DateTime.UtcNow : new DateTime(year + 1, 1, 1)) - time.Value.ToUniversalTime()).TotalDays);
                    return Task.FromResult((duration < 1) ? default(uint?) : (uint)duration);
                }

                StartSectionSection((mode == Portal2MapType.SinglePlayer) ? $"sp-{year}" : $"coop-{year}", $"{year}");
                StartTable(8, 2, (mode == Portal2MapType.SinglePlayer) ? "Map²" : "Map", "Time", "Player", "Date", "Duration", "Demo", "Video");

                var totalscore = 0u;
                var officialtotal = 0u;
                var wrholders = new Dictionary<string, UserStats>();
                var maps = ((mode == Portal2MapType.SinglePlayer)
                    ? Portal2.SinglePlayerMaps
                    : Portal2.CooperativeMaps)
                    .Where(m => m.Exists)
                    .Where(m => m.BestTimeId != 47817) // Propulsion Catch
                    .OrderBy(m => m.Index)
                    .ToList();

                var history = default(YearlyYear);
                history = (years.Any()) ? new YearlyYear(years.Last()) : new YearlyYear(year);
                years.Add(history);

                var yearentries = changelog.Entries
                    .Where(e => e.Date != null && e.Date.Value.Year == history.Year);

                foreach (var map in maps)
                {
                    var record = history.GetOrAddNew(map);
                    foreach (var entry in yearentries
                        .Where(e => e.MapId == map.BestTimeId))
                    {
                        record.AddRecord(entry);
                    }
                }

                foreach (var map in maps)
                {
                    var entries = history.Maps
                        .First(h => h.Map.BestTimeId == map.BestTimeId).Records;

                    if (!entries.Any())
                    {
                        System.Console.WriteLine($"Skipped: {map.Alias}");
                        _page.Add("<tr>");
                        _page.Add($"<td align=\"center\"><a class=\"link\" href=\"{map.Url}\" title=\"{map.Name}\">{map.Alias}</a>{((map.IsOfficial) ? string.Empty : "<sup>1</sup>")}</td>");
                        _page.Add($"<td></td>");
                        _page.Add($"<td></td>");
                        _page.Add($"<td></td>");
                        _page.Add($"<td></td>");
                        _page.Add($"<td></td>");
                        _page.Add($"<td></td>");
                        _page.Add("</tr>");
                        continue;
                    }

                    var latestwr = entries
                        .OrderByDescending(e => e.Date)
                        .First(e => !e.IsBanned);

                    if (map.IsOfficial)
                        officialtotal += latestwr.Score.Current ?? 0;
                    totalscore += latestwr.Score.Current ?? 0;

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
                            _page.Add($"<td rowspan=\"{wrs.Count}\" align=\"center\"><a class=\"link\" href=\"{map.Url}\" title=\"{map.Name}\">{map.Alias}</a>{((map.IsOfficial) ? string.Empty : "<sup>1</sup>")}</td>");
                            _page.Add($"<td rowspan=\"{wrs.Count}\">{wr.Score.Current.AsTimeToString() ?? "Error :("}</td>");
                            once = true;
                        }
                        _page.Add($"<td class=\"valign-wrapper\"><img class=\"circle responsive-img\" src=\"{(wr.Player as SteamUser).AvatarUrl.Replace("_full", string.Empty)}\">&nbsp;&nbsp;&nbsp;<a class=\"link\" href=\"https://board.iverb.me/profile/{(wr.Player as SteamUser).Id}\">{((wr.Player.Name.Length > 15) ? wr.Player.Name.Substring(0, 12) + "..." : wr.Player.Name)}</a></td>");
                        _page.Add($"<td title=\"{wr.Date?.DateTimeToString() + " (CST)"}\">{((wr.Date != null) ? wr.Date?.ToString("yyyy-MM-dd") : "Unknown")}</td>");
                        _page.Add($"<td title=\"{duration?.ToString() ?? "less than 1"} day{(((duration ?? 1) == 1) ? string.Empty : "s")}\">{duration?.ToString() ?? "<1"}</td>");
                        _page.Add((wr.DemoExists) ? $"<td><a title=\"Download Demo File\" class=\"link\" href=\"{(wr as ChangelogEntry).DemoUrl}\" target=\"_blank\">Download</a></td>" : "<td></td>");
                        _page.Add(((wr as ChangelogEntry).VideoExists) ? $"<td><a title=\"Watch on YouTube\" class=\"link\" href=\"{(wr as ChangelogEntry).VideoUrl}\" target=\"_blank\">Watch</a></td>" : "<td></td>");
                        _page.Add("</tr>");
                    }
                }
                if (mode == Portal2MapType.SinglePlayer)
                {
                    _page.Add("<tr>");
                    _page.Add("<td><b>Official Total</b></td>");
                    _page.Add($"<td><b>{officialtotal.AsTimeToString()}</b></td>");
                    _page.Add("</tr>");
                }
                _page.Add("<tr>");
                _page.Add("<td><b>Total</b></td>");
                _page.Add($"<td><b>{totalscore.AsTimeToString()}</b></td>");
                _page.Add("</tr>");
                EndTable();

                // Total wrs
                Title("Total Records");
                if (mode == Portal2MapType.SinglePlayer)
                    StartTable(4, 4, "Player", "Official", "Total");
                else if (mode == Portal2MapType.Cooperative)
                    StartTable(4, 4, "Player", "Total", "Percentage");

                foreach (var player in wrholders
                    .OrderByDescending(p => (mode == Portal2MapType.SinglePlayer)
                        ? p.Value.OfficialWorldRecords
                        : p.Value.TotalWorldRecords))
                {
                    _page.Add("<tr>");
                    _page.Add($"<td class=\"valign-wrapper\"><img class=\"circle responsive-img\" src=\"{player.Value.Player.AvatarUrl.Replace("_full", string.Empty)}\">&nbsp;&nbsp;&nbsp;<a class=\"link\" href=\"https://board.iverb.me/profile/{player.Value.Player.Id}\">{player.Key}</a></td>");
                    if (mode == Portal2MapType.SinglePlayer)
                        _page.Add($"<td title=\"{(int)(Math.Round((decimal)player.Value.OfficialWorldRecords / maps.Where(m => m.IsOfficial).Count(), 2) * 100)}%\">{player.Value.OfficialWorldRecords}</td>");
                    var totalpercentage = (int)(Math.Round((decimal)player.Value.TotalWorldRecords / maps.Count, 2) * 100);
                    _page.Add($"<td title=\"{totalpercentage}%\">{player.Value.TotalWorldRecords}</td>");
                    if (mode == Portal2MapType.Cooperative)
                        _page.Add($"<td>{totalpercentage}%</td>");
                    _page.Add("</tr>");
                }
                EndTable();

                // Duration
                Title("Total Duration");

                if (mode == Portal2MapType.SinglePlayer)
                    StartTable(4, 4, "Player", "Official", "Total");
                else
                    StartTable(4, 4, "Player", "Total");

                foreach (var player in wrholders
                    .OrderByDescending(p => (mode == Portal2MapType.SinglePlayer)
                        ? p.Value.OfficialDuration
                        : p.Value.TotalDuration))
                {
                    _page.Add("<tr>");
                    _page.Add($"<td class=\"valign-wrapper\"><img class=\"circle responsive-img\" src=\"{player.Value.Player.AvatarUrl.Replace("_full", string.Empty)}\">&nbsp;&nbsp;&nbsp;<a class=\"link\" href=\"https://board.iverb.me/profile/{player.Value.Player.Id}\">{player.Key}</a></td>");
                    if (mode == Portal2MapType.SinglePlayer)
                        _page.Add($"<td title=\"{player.Value.OfficialDuration} days\">{player.Value.OfficialDuration}</td>");
                    _page.Add($"<td title=\"{player.Value.TotalDuration} days\">{player.Value.TotalDuration}</td>");
                    _page.Add("</tr>");
                }
                EndTable();

                // Footer
                _page.Add("<br><br><br>");
                _page.Add("<div clas=\"row\" align=\"center\">");
                if (mode == Portal2MapType.SinglePlayer)
                {
                    _page.Add("<br><br><sup>1</sup> Unofficial challenge mode map.");
                    _page.Add("<br><sup>2</sup> Excluding Propulsion Catch.");
                }
                _page.Add("</div>");

                EndSection();
                year++;
            }
            EndSection();

            // Insert tab-tabs
            _page.Insert(line,
$@"         <ul class=""tabs tabs-transparent"">
                {string.Join("\n", _sectionsOfSections)}
			</ul>");
        }

        // HTML stuff
        private void StartPage()
        {
            _page.Insert(0,
$@"<!-- src/Portal2Boards.Net.Test/Examples/Yearly.cs -->
<!-- v{Version}.0 -->
<!DOCTYPE html>
<html>
	<head>
		<title>Yearly | nekzor.github.io</title>
		<link href=""https://fonts.googleapis.com/css?family=Roboto"" rel=""stylesheet"">
		<link href=""https://fonts.googleapis.com/icon?family=Material+Icons"" rel=""stylesheet"">
		<link href=""https://cdnjs.cloudflare.com/ajax/libs/materialize/1.0.0-alpha.4/css/materialize.min.css"" rel=""stylesheet"">
		<meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
		<style>.link {{ color: white }} .link:hover {{ color: black }}</style>
	</head>
	<body class=""white-text blue-grey darken-4"">
		<nav class=""nav-extended blue-grey darken-3"">
			<div class=""nav-wrapper"">
                <div class=""col s12 hide-on-small-only"">
                    <a href=""#"" data-target=""slide-out"" class=""sidenav-trigger show-on-large""><i class=""material-icons"">menu</i></a>
                    <a href=""index.html"">&nbsp;&nbsp;&nbsp;nekzor.github.io</a>
                    <a class=""breadcrumb""></a>
                    <a href=""yearly.html"">Yearly</a>
                </div>
                <div class=""col s12 hide-on-med-and-up"">
                    <a href=""#"" data-target=""slide-out"" class=""sidenav-trigger""><i class=""material-icons"">menu</i></a>
                    <a href=""yearly.html"" class=""brand-logo center"">Yearly</a>
                </div>
            </div>
			<div class=""nav-content"">
				<ul class=""tabs tabs-transparent"">
                    {string.Join("\n", _sections)}
					<li class=""tab""><a href=""#about"">About</a></li>
				</ul>
			</div>
		</nav>
		<ul id=""slide-out"" class=""sidenav"">
            <li><a href=""index.html"">nekzor.github.io</a></li>
            <li><a href=""stats.html"">Statistics</a></li>
            <li><a href=""history.html"">History</a></li>
            <li><a href=""skill.html"">Skill Points</a></li>
            <li><a href=""seum.html"">SEUM</a></li>
            <li><a href=""yearly.html"">Yearly</a></li>
        </ul>");
        }
        private void EndPage()
        {
            _page.Add(
$@"		<div id=""about"">
			<div class=""row""></div>
			<div class=""row"">
				<div class=""col s12 m12 l8 push-l2"">
					<h3>board.iverb.me Yearly</h3>
                    <p>
                        Fastest time every year since 2013, when it was board.ncla.me back then.
                    </p>
                    <br>
                    <h6>Made with <a class=""link"" href=""https://github.com/NeKzor/Portal2Boards.Net"">Portal2Boards.Net</a></h6>
                    <br>
					<h6>Last Update: {DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss '(UTC)'")}</h6>
				</div>
			</div>
		</div>
		<script src=""https://code.jquery.com/jquery-3.3.1.min.js"" integrity=""sha256-FgpCb/KJQlLNfOu91ta32o/NMZxltwRo8QtmkMRdAu8="" crossorigin=""anonymous""></script>
		<script src=""https://cdnjs.cloudflare.com/ajax/libs/materialize/1.0.0-alpha.4/js/materialize.min.js""></script>
		<script>
			$(document).ready(function() {{
				$('.tabs').tabs();
				$('.sidenav').sidenav();
			}});
		</script>
	</body>
</html>");
        }
        private List<string> _sections = new List<string>();
        void StartSection(string id, string title)
        {
            _sections.Add(
$@"                 <li class=""tab""><a href=""#{id}"">{title}</a></li>");
            _page.Add(
$@"     <div id=""{id}"">");
        }
        private List<string> _sectionsOfSections = new List<string>();
        void StartSectionSection(string id, string title)
        {
            _sectionsOfSections.Add(
$@"                 <li class=""tab""><a href=""#{id}"">{title}</a></li>");
            _page.Add(
$@"     <div id=""{id}"">");
        }
        void EndSection()
        {
            _page.Add(
@"      </div>");
        }
        void Title(string text, bool br = true)
        {
            if (br) _page.Add("<br><br><br>");
            _page.Add($"<div class=\"row\"><h3 align=\"center\">{text}</h3></div>");
        }
        private void StartTable(int colLength, int pushLength, params string[] items)
        {
            _page.Add(
$@"		<div class=""row"">
			<div class=""col s12 m12 l{colLength} push-l{pushLength}"">
				<table>
					<thead>
						<tr>");
            foreach (var item in items)
            {
                if (item.Contains("/"))
                {
                    var split = item.Split('/');
                    _page.Add(
$@"							<th title=""{split[1]}"">{split[0]}</th>");
                }
                else
                {
                    _page.Add(
    $@"							<th>{item}</th>");
                }
            }
            _page.Add(
@"						</tr>
					</thead>
					<tbody>");
        }
        private void EndTable()
        {
            _page.Add(
@"					</tbody>
				</table>
			</div>
		</div>");
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

        public IEnumerable<(int Duration, IChangelogEntry Current, IChangelogEntry Previous, Portal2Map Map)>
            GetDurations(int year)
        {
            var now = (year == DateTime.Now.Year) ? DateTime.UtcNow : new DateTime(year + 1, 1, 1);
            now = now.AddHours(-6); // CST

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
