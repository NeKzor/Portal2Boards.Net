using System.Diagnostics;

namespace Portal2Boards.Net.API.Models
{
	[DebuggerDisplay("{Data.ProfileNumber,nq}")]
	public sealed class Profile : IModel
	{
		public ProfileData Data { get; set; }
		public string ApiRequestUrl { get; internal set; }
		public string RequestUrl
			=> ApiRequestUrl.Substring(0, ApiRequestUrl.IndexOf("/json"));

		public Profile()
		{
		}
		public Profile(ProfileData data, string url)
		{
			Data = data;
			ApiRequestUrl = url;
		}
	}
}