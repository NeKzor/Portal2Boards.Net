namespace Portal2Boards
{
    public class EntryPoints : IEntryData
    {
        public uint? Current { get; private set; }
        public uint? Previous { get; private set; }
        public int? Improvement { get; private set; }

        internal static EntryPoints Create(
            uint? current = default,
            uint? previous = default,
            int? improvement = default)
        {
            return new EntryPoints()
            {
                Current = current,
                Previous = previous,
                Improvement = improvement
            };
        }
    }
}
