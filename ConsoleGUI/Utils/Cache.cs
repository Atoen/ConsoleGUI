namespace ConsoleGUI.Utils;

public class Cache<TKey, TValue> : Dictionary<TKey, TValue> where TKey : notnull
{
    public Cache(Func<TKey, TValue> converter) => _converter = converter;

    private readonly Func<TKey, TValue> _converter;

    public TValue GetOrAdd(TKey key)
    {
        if (TryGetValue(key, out var val)) return val;

        var newVal = _converter(key);
        Add(key, newVal);

        return newVal;
    }
}
