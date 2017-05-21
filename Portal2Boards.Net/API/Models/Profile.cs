using System.Diagnostics;

namespace Portal2Boards.Net.API.Models
{
	[DebuggerDisplay("{Data.ProfileNumber,nq}")]
	public sealed class Profile : IModel
	{
		public ProfileData Data { get; set; }
		public string RequestUrl { get; internal set; }

		public Profile()
		{
		}
		public Profile(ProfileData data, string url)
		{
			Data = data;
			RequestUrl = url;
		}
	}
}