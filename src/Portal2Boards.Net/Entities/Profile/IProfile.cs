namespace Portal2Boards
{
    public interface IProfile
	{
		string DisplayName { get; }
		string BoardName { get; }
		string SteamName { get; }
		bool IsBanned { get; }
		bool IsRegistered { get; }
		bool HasRecords { get; }
		string SteamAvatarUrl { get; }
		string TwitchName { get; }
		string YouTubeUrl { get; }
		string Title { get; }
		bool IsAdmin { get; }
		IDataPoints Points { get; }
		IDataTimes Times { get; }
		uint Demos { get; }
		uint YouTubeVideos { get; }
		IDataScore BestScore { get; }
		IDataScore WorstScore { get; }
		IDataScore OldestScore { get; }
		IDataScore NewestScore { get; }
		uint WorldRecords { get; }
		float? GlobalAveragePlace { get; }
	}
}