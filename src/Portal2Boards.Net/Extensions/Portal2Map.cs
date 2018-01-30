using System;
using System.Linq;

namespace Portal2Boards.Extensions
{
	public class Portal2Map
	{
		public static uint Count = 0;

		public uint Index;
		public string Name { get; set; }
		public string Alias { get; set; }
		public Portal2MapType Type { get; set; }
		public ulong? BestTimeId { get; set; }
		public ulong? BestPortalsId { get; set; }
		public uint? ChapterId { get; set; }

		public bool IsOfficial
			=> ((Type == Portal2MapType.SinglePlayer) || Type == (Portal2MapType.Cooperative))
				&& (BestTimeId != default)
				&& (BestPortalsId != default);
		public bool Exists
			=> ((Type == Portal2MapType.SinglePlayer) || Type == (Portal2MapType.Cooperative))
				&& (BestTimeId != default);

		public string Url
			=> $"https://board.iverb.me/chamber/{BestTimeId}";
		public string ImageUrl
			=> $"https://board.iverb.me/images/chambers/{BestTimeId}.jpg";
		public string ImageFullUrl
			=> $"https://board.iverb.me/images/chambers_full/{BestTimeId}.jpg";
		public string BestTimeSteamUrl
			=> $"https://steamcommunity.com/stats/Portal2/leaderboards/{BestTimeId}";
		public string BestPortalsSteamUrl
			=> $"https://steamcommunity.com/stats/Portal2/leaderboards/{BestPortalsId}";

		public Portal2Map(
			string name = default,
			string alias = default,
			Portal2MapType type = default,
			uint? bestTimeId = default,
			uint? bestPortalsId = default,
			uint? chapterId = default)
		{
			Name = name;
			Alias = alias;
			Type = type;
			BestTimeId = bestTimeId;
			BestPortalsId = bestPortalsId;
			ChapterId = chapterId;
			Index = Count;
			Count++;
		}

		public static Portal2Map Search(
			string name,
			StringComparison comparison = StringComparison.CurrentCultureIgnoreCase)
		{
			return Portal2.CampaignMaps
				.FirstOrDefault(m => string.Equals(m.Name, name, comparison) || string.Equals(m.Alias, name, comparison));
		}
		public static Portal2Map Search(ulong id)
		{
			return Portal2.CampaignMaps
				.FirstOrDefault(m => (m.BestTimeId == id) || (m.BestPortalsId == id));
		}
	}
}