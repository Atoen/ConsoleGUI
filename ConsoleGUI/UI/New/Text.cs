using System.Collections;
using ConsoleGUI.ConsoleDisplay;
using ConsoleGUI.UI.Widgets;

namespace ConsoleGUI.UI.New;

[DebuggerDisplay("Text {Content}, Fg: {Foreground.ToString()}, Bg: {Background.ToString()}")]
public class Text : Visual, IText
{
    public Text(string text)
    {
        _content = text;
        Size = new Vector(Length, 1);

        PropertyChanged += OnPropertyChanged;
    }

    protected internal Text(string text, Component parent) : this(text) => Parent = parent;

    public virtual Color Foreground { get; set; } = Color.Black;
    public virtual Color Background { get; set; } = Color.Empty;

    public virtual TextMode TextMode { get; set; } = TextMode.Default;
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

    private readonly Dictionary<string, Action<PropertyChangedEventArgs>> _propertyChangedDelegates = new()
    {
        { nameof(Content), args => Console.Title = args.PropertyName }
    };

    protected virtual void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
    {
        // switch (args.PropertyName)
        // {
        //     case nameof(Content):
        //         Size = new Vector(((string) args.NewValue!).Length, 1);
        //         break;
        //
        //     case nameof(MaxSize):
        //     case nameof(MinSize):
        //         Size = new Vector(Length, 1);
        //         break;
        //
        //     case nameof(Size):
        //         Parent?.SetProperty("RequestedContentSpace", Size);
        //         if (Parent is not null) Center = Parent.Center;
        //         break;
        //     
        //     case nameof(Position):
        //     case nameof(GlobalPosition):
        //         ClearOnMove((Vector) args.OldValue! - (Vector) args.NewValue!);
        //         break;
        // }

        if (_propertyChangedDelegates.TryGetValue(args.PropertyName, out var handler))
        {
            handler.Invoke(args);
        }
    }

    internal override void Render()
    {
        if (Parent is null || Length == 0) return;

        var visibleSize = GetVisibleSize();
        if (visibleSize.Y == 0) return;
        
        var span = Content.AsSpan(0, visibleSize.X);

        Display.Print(Center.X, Center.Y, span, Foreground, Background, Alignment, TextMode);
    }

    protected Vector GetVisibleSize()
    {
        var visibleSize = Size;

        var parentAllowsOverflow = Parent!.GetProperty<bool>("AllowTextOverflow");

        if (!parentAllowsOverflow)
        {
            var parentAllowedSpace = Parent!.GetProperty<Vector>("InnerSize");
            
            visibleSize.X = Math.Min(visibleSize.X, parentAllowedSpace.X);
            visibleSize.Y = Math.Min(visibleSize.Y, parentAllowedSpace.Y);
        }

        return visibleSize;
    }

    internal override void Clear()
    {
        var startPos = Alignment switch
        {
            Alignment.Left => GlobalPosition,
            Alignment.Right => new Vector(GlobalPosition.X - Width, GlobalPosition.Y),
            _ => new Vector(GlobalPosition.X - Width / 2, GlobalPosition.Y)
        };

        Display.ClearRect(startPos, Size);
    }

    public override void Delete()
    {
        Parent = null;
        base.Delete();
    }

    public IEnumerator<char> GetEnumerator() => Content.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString() => Content;
}