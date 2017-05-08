namespace Portal2Boards.Net.Entities
{
	public class EntryPoints
	{
		public uint? Current { get; set; }
		public uint? Previous { get; set; }
		public uint? Improvement { get; set; }

		public EntryPoints(
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