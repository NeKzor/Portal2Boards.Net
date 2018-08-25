using Newtonsoft.Json;

namespace Portal2Boards.API
{
    public sealed class ChamberEntryModel
    {
        [JsonProperty("scoreData")]
        public ChamberEntryScoreDataModel Score { get; set; }
        [JsonProperty("userData")]
        public ChamberEntryUserDataModel User { get; set; }
    }

    public sealed class ChamberEntryScoreDataModel
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

    public sealed class ChamberEntryUserDataModel
    {
        [JsonProperty("boardname")]
        public string BoardName { get; set; }
        [JsonProperty("avatar")]
        public string Avatar { get; set; }
    }
}