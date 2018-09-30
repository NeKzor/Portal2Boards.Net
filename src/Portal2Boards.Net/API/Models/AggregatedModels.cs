using System.Collections.Generic;
using Newtonsoft.Json;

namespace Portal2Boards.API
{
    public sealed class AggregatedModel
    {
        [JsonProperty("Points")]
        public Dictionary<ulong, AggregatedEntryDataModel> Points { get; set; }
        [JsonProperty("Times")]
        public Dictionary<ulong, AggregatedEntryDataModel> Times { get; set; }
    }

    public sealed class AggregatedEntryDataModel
    {
        [JsonProperty("userData")]
        public AggregatedEntryUserDataModel UserData { get; set; }
        [JsonProperty("scoreData")]
        public AggregatedEntryScoreDataModel ScoreData { get; set; }
    }

    public sealed class AggregatedEntryUserDataModel
    {
        [JsonProperty("boardname")]
        public string BoardName { get; set; }
        [JsonProperty("avatar")]
        public string Avatar { get; set; }
    }

    public sealed class AggregatedEntryScoreDataModel
    {
        [JsonProperty("score")]
        public string Score { get; set; }
        [JsonProperty("playerRank")]
        public string PlayerRank { get; set; }
        [JsonProperty("scoreRank")]
        public string ScoreRank { get; set; }
    }
}
