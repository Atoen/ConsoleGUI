using ConsoleGUI.ConsoleDisplay;
using ConsoleGUI.UI.Widgets;

namespace ConsoleGUI.UI;

public class Text : VisualComponent
{
    public Text(string text) : this(text, Color.Black)
    {
        Background = Color.Empty;
    }

    public Text(string text, Color foreground)
    {
        ArgumentNullException.ThrowIfNull(text);

        if (text.Contains(Environment.NewLine))
        {
            throw new ArgumentException("Text cannot contain newline character");
        }

        TextInternal = text;
        Size = new Vector(Length, 1);

        Foreground = foreground;
        Background = Color.Empty;

        ZIndexUpdateMode = ZIndexUpdateMode.OneHigherThanParent;
    }

    public Text(string text, Color foreground, Color background) : this(text, foreground)
    {
        Background = background;
    }

    public override bool ShouldRender => Visible && Parent is {Enabled: true};

    public Color Foreground { get; set; }
    public Color Background
    {
        get => DefaultColor;
        set => DefaultColor = value;
    }

    public TextMode TextMode { get; set; } = TextMode.Default;
    public Alignment Alignment { get; set; } = Alignment.Center;

    protected string TextInternal;

    public virtual string String
    {
        get => TextInternal;
        set
        {
            TextInternal = value;
            Size = new Vector(Length, 1);
        }
    }

    public int Length => TextInternal.Length;

    public override void Render()
    {
        if (Parent == null) return;

        var position = Parent.Center;

        Display.Print(position.X, position.Y, TextInternal, Foreground, Background, Alignment, TextMode);
    }

    public override void Clear()
    {
        var startPos = Alignment switch
        {
            Alignment.Left => GlobalPosition,
            Alignment.Right => new Vector(GlobalPosition.X - Length, GlobalPosition.Y),
            _ => new Vector(GlobalPosition.X - Length / 2, GlobalPosition.Y)
        };

        Display.ClearRect(startPos, Size);
    }

    public override string ToString() => TextInternal;
}
