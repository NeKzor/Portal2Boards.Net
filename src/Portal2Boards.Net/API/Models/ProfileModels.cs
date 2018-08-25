using System.Collections.Generic;
using Newtonsoft.Json;

namespace Portal2Boards.API
{
    public sealed class ProfileModel
    {
        [JsonProperty("profileNumber")]
        public ulong ProfileNumber { get; set; }
        [JsonProperty("isRegistered")]
        public string IsRegistered { get; set; }
        [JsonProperty("hasRecords")]
        public string HasRecords { get; set; }
        [JsonProperty("userData")]
        public ProfileUserModel UserData { get; set; }
        [JsonProperty("points")]
        public ProfilePointsModel Points { get; set; }
        [JsonProperty("times")]
        public ProfileTimesDataModel Times { get; set; }
    }

    public sealed class ProfileUserModel
    {
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }
        [JsonProperty("profile_number")]
        public ulong ProfileNumber { get; set; }
        [JsonProperty("boardname")]
        public string BoardName { get; set; }
        [JsonProperty("steamname")]
        public string SteamName { get; set; }
        [JsonProperty("banned")]
        public string Banned { get; set; }
        [JsonProperty("registered")]
        public string Registered { get; set; }
        [JsonProperty("avatar")]
        public string Avatar { get; set; }
        [JsonProperty("twitch")]
        public string Twitch { get; set; }
        [JsonProperty("youtube")]
        public string YouTube { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("admin")]
        public string Admin { get; set; }
    }

    public sealed class ProfilePointsModel
    {
        [JsonProperty("SP")]
        public ProfilePointsDataModel Sp { get; set; }
        [JsonProperty("COOP")]
        public ProfilePointsDataModel Coop { get; set; }
        [JsonProperty("global")]
        public ProfilePointsDataModel Global { get; set; }
        [JsonProperty("chapters")]
        public IReadOnlyDictionary<string, ProfilePointsDataModel> Chapters { get; set; }
    }

    public class ProfilePointsDataModel
    {
        [JsonProperty("score")]
        public uint? Score { get; set; }
        [JsonProperty("playerRank")]
        public uint? PlayerRank { get; set; }
        [JsonProperty("scoreRank")]
        public uint? ScoreRank { get; set; }
        [JsonProperty("WRDiff")]
        public float? WrDiff { get; set; }
        [JsonProperty("nextRankDiff")]
        public int? NextRankDiff { get; set; }
    }

    public sealed class ProfileTimesDataModel
    {
        [JsonProperty("SP")]
        public ProfileTimesModel Sp { get; set; }
        [JsonProperty("COOP")]
        public ProfileTimesModel Coop { get; set; }
        [JsonProperty("global")]
        public ProfileTimesModel Global { get; set; }
        [JsonProperty("chapters")]
        public IReadOnlyDictionary<string, ProfilePointsDataModel> Chapters { get; set; }
        [JsonProperty("numDemos")]
        public uint NumDemos { get; set; }
        [JsonProperty("numYoutubeVideos")]
        public uint NumYouTubeVideos { get; set; }
        [JsonProperty("bestRank")]
        public ProfileScoreModel BestRank { get; set; }
        [JsonProperty("worstRank")]
        public ProfileScoreModel WorstRank { get; set; }
        [JsonProperty("oldestScore")]
        public ProfileScoreModel OldestScore { get; set; }
        [JsonProperty("newestScore")]
        public ProfileScoreModel NewestScore { get; set; }
        [JsonProperty("numWRs")]
        public uint NumWrs { get; set; }
        [JsonProperty("globalAveragePlace")]
        public float? GlobalAveragePlace { get; set; }
    }

    public sealed class ProfileTimesModel : ProfilePointsDataModel
    {
        /// <summary> NOTE: This will be null for Global. </summary>
        [JsonProperty("chambers")]
        public ProfileTimesChamberData Chambers { get; set; }
    }

    public sealed class ProfileTimesChamberData
    {
        [JsonProperty("numWRs")]
        public uint NumWrs { get; set; }
        [JsonProperty("rankSum")]
        public uint RankSum { get; set; }
        [JsonProperty("mapCount")]
        public uint MapCount { get; set; }
        [JsonProperty("bestRank")]
        public ProfileTimesScoreModel BestRank { get; set; }
        [JsonProperty("worstRank")]
        public ProfileTimesScoreModel WorstRank { get; set; }
        [JsonProperty("oldestScore")]
        public ProfileTimesScoreModel OldestScore { get; set; }
        [JsonProperty("newestScore")]
        public ProfileTimesScoreModel NewestScore { get; set; }
        [JsonProperty("numDemos")]
        public uint NumDemos { get; set; }
        [JsonProperty("numYouTubeVideos")]
        public uint NumYouTubeVideos { get; set; }
        [JsonProperty("chamber")]
        public IReadOnlyDictionary<uint, IReadOnlyDictionary<uint, ProfileTimesMapModel>> Chamber { get; set; }
        [JsonProperty("averagePlace")]
        public float? AveragePlace { get; set; }
    }

    public sealed class ProfileTimesScoreModel : ProfileScoreModel
    {
    }

    public sealed class ProfileTimesMapModel : ProfileScoreDataModel
    {
        [JsonProperty("WRDiff")]
        public float? WrDiff { get; set; }
        [JsonProperty("nextRankDiff")]
        public int? NextRankDiff { get; set; }
    }

    public class ProfileScoreModel
    {
        [JsonProperty("scoreData")]
        public ProfileScoreDataModel ScoreData { get; set; }
        [JsonProperty("map")]
        public ulong Map { get; set; }
    }

    public class ProfileScoreDataModel
    {
        [JsonProperty("note")]
        public string Note { get; set; }
        [JsonProperty("submission")]
        public string Submission { get; set; }
        [JsonProperty("changelogId")]
        public uint ChangelogId { get; set; }
        [JsonProperty("playerRank")]
        public uint? PlayerRank { get; set; }
        [JsonProperty("scoreRank")]
        public uint? ScoreRank { get; set; }
        [JsonProperty("score")]
        public uint? Score { get; set; }
        [JsonProperty("date")]
        public string Date { get; set; }
        [JsonProperty("hasDemo")]
        public string HasDemo { get; set; }
        [JsonProperty("youtubeID")]
        public string YouTubeId { get; set; }
    }
}