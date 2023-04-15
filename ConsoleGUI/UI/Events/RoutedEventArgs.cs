using ConsoleGUI.UI.New;

namespace ConsoleGUI.UI.Events;

public class RoutedEventArgs : EventArgs
{
    public RoutedEventArgs(Control source)
    {
        OriginalSource = source;
        Source = source;
    }

    internal RoutedEventArgs()
    {
    }

    public bool Handled { get; set; }
    public Control Source { get; set; } = null!;
    public Control OriginalSource { get; protected set; } = null!;

    public virtual void Set(Control source, object inputState)
    {
        Source = source;
        OriginalSource = source;
    }
}
