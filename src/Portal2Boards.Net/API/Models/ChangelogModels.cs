using System.Diagnostics;
using Newtonsoft.Json;

namespace Portal2Boards.API
{
	public sealed class ChangelogEntryModel
	{
		[JsonProperty("player_name")]
		public string PlayerName { get; set; }
		[JsonProperty("avatar")]
		public string Avatar { get; set; }
		[JsonProperty("profile_number")]
		public ulong ProfileNumber { get; set; }
		[JsonProperty("score")]
		public uint? Score { get; set; }
		[JsonProperty("id")]
		public ulong Id { get; set; }
		[JsonProperty("pre_rank")]
		public uint? PreRank { get; set; }
		[JsonProperty("post_rank")]
		public uint? PostRank { get; set; }
		[JsonProperty("wr_gain")]
		public string WrGain { get; set; }
		[JsonProperty("time_gained")]
		public string TimeGained { get; set; }
		[JsonProperty("hasDemo")]
		public string HasDemo { get; set; }
		[JsonProperty("youtubeID")]
		public string YouTubeId { get; set; }
		[JsonProperty("note")]
		public string Note { get; set; }
		[JsonProperty("banned")]
		public string Banned { get; set; }
		[JsonProperty("submission")]
		public string Submission { get; set; }
		[JsonProperty("previous_score")]
		public uint? PreviousScore { get; set; }
		[JsonProperty("chamberName")]
		public string ChamberName { get; set; }
		[JsonProperty("chapterId")]
		public uint ChapterId { get; set; }
		[JsonProperty("mapid")]
		public uint MapId { get; set; }
		[JsonProperty("improvement")]
		public uint? Improvement { get; set; }
		[JsonProperty("rank_improvement")]
		public uint? RankImprovement { get; set; }
		[JsonProperty("pre_points")]
		public uint? PrePoints { get; set; }
		[JsonProperty("post_point")]
		public uint? PostPoints { get; set; }
		[JsonProperty("point_improvement")]
		public uint? PointImprovement { get; set; }
	}
}