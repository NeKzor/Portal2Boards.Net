using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Portal2Boards
{
	public class ChangelogQuery
	{
		public ulong? MapId { get; set; }
		public ChapterType? Chapter { get; set; }
		public string ProfileName { get; set; }
		public ulong? ProfileId { get; set; }
		public uint? Type { get; set; }
		public bool? SinglePlayer { get; set; }
		public bool? Cooperative { get; set; }
		public bool? WorldRecord { get; set; }
		public bool? Banned { get; set; }
		public bool? Demo { get; set; }
		public bool? YouTube { get; set; }
		public bool? Submission { get; set; }
		public uint? MaxDaysAgo { get; set; }
		public bool? HasDate { get; set; }

		public string GetString()
		{
			var query = new StringBuilder();

			if (MapId != null) query.Append($"&chamber={MapId}");
			if (Chapter != null) query.Append($"&chapter={(int)Chapter}");
			if (ProfileName != null) query.Append($"&boardName={WebUtility.HtmlEncode(ProfileName)}");
			if (ProfileId != null) query.Append($"&profileNumber={ProfileId}");
			if (Type != null) query.Append($"&type={Type}");
			if (SinglePlayer != null) query.Append($"&sp={((bool)SinglePlayer ? 1 : 0)}");
			if (Cooperative != null) query.Append($"&coop={((bool)Cooperative ? 1 : 0)}");
			if (WorldRecord != null) query.Append($"&wr={((bool)WorldRecord ? 1 : 0)}");
			if (Banned != null) query.Append($"&banned={((bool)Banned ? 1 : 0)}");
			if (Demo != null) query.Append($"&demo={((bool)Demo ? 1 : 0)}");
			if (YouTube != null) query.Append($"&yt={((bool)YouTube ? 1 : 0)}");
			if (Submission != null) query.Append($"&submission={((bool)Submission ? 1 : 0)}");
			if (MaxDaysAgo != null) query.Append($"&maxDaysAgo={MaxDaysAgo}");
			if (HasDate != null) query.Append($"&hasDate={((bool)HasDate ? 1 : 0)}");

			if (query.Length > 0) query[0] = '?';
			return $"{query}";
		}
	}
}