namespace ConsoleGUI.UI.New;

public class PropertyChangedEventArgs : EventArgs
{
    public PropertyChangedEventArgs(string propertyName, object? oldValue, object? newValue)
    {
        PropertyName = propertyName;
        NewValue = newValue;
        OldValue = oldValue;
    }

    public string PropertyName { get; }
    public object? OldValue { get; }
    public object? NewValue { get; }
}