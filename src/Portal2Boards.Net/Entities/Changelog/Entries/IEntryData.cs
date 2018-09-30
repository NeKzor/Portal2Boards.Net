namespace Portal2Boards
{
    public interface IEntryData
    {
        uint? Current { get; }
        uint? Previous { get; }
        int? Improvement { get; }
    }
}
