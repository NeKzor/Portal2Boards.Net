using System;

namespace Portal2Boards
{
    public interface IChangelogEntry
    {
        DateTime? Date { get; }
        uint MapId { get; }
        ChapterType Chapter { get; }
        string Name { get; }
        IEntryData Score { get; }
        IEntryData Rank { get; }
        IEntryData Points { get; }
        ISteamUser Player { get; }
        bool IsBanned { get; }
        bool IsSubmission { get; }
        bool IsWorldRecord { get; }
        bool DemoExists { get; }
        string YouTubeId { get; }
        string Comment { get; }
    }
}
