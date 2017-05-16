using System.Collections.Generic;
using System.Linq;

namespace Portal2Boards.Net.Extensions
{
	public static class Portal2
	{
		public static IReadOnlyCollection<Map> CampaignMaps = new List<Map>()
		{
			// TODO: add all maps
			new Map("sp_a1_intro1", "Container Ride", MapType.SinglePlayer, 47458, 47459, 1)
		};
		public static IReadOnlyCollection<Map> SinglePlayerMaps = CampaignMaps.Where(m => m.Type == MapType.SinglePlayer).ToList();
		public static IReadOnlyCollection<Map> CooperativeMaps = CampaignMaps.Where(m => m.Type == MapType.Cooperative).ToList();
		// TODO: write search task

		// TODO: add more stuff here
		public const uint AppId = 620;
		public const string ProtocolVersion = "2001";
		public const string ExeVersion = "2.0.0.1";
		public const string ExeBuild = "6388";
		public const string ExeBuildDate = "12:19:28 May 4 2016";
		public const uint DefaulTickrate = 60;
		public const uint DefaultMaxFps = 300;
	}
}