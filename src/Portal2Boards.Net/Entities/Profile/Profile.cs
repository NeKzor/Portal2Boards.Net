using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Portal2Boards.API;
using Model = Portal2Boards.API.ProfileModel;

namespace Portal2Boards
{
    [DebuggerDisplay("{SteamId,nq}")]
    public class Profile : IEntity<ulong>, IProfile, IUpdatable
    {
        public ulong Id { get; private set; }
        public string DisplayName { get; private set; }
        public string BoardName { get; private set; }
        public string SteamName { get; private set; }
        public bool IsBanned { get; private set; }
        public bool IsRegistered { get; private set; }
        public bool HasRecords { get; private set; }
        public string SteamAvatarUrl { get; private set; }
        public string TwitchName { get; private set; }
        public string YouTubeUrl { get; private set; }
        public string Title { get; private set; }
        public bool IsAdmin { get; private set; }
        public IDataPoints Points { get; private set; }
        public IDataTimes Times { get; private set; }
        public uint Demos { get; private set; }
        public uint YouTubeVideos { get; private set; }
        public IDataScore BestScore { get; private set; }
        public IDataScore WorstScore { get; private set; }
        public IDataScore OldestScore { get; private set; }
        public IDataScore NewestScore { get; private set; }
        public uint WorldRecords { get; private set; }
        public float? GlobalAveragePlace { get; private set; }

        public string Url
            => $"https://board.iverb.me/profile/{Id}";
        public string SteamUrl
            => $"https://steamcommunity.com/profiles/{Id}";

        internal Portal2BoardsClient Client { get; private set; }

        public async Task<IChangelog> GetChangelogAsync(bool ignoreCache = false)
            => await Client.GetChangelogAsync($"?profileNumber={Id}", ignoreCache).ConfigureAwait(false);

        public async Task UpdateAsync(bool ignoreCache = false)
        {
            var profile = await Client.GetProfileAsync(Id, ignoreCache).ConfigureAwait(false);
            if (profile == null)
                throw new Exception("Failed to update profile.");

            IsRegistered = profile.IsRegistered;
            HasRecords = profile.HasRecords;
            DisplayName = profile.DisplayName;
            BoardName = profile.BoardName;
            SteamName = profile.SteamName;
            IsBanned = profile.IsBanned;
            SteamAvatarUrl = profile.SteamAvatarUrl;
            TwitchName = profile.TwitchName;
            YouTubeUrl = profile.YouTubeUrl;
            Title = profile.Title;
            IsAdmin = profile.IsAdmin;
            Points = profile.Points;
            Times = profile.Times;
            Demos = profile.Demos;
            YouTubeVideos = profile.YouTubeVideos;
            BestScore = profile.BestScore;
            WorstScore = profile.WorstScore;
            OldestScore = profile.OldestScore;
            NewestScore = profile.NewestScore;
            WorldRecords = profile.WorldRecords;
            GlobalAveragePlace = profile.GlobalAveragePlace;
        }

        internal static Profile Create(Portal2BoardsClient client, Model model)
        {
            var chapters = new Dictionary<ChapterType, IDataScoreInfo>();
            if (model.Points?.Chapters != null)
            {
                foreach (var item in model.Points.Chapters)
                {
                    if (Enum.TryParse<ChapterType>(item.Key, out var chapter))
                        chapters.Add(chapter, Portal2Boards.DataScoreInfo.Create(item.Value));
                }
            }

            return new Profile()
            {
                Id = model.ProfileNumber,
                IsRegistered = model.IsRegistered == "1",
                HasRecords = model.HasRecords == "1",
                DisplayName = WebUtility.HtmlDecode(model.UserData.DisplayName),
                BoardName = WebUtility.HtmlDecode(model.UserData.BoardName),
                SteamName = WebUtility.HtmlDecode(model.UserData.SteamName),
                IsBanned = model.UserData.Banned == "1",
                SteamAvatarUrl = model.UserData.Avatar,
                TwitchName = model.UserData.Twitch,
                YouTubeUrl = model.UserData.YouTube,
                Title = model.UserData.Title,
                IsAdmin = model.UserData.Admin == "1",
                Points = DataPoints.Create
                (
                    Portal2Boards.DataScoreInfo.Create(model.Points.Sp),
                    Portal2Boards.DataScoreInfo.Create(model.Points.Coop),
                    Portal2Boards.DataScoreInfo.Create(model.Points.Global),
                    chapters
                ),
                Times = DataTimes.Create(client, model.Times),
                Demos = model.Times.NumDemos,
                YouTubeVideos = model.Times.NumYouTubeVideos,
                BestScore = DataScore.Create(client, model.Times.BestRank),
                WorstScore = DataScore.Create(client, model.Times.WorstRank),
                OldestScore = DataScore.Create(client, model.Times.OldestScore),
                NewestScore = DataScore.Create(client, model.Times.NewestScore),
                WorldRecords = model.Times.NumWrs,
                GlobalAveragePlace = model.Times.GlobalAveragePlace
            };
        }
    }
}
