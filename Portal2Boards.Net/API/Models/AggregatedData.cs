using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;

namespace Portal2Boards.Net.API.Models
{
	[DebuggerDisplay("Count = {Points.Count + Times.Count,nq}")]
	public sealed class AggregatedData
	{
		[JsonProperty("Points")]
		public Dictionary<ulong, AggregatedEntryData> Points { get; set; }
		[JsonProperty("Times")]
		public Dictionary<ulong, AggregatedEntryData> Times { get; set; }
	}

	public sealed class AggregatedEntryData
	{
		[JsonProperty("userData")]
		public AggregatedEntryUserData UserData { get; set; }
		[JsonProperty("scoreData")]
		public AggregatedEntryScoreData ScoreData { get; set; }
	}

	public sealed class AggregatedEntryUserData
	{
		[JsonProperty("boardname")]
		public string BoardName { get; set; }
		[JsonProperty("avatar")]
		public string Avatar { get; set; }
	}

	public sealed class AggregatedEntryScoreData
	{
		[JsonProperty("score")]
		public string Score { get; set; }
		[JsonProperty("playerRank")]
		public string PlayerRank { get; set; }
		[JsonProperty("scoreRank")]
		public string ScoreRank { get; set; }
	}
}