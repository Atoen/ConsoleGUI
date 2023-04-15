using ConsoleGUI.ConsoleDisplay;
using ConsoleGUI.UI.Widgets;

namespace ConsoleGUI.UI.New;

[DebuggerDisplay("Text {String}, Fg: {Foreground.ToString()}, Bg: {Background.ToString()}")]
public class Text : VisualComponent
{
    public Text(string text)
    {
        _string = text;
        Size = new Vector(Length, 1);
    }

    public Color Foreground { get; set; } = Color.Black;
    public Color Background { get; set; } = Color.Empty;

    public TextMode TextMode { get; set; } = TextMode.Default;
    public Alignment Alignment { get; set; } = Alignment.Center;

    public string String
    {
        get => _string;
        set => SetField(ref _string, value, onChanged: OnStringChanged);
    }

    private void OnStringChanged(string oldValue, string newValue)
    {
        Size = new Vector(newValue.Length, 1);
    }

    public int Length => String.Length;

    public override bool Visible
    {
        get
        {
            if (Parent is null) return false;
            return _visible && Parent.GetProperty<bool>(nameof(Visible));
        }
        set => _visible = value;
    }

    private bool _visible = true;
    private string _string;

    internal override void Render()
    {
        if (Parent is null) return;

        var shouldTrim = !Parent.GetProperty<bool>("AllowTextOverflow");

        ReadOnlySpan<char> displaySpan;

        if (shouldTrim)
        {
            var allowedSpace = Parent.GetProperty<Vector>("InnerSize");
            if (allowedSpace.X < 1 || allowedSpace.Y < 1) return;
            
            displaySpan = String.AsSpan(0, Math.Min(allowedSpace.X, Length));
        }
        else
        {
            displaySpan = String.AsSpan();
        }
        

        var position = Parent.Center;

        Display.Print(position.X, position.Y, displaySpan, Foreground, Background, Alignment);
    }

    internal override void Clear()
    {
        var startPos = Alignment switch
        {
            Alignment.Left => GlobalPosition,
            Alignment.Right => new Vector(GlobalPosition.X - Length, GlobalPosition.Y),
            _ => new Vector(GlobalPosition.X - Length / 2, GlobalPosition.Y)
        };

        Display.ClearRect(startPos, Size);
    }

    public override void Delete()
    {
        Parent = null;
        base.Delete();
    }

    public override string ToString() => String;
}