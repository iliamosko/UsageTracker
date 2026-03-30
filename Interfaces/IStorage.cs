namespace UsageTracker.Interfaces;

public interface IStorage<T>
{
    bool Add(T item);
    bool Remove(T item);
    T? Get(T item);
    T? GetByName(string name);
    bool Contains(string name);
    IReadOnlyList<T> GetAll();
}
