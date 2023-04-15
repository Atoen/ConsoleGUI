using ConsoleGUI.ConsoleDisplay;
using ConsoleGUI.UI.Widgets;

namespace ConsoleGUI.UI.New;

[DebuggerDisplay("Text {Content}, Fg: {Foreground.ToString()}, Bg: {Background.ToString()}")]
public class Text : VisualComponent
{
    public Text(string text)
    {
        _content = text;
        Size = new Vector(Length, 1);

        PropertyChanged += OnPropertyChanged;
    }

    public Color Foreground { get; set; } = Color.Black;
    public Color Background { get; set; } = Color.Empty;

    public TextMode TextMode { get; set; } = TextMode.Default;
    public Alignment Alignment { get; set; } = Alignment.Center;

    public string Content
    {
        get => _content;
        set => SetField(ref _content, value);
    }

    public int Length => Content.Length;

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
    private string _content;
    private int _visualLength;

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
    {
        switch (args.PropertyName)
        {
            case nameof(Content):
                Size = new Vector(((string) args.NewValue!).Length, 1);
                break;

            case nameof(MaxSize):
            case nameof(MinSize):
                Size = new Vector(Length, 1);
                break;

            case nameof(Size):
                _visualLength = Width;
                Parent?.SetProperty("RequestedContentSpace", Size);
                if (Parent is not null) Center = Parent.Center;
                break;
        }
    }

    internal override void Render()
    {
        if (Parent is null) return;

        var displaySpan = Content.AsSpan();

        var sliceLength = Math.Min(_visualLength, Length);

        var parentAllowsOverflow = Parent.GetProperty<bool>("AllowTextOverflow");
        var parentAllowedSpace = Parent.GetProperty<Vector>("InnerSize");

        if (!parentAllowsOverflow)
        {
            if (parentAllowedSpace.Y < 1) return;

            sliceLength = Math.Min(sliceLength, parentAllowedSpace.X);
        }

        var visibleSlice = displaySpan[..sliceLength];

        Display.Print(Center.X, Center.Y, visibleSlice, Foreground, Background, Alignment);
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

    public override string ToString() => Content;
}