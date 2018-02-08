namespace Portal2Boards
{
	public class EntryScore : IEntryData, IEntryScore
	{
		public uint? Current { get; private set; }
		public uint? Previous { get; private set; }
		public int? Improvement { get; private set; }
		public ScoreType Type { get; private set; }

		internal static EntryScore Create(
			uint? current = default,
			uint? previous = default,
			int? improvement = default,
			ScoreType type = default)
		{
			return new EntryScore()
			{
				Current = current,
				Previous = previous,
				Improvement = improvement,
				Type = type
			};
		}
	}
}