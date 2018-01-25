namespace Portal2Boards
{
	public class EntryRank : IEntryData
	{
		public uint? Current { get; set; }
		public uint? Previous { get; set; }
		public uint? Improvement { get; set; }

		internal static EntryRank Create(
			uint? current = default,
			uint? previous = default,
			uint? improvement = default)
		{
			return new EntryRank()
			{
				Current = current,
				Previous = previous,
				Improvement = improvement
			};
		}
	}
}
