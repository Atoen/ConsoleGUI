namespace ConsoleGUI.UI.Widgets;

public class PropertyChangedEventArgs : PropertyChangedEventArgs<object?>
{
    public PropertyChangedEventArgs(object? oldValue, object? newValue) : base(oldValue, newValue)
    {
    }

    public PropertyChangedEventArgs<T> As<T>() => new((T) OldValue!, (T) NewValue!);
}

public class PropertyChangedEventArgs<T> : EventArgs
{
    public PropertyChangedEventArgs(T oldValue, T newValue)
    {
        OldValue = oldValue;
        NewValue = newValue;
    }

    public T OldValue { get; }
    public T NewValue { get; }

    public static implicit operator PropertyChangedEventArgs(PropertyChangedEventArgs<T> args) => new(args.OldValue, args.NewValue);
}

