using ConsoleGUI.UI.Widgets;

namespace ConsoleGUI.UI.Events;

public class InputEventArgs : RoutedEventArgs
{
    public InputEventArgs(Control source, Device device) : base(source) => Device = device;


    internal InputEventArgs(Device device) => Device = device;

    public Device Device { get; protected set; }

    public override void Set(Control source, object inputState)
    {
        Source = source;
        OriginalSource = source;

        Device = inputState switch
        {
            MouseState => Device.MouseDevice,
            KeyboardState => Device.KeyboardDevice,
            _ => throw new ArgumentException("Input state is invalid", nameof(inputState))
        };
    }
}

public enum Device
{
    MouseDevice,
    KeyboardDevice
}
