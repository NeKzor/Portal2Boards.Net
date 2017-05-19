using System.Diagnostics;
using Newtonsoft.Json;
using Portal2Boards.Net.Entities;

namespace Portal2Boards.Net.API.Models
{
	[DebuggerDisplay("{Id,nq}")]
	public sealed class BoardData
	{
		public ulong Id { get; set; }
		public BoardEntryData Data { get; set; }

		public BoardData(ulong id, BoardEntryData data)
		{
			Id = id;
			Data = data;
		}

		public static explicit operator ChamberData(BoardData data)
			=> new ChamberData(data);
	}

	public sealed class BoardEntryData
	{
		[JsonProperty("scoreData")]
		public BoardEntryScoreData Score { get; set; }
		[JsonProperty("userData")]
		public BoardEntryUserData User { get; set; }
	}

	public sealed class BoardEntryScoreData
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

	public sealed class BoardEntryUserData
	{
		[JsonProperty("boardname")]
		public string BoardName { get; set; }
		[JsonProperty("avatar")]
		public string Avatar { get; set; }
	}
}