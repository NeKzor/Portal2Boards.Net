using System.Diagnostics;
using Newtonsoft.Json;

namespace Portal2Boards.API
{
    public sealed class WallOfShameModel
    {
        [JsonProperty("profile_number")]
        public ulong ProfileNumber { get; set; }
        [JsonProperty("avatar")]
        public string Avatar { get; set; }
        [JsonProperty("playername")]
        public string PlayerName { get; set; }
    }
}
