using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using Portal2Boards.Net.Entities;

namespace Portal2Boards.Net.API.Models
{
	[DebuggerDisplay("{UserData.ProfileNumber,nq}")]
	public sealed class ProfileData
	{
		[JsonProperty("profileNumber")]
		public string ProfileNumber { get; set; }
		[JsonProperty("isRegistered")]
		public string IsRegistered { get; set; }
		[JsonProperty("hasRecords")]
		public string HasRecords { get; set; }
		[JsonProperty("userData")]
		public ProfileUserData UserData { get; set; }
		[JsonProperty("points")]
		public ProfilePointsData Points { get; set; }
		[JsonProperty("times")]
		public ProfileTimesData Times { get; set; }

		public static explicit operator UserData(ProfileData data)
			=> new UserData(data);
	}

	public sealed class ProfileUserData
	{
		[JsonProperty("displayName")]
		public string DisplayName { get; set; }
		[JsonProperty("profile_number")]
		public string ProfileNumber { get; set; }
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

	public sealed class ProfilePointsData
	{
		[JsonProperty("SP")]
		public ProfilePoints Sp { get; set; }
		[JsonProperty("COOP")]
		public ProfilePoints Coop { get; set; }
		[JsonProperty("global")]
		public ProfilePoints Global { get; set; }
		[JsonProperty("chapters")]
		public Dictionary<string, ProfilePoints> Chapters { get; set; }
	}

	public class ProfilePoints
	{
		[JsonProperty("score")]
		public string Score { get; set; }
		[JsonProperty("playerRank")]
		public string PlayerRank { get; set; }
		[JsonProperty("scoreRank")]
		public string ScoreRank { get; set; }
		[JsonProperty("WRDiff")]
		public string WrDiff { get; set; }
		[JsonProperty("nextRankDiff")]
		public string NextRankDiff { get; set; }
	}

	public sealed class ProfileTimesData
	{
		[JsonProperty("SP")]
		public ProfileTimes Sp { get; set; }
		[JsonProperty("COOP")]
		public ProfileTimes Coop { get; set; }
		[JsonProperty("global")]
		public ProfileTimes Global { get; set; }
		[JsonProperty("chapters")]
		public Dictionary<string, ProfilePoints> Chapters { get; set; }
		[JsonProperty("numDemos")]
		public string NumDemos { get; set; }
		[JsonProperty("numYoutubeVideos")]
		public string NumYouTubeVideos { get; set; }
		[JsonProperty("bestRank")]
		public ProfileScore BestRank { get; set; }
		[JsonProperty("worstRank")]
		public ProfileScore WorstRank { get; set; }
		[JsonProperty("oldestScore")]
		public ProfileScore OldestScore { get; set; }
		[JsonProperty("newestScore")]
		public ProfileScore NewestScore { get; set; }
		[JsonProperty("numWRs")]
		public string NumWrs { get; set; }
		[JsonProperty("globalAveragePlace")]
		public string GlobalAveragePlace { get; set; }
	}

	public sealed class ProfileTimes : ProfilePoints
	{
		[JsonProperty("chambers")]
		public ProfileTimesChamberData Chambers { get; set; }
	}

	public sealed class ProfileTimesChamberData
	{
		[JsonProperty("numWRs")]
		public string NumWrs { get; set; }
		[JsonProperty("rankSum")]
		public string RankSum { get; set; }
		[JsonProperty("mapCount")]
		public string MapCount { get; set; }
		[JsonProperty("bestRank")]
		public ProfileTimesScore BestRank { get; set; }
		[JsonProperty("worstRank")]
		public ProfileTimesScore WorstRank { get; set; }
		[JsonProperty("oldestScore")]
		public ProfileTimesScore OldestScore { get; set; }
		[JsonProperty("newestScore")]
		public ProfileTimesScore NewestScore { get; set; }
		[JsonProperty("numDemos")]
		public uint NumDemos { get; set; }
		[JsonProperty("numYouTubeVideos")]
		public uint NumYouTubeVideos { get; set; }
		[JsonProperty("chamber")]
		public ProfileTimesChamber Chamber { get; set; }
		[JsonProperty("averagePlace")]
		public string AveragePlace { get; set; }
	}

	public sealed class ProfileTimesScore : ProfileScore
	{
	}

	public sealed class ProfileTimesChamber
	{
		public Dictionary<string, ProfileTimesChapter> Data { get; set; }
	}

	public sealed class ProfileTimesChapter
	{
		public Dictionary<string, ProfileTimesMapData> Data { get; set; }
	}

	public sealed class ProfileTimesMapData : ProfileScoreData
	{
		[JsonProperty("WRDiff")]
		public string WrDiff { get; set; }
		[JsonProperty("nextRankDiff")]
		public string NextRankDiff { get; set; }
	}

	public class ProfileScore
	{
		[JsonProperty("scoreData")]
		public ProfileScoreData ScoreData { get; set; }
		[JsonProperty("map")]
		public string Map { get; set; }
	}

	public class ProfileScoreData
	{
		[JsonProperty("note")]
		public string Note { get; set; }
		[JsonProperty("submission")]
		public string Submission { get; set; }
		[JsonProperty("changelogId")]
		public string ChangelogId { get; set; }
		[JsonProperty("playerRank")]
		public string PlayerRank { get; set; }
		[JsonProperty("scoreRank")]
		public string ScoreRank { get; set; }
		[JsonProperty("score")]
		public string Score { get; set; }
		[JsonProperty("date")]
		public string Date { get; set; }
		[JsonProperty("hasDemo")]
		public string HasDemo { get; set; }
		[JsonProperty("youtubeID")]
		public string YouTubeId { get; set; }
	}
}