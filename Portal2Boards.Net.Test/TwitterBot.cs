using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Portal2Boards.Net.API;
using Portal2Boards.Net.Entities;
using Portal2Boards.Net.Extensions;

namespace Portal2Boards.Net.Test
{
	// This is a copy of NeKzBot (https://github.com/NeKzor/NeKzBot)
	public static class Twitter
	{
		public const int TweetLimit = 140;
	}

	internal static class TwitterBot
    {
		private static ChangelogParameters _latestWorldRecords { get; set; }
		private static Portal2BoardsClient _client { get; set; }
		private static readonly List<string> _tweets = new List<string>();
		private const string _path = @"tweets.txt";

		public static Task InitAsync()
		{
			_latestWorldRecords = new ChangelogParameters
			{
				[Parameters.WorldRecord] = 1,
				[Parameters.MaxDaysAgo] = 4
			};
			_client = new Portal2BoardsClient(_latestWorldRecords, autoCache: false);
			return Task.CompletedTask;
		}

		public static async Task RunAsync()
		{
			if (File.Exists(_path))
				File.Delete(_path);
			_tweets.Clear();

			var entryupdates = await _client.GetChangelogAsync();
			if (entryupdates != null)
			{
				foreach (var update in entryupdates)
				{
					var delta = await GetWorldRecordDelta(update) ?? -1;
					var wrdelta = (delta != -1) ? $" (-{delta.ToString("N2")})"
												: string.Empty;
					var tweet = await FormatMainTweetAsync($"New World Record in {update.Map.Name}\n" +
														   $"{update.Score.Current.AsTimeToString()}{wrdelta} by {update.Player.Name}\n" +
														   $"{update.Date?.DateTimeToString()} (UTC)",
														   (update.DemoExists) ? update.DemoLink : string.Empty,
														   (update.VideoExists) ? update.VideoLink : string.Empty);
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

		private static async Task<float?> GetWorldRecordDelta(EntryData wr)
		{
			var map = await Portal2.GetMapByName(wr.Map.Name);
			var found = false;
			var foundcoop = false;
			foreach (var entry in await _client.GetChangelogAsync($"?wr=1&chamber={map.BestTimeId}"))
			{
				if (entry.IsBanned)
					continue;
				if (found)
				{
					var oldwr = entry.Score.Current.AsTime();
					var newwr = wr.Score.Current.AsTime();
					if (map.Type == MapType.Cooperative)
					{
						if (foundcoop)
						{
							if (oldwr == newwr)
								return 0;
							if (newwr < oldwr)
								return oldwr - newwr;
						}
						else if (oldwr == newwr)
						{
							foundcoop = true;
							continue;
						}
						else
						{
							if (oldwr == newwr)
								return 0;
							if (newwr < oldwr)
								return oldwr - newwr;
						}
					}
					else if (map.Type == MapType.SinglePlayer)
					{
						if (oldwr == newwr)
							return 0;
						if (newwr < oldwr)
							return oldwr - newwr;
					}
					break;
				}
				if (entry.Id == wr.Id)
					found = true;
			}
			return default(float?);
		}

		private static async Task<string> FormatMainTweetAsync(string msg, params string[] stuff)
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
