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
    internal class Leaderboard
    {
        public const uint Version = 4;

        private readonly List<string> _page;
        private readonly Portal2BoardsClient _client;

        public Leaderboard()
        {
            _page = new List<string>();
            _client = new Portal2BoardsClient($"Leaderboard/{Version}.0");
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
            await GenerateRecordsAsync(Portal2MapType.SinglePlayer);
            await GenerateRecordsAsync(Portal2MapType.Cooperative);
            await GenerateStatsPageAsync();
            await GenerateCommunityStatsPageAsync();
            StartPage();
            EndPage();
            _page.Insert(0, $"<!-- Generated static page in {watch.Elapsed.TotalSeconds} seconds. -->");
        }
        public async Task GenerateRecordsAsync(Portal2MapType mode)
        {
            // Local function
            Task<uint?> GetDuration(DateTime? time)
            {
                if (time == default(DateTime?))
                    return Task.FromResult(default(uint?));

                var duration = Math.Abs((DateTime.UtcNow - time.Value.ToUniversalTime()).TotalDays);
                return Task.FromResult((duration < 1) ? default(uint?) : (uint)duration);
            }

            StartSection((mode == Portal2MapType.SinglePlayer) ? "sp" : "coop", (mode == Portal2MapType.SinglePlayer) ? "Single Player" : "Cooperative");
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
                    _page.Add($"<td class=\"valign-wrapper\"><img class=\"circle responsive-img\" src=\"{(wr.Player as SteamUser).AvatarUrl.Replace("_full", string.Empty)}\">&nbsp;&nbsp;&nbsp;<a class=\"link\" href=\"https://board.iverb.me/profile/{(wr.Player as SteamUser).Id}\">{wr.Player.Name}</a></td>");
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
        }
        public async Task GenerateStatsPageAsync()
        {
            StartSection("stats", "Statistics");

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
            Title("Most Records");
            StartTable(6, 3, "Player", "Total", "2013", "2014", "2015", "2016", "2017", "2018");
            foreach (var wr in wrh
                .OrderByDescending(h => h.Records.Count)
                .ThenBy(r => r.Records.First().Date))
            {
                _page.Add("<tr>");
                _page.Add($"<td><a class=\"link\" href=\"https://board.iverb.me/profile/{wr.Player.Id}\">{wr.Player.Name}</a></td>");
                _page.Add($"<td title=\"{wr.Records.Count(r => r.Map.IsOfficial)} Official\">{wr.Records.Count}</td>");
                for (int year = 2013; year < 2019; year++)
                {
                    var total = wr.Records.Where(r => r.Date.Value.Year == year);
                    var official = total.Where(r => r.Map.IsOfficial);
                    _page.Add($"<td title=\"{official.Count()} Official\">{total.Count()}</td>");
                }
                _page.Add("</tr>");
            }
            EndTable();

            // Unique World Records of Players
            Title("Unique Records");
            StartTable(4, 4, "Player", "Total", "Sp:Coop");
            foreach (var wr in wrh.OrderByDescending(h => h.UniqueRecords.Count))
            {
                var unique = wr.UniqueRecords;
                var sp = unique.Count(r => r.Map.Type == Portal2MapType.SinglePlayer);
                var mp = unique.Count(r => r.Map.Type == Portal2MapType.Cooperative);
                var osp = unique.Count(r => r.Map.Type == Portal2MapType.SinglePlayer && r.Map.IsOfficial);
                var omp = unique.Count(r => r.Map.Type == Portal2MapType.Cooperative && r.Map.IsOfficial);

                _page.Add("<tr>");
                _page.Add($"<td><a class=\"link\" href=\"https://board.iverb.me/profile/{wr.Player.Id}\">{wr.Player.Name}</a></td>");
                _page.Add($"<td title=\"{unique.Count(r => r.Map.IsOfficial)} Official\">{unique.Count}</td>");
                _page.Add($"<td title=\"{osp}:{omp} Official\">{sp}:{mp}</td>");
                _page.Add("</tr>");
            }
            EndTable();

            // Activity
            Title("Activity");
            StartTable(6, 3, "Year/Month", "Total", "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec");
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
            EndTable();

            // Most World Records per Year
            Title("Most Records per Year");
            StartTable(4, 4, "Year", "Player", "Total");
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
                    _page.Add($"<td><a class=\"link\" href=\"https://board.iverb.me/profile/{player.Player.Id}\">{player.Player.Name}</a></td>");
                    _page.Add($"<td title=\"{off} Official\">{recs}</td>");
                    _page.Add("</tr>");
                }
            }
            EndTable();

            // Longest Lasting World Records
            Title("Longest Lasting Records");
            StartTable(6, 3, "Map", "Time", "Player", "Duration/in days", "Start", "End");
            foreach (var stats in durations
                .OrderByDescending(d => d.Duration)
                .Take(20))
            {
                var (duration, current, previous, map) = stats;
                _page.Add("<tr>");
                _page.Add($"<td><a class=\"link\" href=\"{map.Url}\">{map.Alias}</a></td>");
                _page.Add($"<td>{current.Score.Current.AsTimeToString()}</td>");
                _page.Add($"<td><a class=\"link\" href=\"https://board.iverb.me/profile/{(current.Player as SteamUser).Id}\">{current.Player.Name}</a></td>");
                _page.Add($"<td>{((duration < 1) ? "<1" : $"{duration}")}</td>");
                _page.Add($"<td title=\"{current.Date.DateTimeToString() + " (CST)"}\">{(current.Date?.ToString("yyy-MM-dd"))}</td>");
                if (previous != null)
                    _page.Add($"<td title=\"{previous.Date.DateTimeToString() + " (CST)"}\">{(previous.Date?.ToString("yyy-MM-dd"))}</td>");
                else
                    _page.Add("<td>Ongoing</td>");
                _page.Add("</tr>");
            }
            EndTable();

            // Longest Lasting World Records History
            Title("Longest Lasting Records History");
            StartTable(6, 3, "Map", "Time", "Player", "Duration/in days", "Start", "End");
            var highest = default((int Duration, IChangelogEntry Current, IChangelogEntry Previous, Portal2Map Map));
            foreach (var stats in durations
                .OrderBy(d => d.Current.Date))
            {
                if (stats.Duration > highest.Duration)
                {
                    var (duration, current, previous, map) = stats;
                    _page.Add("<tr>");
                    _page.Add($"<td><a class=\"link\" href=\"{map.Url}\">{map.Alias}</a></td>");
                    _page.Add($"<td>{current.Score.Current.AsTimeToString()}</td>");
                    _page.Add($"<td><a class=\"link\" href=\"https://board.iverb.me/profile/{(current.Player as SteamUser).Id}\">{current.Player.Name}</a></td>");
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
            EndTable();

            // Total Time of World Records
            Title("Total Time of Records");
            StartTable(4, 4, "Year", "Single Player", "Cooperative");
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
            EndTable();

            // Most World Record Improvements
            Title("Most Record Improvements");
            StartTable(4, 4, "Map", "Records");
            foreach (var improvement in wri
                .OrderByDescending(i => i.Improvements)
                .Take(20))
            {
                _page.Add("<tr>");
                _page.Add($"<td><a class=\"link\" href=\"{improvement.Map.Url}\">{improvement.Map.Alias}</a></td>");
                _page.Add($"<td>{improvement.Improvements}</td>");
                _page.Add("</tr>");
            }
            EndTable();

            // Largest World Record Improvement
            Title("Largest Record Improvement");
            StartTable(6, 3, "Map", "Time", "Player", "Improvement", "Date Set");
            foreach (var stats in improvements
                .OrderByDescending(d => d.Improvement)
                .ThenBy(d => d.Current.Date)
                .Take(20))
            {
                var (improvement, current, previous, map) = stats;
                _page.Add("<tr>");
                _page.Add($"<td><a class=\"link\" href=\"{map.Url}\">{map.Alias}</a></td>");
                _page.Add($"<td>{current.Score.Current.AsTimeToString()}</td>");
                _page.Add($"<td><a class=\"link\" href=\"https://board.iverb.me/profile/{(current.Player as SteamUser).Id}\">{current.Player.Name}</a></td>");
                _page.Add($"<td>{((uint)improvement).AsTimeToString()}</td>");
                _page.Add($"<td title=\"{current.Date.DateTimeToString() + " (CST)"}\">{(current.Date?.ToString("yyy-MM-dd"))}</td>");
                _page.Add("</tr>");
            }
            EndTable();

            // Others
            var longest = maps
                .SelectMany(m => m.Records)
                .Where(m => !string.IsNullOrEmpty(m.Comment))
                .OrderByDescending(c => c.Comment.Length)
                .ThenBy(c => c.Date);
            var shortest = maps
                .SelectMany(m => m.Records)
                .Where(m => !string.IsNullOrEmpty(m.Comment))
                .OrderBy(c => c.Comment.Length)
                .ThenBy(c => c.Date);

            Title("Longest Comments");
            StartTable(8, 2, "Player", "Length", "Comment");
            foreach (var others in longest.Take(5))
            {
                _page.Add("<tr>");
                _page.Add($"<td><a class=\"link\" href=\"https://board.iverb.me/profile/{(others.Player as SteamUser).Id}\">{others.Player.Name}</a></td>");
                _page.Add($"<td>{others.Comment.Length}</td>");
                _page.Add($"<td>{others.Comment}</td>");
                _page.Add("</tr>");
            }
            EndTable();

            Title("Shortest Comments");
            StartTable(4, 4, "Player", "Length", "Comment");
            foreach (var others in shortest.Take(5))
            {
                _page.Add("<tr>");
                _page.Add($"<td><a class=\"link\" href=\"https://board.iverb.me/profile/{(others.Player as SteamUser).Id}\">{others.Player.Name}</a></td>");
                _page.Add($"<td>{others.Comment.Length}</td>");
                _page.Add($"<td>{others.Comment}</td>");
                _page.Add("</tr>");
            }
            EndTable();

            Title("Average Comment Length");
            var avgcomment = maps
                .SelectMany(m => m.Records)
                .Where(e => !string.IsNullOrEmpty(e.Comment))
                .Average(e => e.Comment.Length);
            _page.Add($"<br><h2 align=\"center\">{avgcomment.ToString("N2")}</h2>");

            Title("Percentage of Proof");
            var since = DateTime.Parse("2017-05-11");
            var requireproof = maps
                .SelectMany(m => m.Records)
                .Where(e => e.Date >= since)
                .Where(e => e.Rank.Current <= 5);
            var proof = requireproof
                .Count(e => e.DemoExists || (e as ChangelogEntry).VideoExists);
            var proofornoproof = requireproof
                .Count();
            _page.Add($"<br><h2 title=\"since 2017-05-11\" align=\"center\">{((double)proof / proofornoproof * 100).ToString("N2")}%</h2>");

            EndSection();
        }
        public async Task GenerateCommunityStatsPageAsync()
        {
            StartSection("community", "Community");

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
            Title("Most Personal Records");
            StartTable(6, 3, "Player", "Total", "2013", "2014", "2015", "2016", "2017", "2018");
            var always = new List<RecordHolder>();
            foreach (var player in all
                .OrderByDescending(h => h.Records.Count)
                .Take(100))
            {
                var recs = player.Records.Count;
                var off = player.Records.Count(r => r.Map.IsOfficial);

                _page.Add("<tr>");
                _page.Add($"<td><a class=\"link\" href=\"https://board.iverb.me/profile/{player.Player.Id}\">{player.Player.Name}</a></td>");
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
            EndTable();

            // Activity
            Title("Activity");
            StartTable(6, 3, "Year/Month", "Total", "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec");
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
            EndTable();

            // Most Records per Year
            Title("Most Records per Year");
            StartTable(4, 4, "Year", "Player", "Total");
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
                    _page.Add($"<td><a class=\"link\" href=\"https://board.iverb.me/profile/{player.Player.Id}\">{player.Player.Name}</a></td>");
                    _page.Add($"<td title=\"{off} Official\">{recs}</td>");
                    _page.Add("</tr>");
                }
            }
            EndTable();

            // At Least One Record Every Year
            Title("Activity Since 2013");
            StartTable(6, 3, "Player", "Total", "2013", "2014", "2015", "2016", "2017", "2018");
            foreach (var player in always
                .OrderByDescending(h => h.Records.Count)
                .Take(10))
            {
                var recs = player.Records.Count;
                var off = player.Records.Count(r => r.Map.IsOfficial);

                _page.Add("<tr>");
                _page.Add($"<td><a class=\"link\" href=\"https://board.iverb.me/profile/{player.Player.Id}\">{player.Player.Name}</a></td>");
                _page.Add($"<td title=\"{off} Official\">{recs}</td>");
                for (int year = 2013; year < 2019; year++)
                {
                    var total = player.Records.Where(r => r.Date.Value.Year == year);
                    var official = total.Where(r => r.Map.IsOfficial);
                    _page.Add($"<td title=\"{official.Count()} Official\">{total.Count()}</td>");
                }
                _page.Add("</tr>");
            }
            EndTable();

            // New Players per Year
            Title("New Players");
            StartTable(4, 4, "Year", "Players", "Total", "Growth");
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
            EndTable();

            // Most Second Places
            Title("Most 2nd Places");
            StartTable(4, 4, "Year", "Player", "Total");
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
                    _page.Add($"<td><a class=\"link\" href=\"https://board.iverb.me/profile/{player.Player.Id}\">{player.Player.Name}</a></td>");
                    _page.Add($"<td title=\"{off} Official\">{recs}</td>");
                    _page.Add("</tr>");
                }
            }
            EndTable();

            // Most Records
            Title("Most Records");
            StartTable(4, 4, "Map", "Records");
            foreach (var map in maps
                .OrderByDescending(m => m.Records.Count)
                .Take(20))
            {
                _page.Add("<tr>");
                _page.Add($"<td><a class=\"link\" href=\"{map.Map.Url}\">{map.Map.Alias}</a></td>");
                _page.Add($"<td>{map.Records.Count}</td>");
                _page.Add("</tr>");
            }
            EndTable();

            // Least Records
            Title("Least Records");
            StartTable(4, 4, "Map", "Records");
            foreach (var map in maps
                .OrderBy(m => m.Records.Count)
                .Take(20))
            {
                _page.Add("<tr>");
                _page.Add($"<td><a class=\"link\" href=\"{map.Map.Url}\">{map.Map.Alias}</a></td>");
                _page.Add($"<td>{map.Records.Count}</td>");
                _page.Add("</tr>");
            }
            EndTable();

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

            Title("Longest Comments");
            StartTable(8, 2, "Player", "Length", "Comment");
            foreach (var others in longest.Take(10))
            {
                _page.Add("<tr>");
                _page.Add($"<td><a class=\"link\" href=\"https://board.iverb.me/profile/{(others.Player as SteamUser).Id}\">{others.Player.Name}</a></td>");
                _page.Add($"<td>{others.Comment.Length}</td>");
                _page.Add($"<td>{others.Comment}</td>");
                _page.Add("</tr>");
            }
            EndTable();

            Title("Shortest Comments");
            StartTable(4, 4, "Player", "Length", "Comment");
            foreach (var others in shortest.Take(10))
            {
                _page.Add("<tr>");
                _page.Add($"<td><a class=\"link\" href=\"https://board.iverb.me/profile/{(others.Player as SteamUser).Id}\">{others.Player.Name}</a></td>");
                _page.Add($"<td>{others.Comment.Length}</td>");
                _page.Add($"<td>{others.Comment}</td>");
                _page.Add("</tr>");
            }
            EndTable();

            Title("Average Comment Length");
            var avgcomment = maps
                .SelectMany(m => m.Records)
                .Where(e => !string.IsNullOrEmpty(e.Comment))
                .Average(e => e.Comment.Length);
            _page.Add($"<br><h2 align=\"center\">{avgcomment.ToString("N2")}</h2>");

            Title("Most Bans by Map");
            StartTable(4, 4, "Map", "Bans");
            foreach (var others in mapbans
                .OrderByDescending(h => h.Records.Count)
                .Take(10))
            {
                _page.Add("<tr>");
                _page.Add($"<td><a class=\"link\" href=\"{others.Map.Url}\">{others.Map.Alias}</a></td>");
                _page.Add($"<td>{others.Records.Count}</td>");
                _page.Add("</tr>");
            }
            EndTable();

            Title("Most Bans by Player");
            StartTable(4, 4, "Player", "Bans");
            foreach (var others in all
                .OrderByDescending(h => h.BannedRecords.Count)
                .Take(20))
            {
                _page.Add("<tr>");
                _page.Add($"<td><a class=\"link\" href=\"https://board.iverb.me/profile/{others.Player.Id}\">{others.Player.Name}</a></td>");
                _page.Add($"<td>{others.BannedRecords.Count}</td>");
                _page.Add("</tr>");
            }
            EndTable();

            Title("Percentage Proof");
            var since = DateTime.Parse("2017-05-11");
            var requireproof = maps
                .SelectMany(m => m.Records)
                .Where(e => e.Date >= since)
                .Where(e => e.Rank.Current <= 5);
            var proof = requireproof
                .Count(e => e.DemoExists || (e as ChangelogEntry).VideoExists);
            var proofornoproof = requireproof
                .Count();
            _page.Add($"<br><h2 title=\"since 2017-05-11\" align=\"center\">{((double)proof / proofornoproof * 100).ToString("N2")}%</h2>");

            Title("Most Demo Uploads");
            StartTable(4, 4, "Player", "Demos");
            var demos = all
                .Where(m => m.Records.Any(e => e.Entry.DemoExists))
                .OrderByDescending(m => m.Records.Count(e => e.Entry.DemoExists));
            foreach (var demo in demos.Take(10))
            {
                _page.Add("<tr>");
                _page.Add($"<td><a class=\"link\" href=\"https://board.iverb.me/profile/{demo.Player.Id}\">{demo.Player.Name}</a></td>");
                _page.Add($"<td>{demo.Records.Count(e => e.Entry.DemoExists)}</td>");
                _page.Add("</tr>");
            }
            EndTable();

            Title("Most Video Links");
            StartTable(4, 4, "Player", "Links");
            var videos = all
                .Where(m => m.Records.Any(e => (e.Entry as ChangelogEntry).VideoExists))
                .OrderByDescending(m => m.Records.Count(e => (e.Entry as ChangelogEntry).VideoExists)); ;
            foreach (var video in videos.Take(10))
            {
                _page.Add("<tr>");
                _page.Add($"<td><a class=\"link\" href=\"https://board.iverb.me/profile/{video.Player.Id}\">{video.Player.Name}</a></td>");
                _page.Add($"<td>{video.Records.Count(e => (e.Entry as ChangelogEntry).VideoExists)}</td>");
                _page.Add("</tr>");
            }
            EndTable();

            EndSection();
        }

        // HTML stuff
        private void StartPage()
        {
            _page.Insert(0,
$@"<!-- src/Portal2Boards.Net.Test/Examples/Leaderboard.cs -->
<!-- v{Version}.0 -->
<!DOCTYPE html>
<html>
	<head>
		<title>Portal2Boards.Net | nekzor.github.io</title>
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
					<a href=""index.html"" class=""breadcrumb"">&nbsp;&nbsp;&nbsp;nekzor.github.io</a>
					<a href=""p2bn.html"" class=""breadcrumb"">Portal2Boards.Net</a>
				</div>
				<div class=""col s12 hide-on-med-and-up"">
					<a href=""#"" data-target=""slide-out"" class=""sidenav-trigger""><i class=""material-icons"">menu</i></a>
					<a href=""p2bn.html"" class=""brand-logo center"">P2BN</a>
                    <a href=""history.html"" class=""brand-logo center"">History</a>
				</div>
			</div>
			<div class=""nav-content"">
				<ul class=""tabs tabs-transparent"">
					{string.Join("\n", _sections)}
					<li class=""tab""><a href=""#about"">About</a></li>
				</ul>
			</div>
		</nav>
		<ul id=""slide-out"" class=""sidenav hide-on-med-and-up"">
			<li><a href=""index.html"">nekzor.github.io</a></li>
			<li><a href=""p2bn.html"">Portal2Boards.Net</a></li>
		</ul>");
        }
        private void EndPage()
        {
            _page.Add(
$@"		<div id=""about"">
			<div class=""row""></div>
			<div class=""row"">
				<div class=""col s12 m12 l10 push-s1"">
					<h3>Portal2Boards.Net Library Test</h3>
					<p><a class=""link"" href=""https://github.com/NeKzor/Portal2Boards.Net"">Portal2Boards.Net</a> is a .NET API wrapper for <a class=""link"" href=""https://board.iverb.me"">board.iverb.me</a>. One of the tests are made to mess around with the data from the website. This page will be generated as result.</p>
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
            _page.Add($"<div id=\"{id}\">");
        }
        void EndSection()
        {
            _page.Add("</div>");
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
				<table class=""highlight"">
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