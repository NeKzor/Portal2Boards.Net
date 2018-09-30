namespace Portal2Boards
{
    public interface IEntity<T>
        where T : struct
    {
        T Id { get; }
    }
}
