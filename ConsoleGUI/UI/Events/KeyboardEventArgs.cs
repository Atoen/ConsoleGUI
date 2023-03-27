namespace ConsoleGUI.UI.Events;

public class KeyboardEventArgs : InputEventArgs
{
    public KeyboardEventArgs(Control source, KeyboardState keyboardState) : base(source, Device.KeyboardDevice)
    {
        Key = keyboardState.Key;
        Char = keyboardState.Char;
        IsPressed = keyboardState.Pressed;
        IsReleased = !IsPressed;
    }

    public KeyboardEventArgs() : base(Device.KeyboardDevice)
    {
    }

    public ConsoleKey Key { get; protected set; }
    public char Char { get; protected set; }
    public bool IsReleased { get; protected set; }
    public bool IsPressed { get; protected set; }

    public override void Set(Control source, object inputState)
    {
        OriginalSource = source;
        Source = source;

        var keyboardState = (KeyboardState)inputState;

        Key = keyboardState.Key;
        Char = keyboardState.Char;
        IsPressed = keyboardState.Pressed;
        IsReleased = !IsPressed;
    }
}
