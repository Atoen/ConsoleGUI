using ConsoleGUI.UI;
using ConsoleGUI.UI.Events;
using static ConsoleGUI.NativeConsole;

namespace ConsoleGUI;

public static class Input
{
    public static bool TreatControlCAsInput
    {
        get => Console.TreatControlCAsInput;
        set => Console.TreatControlCAsInput = value;
    }

    internal static event EventHandler<SCoord>? WindowEvent;
    public static event EventHandler? ControlC;

    private static volatile bool _running;
    private static MouseButton _lastMouseButton = MouseButton.None;

    private static readonly MouseState MouseState = new();
    private static readonly KeyboardState KeyboardState = new();

    private static readonly List<Control> Controls = new();
    private static readonly ReaderWriterLockSlim LockSlim = new();

    private static readonly EventArgsPool<MouseEventArgs> MouseArgsPool = new();
    private static readonly EventArgsPool<KeyboardEventArgs> KeyboardArgsPool = new();

    public static void Init()
    {
        if (_running) return;
        _running = true;

        TreatControlCAsInput = true;

        var inHandle = HandleIn;

        var mode = 0u;
        GetConsoleMode(inHandle, ref mode);

        mode &= ~EnableQuickEditMode;
        mode |= EnableWindowInput;
        mode |= EnableMouseInput;

        SetConsoleMode(inHandle, mode);

        new Thread(HandleInput)
        {
            Name = "Inupt Thread"
        }.Start();
    }

    internal static void Register(Control control)
    {
        LockSlim.EnterWriteLock();
        Controls.Add(control);
        LockSlim.ExitWriteLock();
    }

    internal static void Unregister(Control control)
    {
        LockSlim.EnterWriteLock();
        Controls.Remove(control);
        LockSlim.ExitWriteLock();
    }

    private static void HandleInput()
    {
        try
        {
            MainLoop();
        }
        catch (Exception exception)
        {
            Application.Exit(exception);

            Throw(exception);
        }
    }

    [Conditional("DEBUG")]
    private static void Throw(Exception exception) => throw exception;

    private static void MainLoop()
    {
        var handleIn = GetStdHandle(StdHandleIn);
        var recordArray = new[] {new InputRecord()};

        while (_running)
        {
            uint numRead = 0;
            ReadConsoleInput(handleIn, recordArray, 1, ref numRead);

            var record = recordArray[0];

            switch (record.EventType)
            {
                case MouseEventCode:
                    MouseState.Assign(ref record.MouseEventRecord);
                    HandleMouse();
                    break;

                case KeyEventCode:
                    KeyboardState.Assign(ref record.KeyEventRecord);
                    HandleKeyboard();
                    break;

                case WindowBufferSizeEvent:
                    WindowEvent?.Invoke(null, record.WindowBufferSizeEventRecord.Size);
                    break;
            }
        }
    }

    private static void HandleMouse()
    {
        var zIndex = int.MinValue;
        Control? hit = null;

        var pos = MouseState.Position;

        LockSlim.EnterWriteLock();

        foreach (var control in Controls)
        {
            if (!control.HitTestVisible) continue;

            if (control.ContainsPoint(pos) && control.ZIndex > zIndex)
            {
                // Previously marked as hit - now detected something on higher layer blocking the cursor
                if (hit != null) Miss(hit, MouseState.Buttons);

                zIndex = control.ZIndex;
                hit = control;
            }
            else
            {
                Miss(control, MouseState.Buttons);
            }
        }

        LockSlim.ExitWriteLock();

        if (hit == null) return;

        var args = MouseArgsPool.Get(hit, MouseState);

        SendMouseEvents(hit, args);

        _lastMouseButton = MouseState.Buttons;

        MouseArgsPool.Return(args);
    }

    private static void Miss(Control control, MouseButton button)
    {
        MouseEventArgs? args = null;

        if (control.IsMouseOver)
        {
            args = MouseArgsPool.Get(control, MouseState);
            control.SendMouseEvent(MouseEventType.MouseExit, args);
        }

        if (control.Focused && (button & MouseButton.Left) != 0)
        {
            args ??= MouseArgsPool.Get(control, MouseState);
            control.SendFocusEvent(FocusEventType.LostFocus, args);
        }

        if (args != null) MouseArgsPool.Return(args);
    }

    private static void SendMouseEvents(Control control, MouseEventArgs args)
    {
        if ((MouseState.Flags & MouseEventFlags.DoubleClicked) != 0) control.SendMouseEvent(MouseEventType.DoubleClick, args);

        if (args.ScrollDirection != ScrollDirection.None) control.SendMouseEvent(MouseEventType.MouseScroll, args);

        switch (_lastMouseButton)
        {
            case MouseButton.None:
                SendMouseDown(control, args);
                break;

            case MouseButton.Left when args.LeftButton == MouseButtonState.Released:
                control.SendMouseEvent(MouseEventType.MouseLeftUp, args);
                break;

            case MouseButton.Right when args.RightButton == MouseButtonState.Released:
                control.SendMouseEvent(MouseEventType.MouseRightUp, args);
                break;
        }

        if (!control.IsMouseOver) control.SendMouseEvent(MouseEventType.MouseEnter, args);

        control.SendMouseEvent(MouseEventType.MouseMove, args);
    }

    private static void SendMouseDown(Control control, MouseEventArgs args)
    {
        if (args.LeftButton == MouseButtonState.Pressed)
        {
            control.SendMouseEvent(MouseEventType.MouseLeftDown, args);

            if (control.Focusable) control.SendFocusEvent(FocusEventType.GotFocus, args);
        }

        if (args.RightButton == MouseButtonState.Pressed)
        {
            control.SendMouseEvent(MouseEventType.MouseRightDown, args);
        }

        if (args.MiddleButton == MouseButtonState.Pressed)
        {
            control.SendMouseEvent(MouseEventType.MouseMiddleDown, args);
        }
    }

    private static void HandleKeyboard()
    {
        if ((KeyboardState.Modifiers & KeyModifiers.LeftControl) != 0 && KeyboardState is { Key: ConsoleKey.C, Pressed: true })
        {
            ControlC?.Invoke(null, EventArgs.Empty);
        }

        LockSlim.EnterWriteLock();

        foreach (var control in Controls)
        {
            if (!control.Focused) continue;

            SendKeyboardEvent(control);
            break;
        }

        LockSlim.ExitWriteLock();
    }

    private static void SendKeyboardEvent(Control control)
    {
        var args = KeyboardArgsPool.Get(control, KeyboardState);
        args.Set(control, KeyboardState);

        control.SendKeyboardEvent(KeyboardState.Pressed ? KeyboardEventType.KeyDown : KeyboardEventType.KeyUp, args);

        KeyboardArgsPool.Return(args);
    }

    public static void Stop() => _running = false;
}

public sealed class KeyboardState
{
    public ConsoleKey Key;
    public char Char;
    public bool Pressed;
    public KeyModifiers Modifiers;

    public void Assign(ref KeyEventRecord record)
    {
        Key = (ConsoleKey)record.VirtualKeyCode;
        Pressed = record.KeyDown;
        Char = record.UnicodeChar;
        Modifiers = (KeyModifiers)record.ControlKeyState;
    }
}

public sealed class MouseState
{
    public Vector Position;
    public MouseButton Buttons;
    public MouseEventFlags Flags;
    public MouseWheelState Wheel;

    public void Assign(ref MouseEventRecord record)
    {
        Position = new Vector(record.MousePosition.X, record.MousePosition.Y);
        Buttons = (MouseButton) record.ButtonState;
        Wheel = (MouseWheelState) record.ButtonState;
        Flags = (MouseEventFlags) record.EventFlags;
    }
}

[Flags]
public enum MouseButton : byte
{
    None = 0,
    Left = 1,
    Right = 2,
    Middle = 4
}

[Flags]
public enum MouseEventFlags : byte
{
    Moved = 1,
    DoubleClicked = 2,
    Wheeled = 4,
    HorizontalWheeled = 8
}

public enum MouseWheelState : ulong
{
    Up = 0xff880000,
    AnsiUp = 0xff800000,
    Down = 0x780000,
    AnsiDown = 0x800000
}

[Flags]
public enum KeyModifiers : byte
{
    None = 0,
    RightAlt = 1,
    LeftAlt = 2,
    RightControl = 4,
    LeftControl = 8,
    Shift = 16
}
