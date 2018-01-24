using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;

namespace Portal2Boards.API.Models
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
		public ProfileUserData UserData { get; set; }
		[JsonProperty("points")]
		public ProfilePointsData Points { get; set; }
		[JsonProperty("times")]
		public ProfileTimesData Times { get; set; }
	}

	public sealed class ProfileUserData
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

	public sealed class ProfilePointsData
	{
		[JsonProperty("SP")]
		public ProfilePoints Sp { get; set; }
		[JsonProperty("COOP")]
		public ProfilePoints Coop { get; set; }
		[JsonProperty("global")]
		public ProfilePoints Global { get; set; }
		[JsonProperty("chapters")]
		public IReadOnlyDictionary<string, ProfilePoints> Chapters { get; set; }
	}

	public class ProfilePoints
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

	public sealed class ProfileTimesData
	{
		[JsonProperty("SP")]
		public ProfileTimes Sp { get; set; }
		[JsonProperty("COOP")]
		public ProfileTimes Coop { get; set; }
		[JsonProperty("global")]
		public ProfileTimes Global { get; set; }
		[JsonProperty("chapters")]
		public IReadOnlyDictionary<string, ProfilePoints> Chapters { get; set; }
		[JsonProperty("numDemos")]
		public uint NumDemos { get; set; }
		[JsonProperty("numYoutubeVideos")]
		public uint NumYouTubeVideos { get; set; }
		[JsonProperty("bestRank")]
		public ProfileScore BestRank { get; set; }
		[JsonProperty("worstRank")]
		public ProfileScore WorstRank { get; set; }
		[JsonProperty("oldestScore")]
		public ProfileScore OldestScore { get; set; }
		[JsonProperty("newestScore")]
		public ProfileScore NewestScore { get; set; }
		[JsonProperty("numWRs")]
		public uint NumWrs { get; set; }
		[JsonProperty("globalAveragePlace")]
		public float? GlobalAveragePlace { get; set; }
	}

	public sealed class ProfileTimes : ProfilePoints
	{
		/// <summary> Note: This will be null for Global. </summary>
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
		public IReadOnlyDictionary<uint, IReadOnlyDictionary<uint, ProfileTimesMapData>> Chamber { get; set; }
		[JsonProperty("averagePlace")]
		public float? AveragePlace { get; set; }
	}

	public sealed class ProfileTimesScore : ProfileScore
	{
	}

	public sealed class ProfileTimesMapData : ProfileScoreData
	{
		[JsonProperty("WRDiff")]
		public float? WrDiff { get; set; }
		[JsonProperty("nextRankDiff")]
		public int? NextRankDiff { get; set; }
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