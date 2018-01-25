using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Portal2Boards
{
	public class ChangelogQueryBuilder
	{
		private ChangelogQuery _query;

		public ChangelogQueryBuilder()
			=> _query = new ChangelogQuery();

		public ChangelogQueryBuilder WithMapId(ulong mapId)
		{
			_query.MapId = mapId;
			return this;
		}
		public ChangelogQueryBuilder WithChapter(Chapter chapter)
		{
			_query.Chapter = chapter;
			return this;
		}
		public ChangelogQueryBuilder WithProfileName(string profileName)
		{
			_query.ProfileName = profileName;
			return this;
		}
		public ChangelogQueryBuilder WithProfileId(ulong profileId)
		{
			_query.ProfileId = profileId;
			return this;
		}
		public ChangelogQueryBuilder WithType(uint type)
		{
			_query.Type = type;
			return this;
		}
		public ChangelogQueryBuilder WithSinglePlayer(bool singlePlayer)
		{
			_query.SinglePlayer = singlePlayer;
			return this;
		}
		public ChangelogQueryBuilder WithCooperative(bool cooperative)
		{
			_query.Cooperative = cooperative;
			return this;
		}
		public ChangelogQueryBuilder WithWorldRecord(bool worldRecord)
		{
			_query.WorldRecord = worldRecord;
			return this;
		}
		public ChangelogQueryBuilder WithBanned(bool banned)
		{
			_query.Banned = banned;
			return this;
		}
		public ChangelogQueryBuilder WithDemo(bool demo)
		{
			_query.Demo = demo;
			return this;
		}
		public ChangelogQueryBuilder WithYouTube(bool youTube)
		{
			_query.YouTube = youTube;
			return this;
		}
		public ChangelogQueryBuilder WithSubmission(bool submission)
		{
			_query.Submission = submission;
			return this;
		}
		public ChangelogQueryBuilder WithMaxDaysAgo(uint maxDaysAgo)
		{
			_query.MaxDaysAgo = maxDaysAgo;
			return this;
		}
		public ChangelogQueryBuilder WithHasDate(bool hasDate)
		{
			_query.HasDate = hasDate;
			return this;
		}

		public ChangelogQuery Build()
			=> _query;
	}
}