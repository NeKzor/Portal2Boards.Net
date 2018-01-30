using System;
using System.Diagnostics;
using System.Net;
using Portal2Boards.API;
using Portal2Boards.Extensions;
using Model = Portal2Boards.API.ProfileTimesMapModel;

namespace Portal2Boards
{
	public class MapData : DataScore, IMapData
	{
		public float? DeltaToWorldRecord { get; private set; }
		public int? DeltaToNextRank { get; private set; }

		internal static MapData Create(Model model)
		{
			if (model == null) return default;

			return new MapData()
			{
				Comment = WebUtility.HtmlDecode(model.Note),
				IsSubmission = model.Submission == "1",
				Id = model.ChangelogId,
				PlayerRank = model.PlayerRank,
				ScoreRank = model.ScoreRank,
				Score = model.Score,
				Date = (!string.IsNullOrEmpty(model.Date))
					? DateTime.Parse(model.Date)
					: default,
				DemoExists = model.HasDemo == "1",
				YouTubeId = model.YouTubeId,
				DeltaToWorldRecord = model.WrDiff,
				DeltaToNextRank = model.NextRankDiff
			};
		}
	}
}