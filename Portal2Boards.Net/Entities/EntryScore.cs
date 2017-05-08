namespace Portal2Boards.Net.Entities
{
	public class EntryScore
	{
		public uint? Current { get; set; }
		public uint? Previous { get; set; }
		public uint? Improvement { get; set; }
		public ScoreType Type { get; set; }

		public EntryScore(
			uint? current = default(uint?),
			uint? previous = default(uint?),
			uint? improvement = default(uint?),
			ScoreType type = default(ScoreType))
		{
			Current = current;
			Previous = previous;
			Improvement = improvement;
			Type = type;
		}
	}
}