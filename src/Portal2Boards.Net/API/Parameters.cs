using System.Collections;
using System.Linq;
using System.Reflection;

namespace Portal2Boards.API
{
	public class Parameters : IEnumerable
	{
		public static Parameter MapId = new Parameter("chamber");
		public static Parameter ChapterId = new Parameter("chapter");
		public static Parameter PlayerName = new Parameter("boardName");
		public static Parameter SteamId = new Parameter("profileNumber");
		public static Parameter Type = new Parameter("type");
		public static Parameter SinglePlayer = new Parameter("sp");
		public static Parameter Cooperative = new Parameter("coop");
		public static Parameter WorldRecord = new Parameter("wr");
		public static Parameter Banned = new Parameter("banned");
		public static Parameter Demo = new Parameter("demo");
		public static Parameter YouTube = new Parameter("yt");
		public static Parameter Submission = new Parameter("submission");
		public static Parameter MaxDaysAgo = new Parameter("maxDaysAgo");
		public static Parameter HasDate = new Parameter("hasDate");
		public static Parameter EntryId = new Parameter("id");

		public IEnumerator GetEnumerator()
			=> GetType()
				.GetRuntimeFields()
				.Select(p => p.GetValue(this))
				.GetEnumerator();
	}
}