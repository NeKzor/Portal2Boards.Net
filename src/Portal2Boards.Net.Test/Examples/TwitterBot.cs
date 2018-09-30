using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Portal2Boards.Extensions;

namespace Portal2Boards.Test.Examples
{
    // This is a copy of NeKzBot (https://github.com/NeKzor/NeKzBot)
    internal static class Twitter
    {
        public const int TweetLimit = 280;
    }

    internal class TwitterBot
    {
        private ChangelogQuery _latestWorldRecords { get; set; }
        private Portal2BoardsClient _client { get; set; }
        private readonly List<string> _tweets = new List<string>();

        private const string _path = "tweets.txt";

        public Task InitAsync()
        {
            _latestWorldRecords = new ChangelogQueryBuilder()
                .WithWorldRecord(true)
                .WithMaxDaysAgo(4)
                .Build();
            _client = new Portal2BoardsClient(autoCache: false);
            return Task.CompletedTask;
        }

        public async Task RunAsync()
        {
            if (File.Exists(_path))
                File.Delete(_path);
            _tweets.Clear();

            var changelog = await _client.GetChangelogAsync(() => _latestWorldRecords);
            if (changelog != null)
            {
                foreach (ChangelogEntry update in changelog.Entries)
                {
                    var map = Portal2Map.Search(update.MapId);

                    var delta = await GetWorldRecordDelta(map, update) ?? -1;
                    var wrdelta = (delta != -1)
                        ? $" (-{delta.ToString("N2")})"
                        : string.Empty;

                    var tweet = await FormatMainTweetAsync
                    (
                        $"New World Record in {update.Name}\n" +
                        $"{update.Score.Current.AsTimeToString()}{wrdelta} by {update.Player.Name}\n" +
                        $"{update.Date?.DateTimeToString()} (CST)",
                        (update.DemoExists) ? update.DemoUrl : string.Empty,
                        (update.VideoExists) ? update.VideoUrl : string.Empty
                    );

                    if (tweet != string.Empty)
                    {
                        _tweets.Add(tweet);
                        var reply = await FormatReplyTweetAsync(update.Player.Name, update.Comment);
                        if (reply != string.Empty)
                            _tweets.Add(reply);
                    }
                }
                _tweets.Add(string.Empty);
            }
            File.AppendAllLines(_path, _tweets);
        }

        private async Task<float?> GetWorldRecordDelta(Portal2Map map, IChangelogEntry wr)
        {
            var found = false;
            var foundcoop = false;

            var changelog = await _client.GetChangelogAsync($"?wr=1&chamber={map.BestTimeId}");

            foreach (ChangelogEntry entry in changelog.Entries)
            {
                if (entry.IsBanned)
                    continue;
                if (found)
                {
                    var oldwr = entry.Score.Current.AsTime();
                    var newwr = wr.Score.Current.AsTime();
                    if (map.Type == Portal2MapType.Cooperative)
                    {
                        if (foundcoop)
                        {
                            if (oldwr == newwr)
                                return 0;
                            if (newwr < oldwr)
                                return oldwr - newwr;
                        }
                        // Tie or partner score
                        else if (oldwr == newwr)
                        {
                            // Cooperative world record without a partner
                            // will be ignored, sadly that's a thing :>
                            foundcoop = true;
                            continue;
                        }
                        else if (newwr < oldwr)
                        {
                            return oldwr - newwr;
                        }
                    }
                    else if (map.Type == Portal2MapType.SinglePlayer)
                    {
                        if (oldwr == newwr)
                            return 0;
                        if (newwr < oldwr)
                            return oldwr - newwr;
                    }
                    break;
                }
                if (entry.Id == (wr as ChangelogEntry).Id)
                    found = true;
            }

            return default;
        }

        private async Task<string> FormatMainTweetAsync(string msg, params string[] stuff)
        {
            var output = msg;
            try
            {
                if (Twitter.TweetLimit - output.Length < 0)
                    return string.Empty;

                foreach (var item in stuff)
                {
                    if (item == string.Empty)
                        continue;
                    if (Twitter.TweetLimit - output.Length - item.Length < 0)
                        return output;
                    output += "\n" + item;
                }
                return output;
            }
            catch (Exception e)
            {
                await Logger.LogException("TwitterBot.FormatMainTweetAsync Error", e);
            }
            return string.Empty;
        }

        private static async Task<string> FormatReplyTweetAsync(string player, string comment)
        {
            var newline = $"@Portal2Records {player}: ";
            var output = string.Empty;
            try
            {
                // There isn't always a comment
                if (string.IsNullOrEmpty(comment))
                    return comment;

                // Check what's left
                const string cut = "...";
                var left = Twitter.TweetLimit - output.Length - newline.Length - comment.Length;
                if ((-left + cut.Length == comment.Length)
                || (newline.Length >= Twitter.TweetLimit - output.Length))
                    return output;

                // It's safe
                if (left >= 0)
                    return newline + comment;

                // Cut comment and append "..." at the end
                return newline + comment.Substring(0, Twitter.TweetLimit - output.Length - newline.Length - cut.Length) + cut;
            }
            catch (Exception e)
            {
                await Logger.LogException("TwitterBot.FormatReplyTweetAsync Error", e);
            }
            return string.Empty;
        }

        internal static class Logger
        {
            public static Task LogException(string msg, Exception e)
            {
                Debug.WriteLine($"{msg}\n{e}");
                return Task.CompletedTask;
            }
        }
    }
}
