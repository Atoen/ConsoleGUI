using ConsoleGUI.UI.New;

namespace ConsoleGUI.UI.Events;

public class MouseEventArgs : InputEventArgs
{
    public MouseEventArgs(Control source, MouseState mouseState) : base(source, Device.MouseDevice)
    {
        LeftButton = (mouseState.Buttons & MouseButton.Left) != 0
            ? MouseButtonState.Pressed
            : MouseButtonState.Released;

        RightButton = (mouseState.Buttons & MouseButton.Right) != 0
            ? MouseButtonState.Pressed
            : MouseButtonState.Released;

        MiddleButton = (mouseState.Buttons & MouseButton.Middle) != 0
            ? MouseButtonState.Pressed
            : MouseButtonState.Released;

        ScrollDirection = mouseState.Wheel switch
        {
            MouseWheelState.Down or MouseWheelState.AnsiDown => ScrollDirection.Down,
            MouseWheelState.Up or MouseWheelState.AnsiUp => ScrollDirection.Up,
            _ => ScrollDirection.None
        };
    }

    public MouseEventArgs() : base(Device.MouseDevice)
    {
    }

    public MouseButtonState LeftButton { get; protected set; }
    public MouseButtonState RightButton { get; protected set; }
    public MouseButtonState MiddleButton { get; protected set; }

    public Vector CursorPosition { get; protected set; }
    public Vector RelativeCursorPosition { get; protected set; }

    public ScrollDirection ScrollDirection { get; protected set; }

    public override void Set(Control source, object inputState)
    {
        OriginalSource = source;
        Source = source;

        var mouseState = (MouseState)inputState;

        CursorPosition = mouseState.Position;
        RelativeCursorPosition = CursorPosition - OriginalSource.GlobalPosition;

        LeftButton = (mouseState.Buttons & MouseButton.Left) != 0
            ? MouseButtonState.Pressed
            : MouseButtonState.Released;

        RightButton = (mouseState.Buttons & MouseButton.Right) != 0
            ? MouseButtonState.Pressed
            : MouseButtonState.Released;

        MiddleButton = (mouseState.Buttons & MouseButton.Middle) != 0
            ? MouseButtonState.Pressed
            : MouseButtonState.Released;

        ScrollDirection = mouseState.Wheel switch
        {
            MouseWheelState.Down or MouseWheelState.AnsiDown => ScrollDirection.Down,
            MouseWheelState.Up or MouseWheelState.AnsiUp => ScrollDirection.Up,
            _ => ScrollDirection.None
        };
    }
}

public enum MouseButtonState
{
    Released,
    Pressed
}

public enum ScrollDirection
{
    Up,
    Down,
    None
}