namespace ConsoleGUI.UI.Events;

internal class EventArgsPool<T> where T : RoutedEventArgs, new()
{
    private readonly Stack<T> _args = new();

    public T Get(Control source, object inputState)
    {
        if (_args.TryPop(out var args))
        {
            args.Set(source, inputState);
            return args;
        }

        var newArgs = new T();
        newArgs.Set(source, inputState);

        return newArgs;
    }

    public void Return(T arg)
    {
        _args.Push(arg);
    }
}
