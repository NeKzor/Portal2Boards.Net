using System.Diagnostics;
using Newtonsoft.Json;
using Portal2Boards.Net.Entities;

namespace Portal2Boards.Net.API.Models
{
	[DebuggerDisplay("{Id,nq}")]
	public sealed class AggregatedData
	{
		public string Id { get; set; }
		public AggregatedEntryData Data { get; set; }

		public AggregatedData(string id, AggregatedEntryData data)
		{
			Id = id;
			Data = data;
		}

		//public static explicit operator ChamberData(BoardData data)
		//	=> new ChamberData(data);
	}

	public sealed class AggregatedEntryData
	{
		[JsonProperty("userData")]
		public AggregatedEntryUserData User { get; set; }
		[JsonProperty("scoreData")]
		public AggregatedEntryScoreData Score { get; set; }
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