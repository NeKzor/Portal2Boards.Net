using System.Diagnostics;
using Newtonsoft.Json;

namespace Portal2Boards.API
{
    public sealed class DonatorModel
    {
        [JsonProperty("profile_number")]
        public ulong ProfileNumber { get; set; }
        [JsonProperty("avatar")]
        public string Avatar { get; set; }
        [JsonProperty("playername")]
        public string PlayerName { get; set; }
        [JsonProperty("donation_amount")]
        public decimal DonationAmount { get; set; }
    }
}