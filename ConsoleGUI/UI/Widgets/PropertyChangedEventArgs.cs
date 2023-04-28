namespace ConsoleGUI.UI.Widgets;

public class PropertyChangedEventArgs : EventArgs
{
    public PropertyChangedEventArgs(object? oldValue, object? newValue)
    {
        NewValue = newValue;
        OldValue = oldValue;
    }
    
    public object? OldValue { get; }
    public object? NewValue { get; }

    public PropertyChangedEventArgs<T> As<T>() => new((T) OldValue!, (T) NewValue!);
}

public class PropertyChangedEventArgs<T>
{
    public PropertyChangedEventArgs(T oldValue, T newValue)
    {
        OldValue = oldValue;
        NewValue = newValue;
    }

    public T OldValue { get; }
    public T NewValue { get; }
}

