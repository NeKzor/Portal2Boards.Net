namespace Portal2Boards
{
    public interface IEntryData
	{
		uint? Current { get; }
		uint? Previous { get; }
		uint? Improvement { get; }
	}
}