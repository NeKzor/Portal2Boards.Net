namespace Portal2Boards.Net.Entities
{
	public class EntryRank
	{
		public uint? Current { get; set; }
		public uint? Previous { get; set; }
		public uint? Improvement { get; set; }

		public EntryRank(
			uint? current = default(uint?),
			uint? previous = default(uint?),
			uint? improvement = default(uint?))
		{
			Current = current;
			Previous = previous;
			Improvement = improvement;
		}
	}
}
