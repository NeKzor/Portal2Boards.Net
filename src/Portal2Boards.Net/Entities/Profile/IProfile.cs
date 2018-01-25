namespace Portal2Boards
{
    public interface IProfile
	{
		ulong SteamId { get; }
		string DisplayName { get; }
		string BoardName { get; }
		string SteamName { get; }
		bool IsBanned { get; }
		bool IsRegistered { get; }
		bool HasRecords { get; }
		string SteamAvatarLink { get; }
		string TwitchLink { get; }
		string YouTubeLink { get; }
		string Title { get; }
		bool IsAdmin { get; }
		IDataPoints Points { get; }
		IDataTimes Times { get; }
		uint DemoCount { get; }
		uint YouTubeVideoCount { get; }
		IDataScore BestScore { get; }
		IDataScore WorstScore { get; }
		IDataScore OldestScore { get; }
		IDataScore NewestScore { get; }
		uint WorldRecordCount { get; }
		float? GlobalAveragePlace { get; }
	}
}