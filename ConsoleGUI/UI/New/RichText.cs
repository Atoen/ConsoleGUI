using System.Collections;
using ConsoleGUI.ConsoleDisplay;

namespace ConsoleGUI.UI.New;

public class RichText : Text, IEnumerable<RichTextElement>
{
    public RichText(string text) : base(text)
    {
        RichData = text.Select(symbol => new RichTextElement(symbol, _foreground, _background, _textMode)).ToList();
    }

    public override Color Foreground
    {
        get => _foreground;
        set => _foreground = value;
    }

    public override Color Background
    {
        get => _background;
        set => _background = value;
    }

    public override TextMode TextMode
    {
        get => _textMode;
        set => _textMode = value;
    }

    private Color _foreground = Color.Black;
    private Color _background = Color.Empty;
    private TextMode _textMode = TextMode.Default;


    public List<RichTextElement> RichData { get; }

    public void AppendRich(string text, Color foreground) => AppendRich(text, foreground, Color.Empty);

    public void AppendRich(string text, Color foreground, Color background, TextMode textMode = TextMode.Default)
    {
        var newElements = text.Select(symbol => new RichTextElement(symbol, foreground, Background, TextMode));

        RichData.AddRange(newElements);

        _foreground = foreground;
        _background = background;
        _textMode = textMode;

        Content += text;
    }

    public void SetAllForeground(Color foreground)
    {
        foreach (var element in RichData)
        {
            element.Foreground = foreground;
        }
    }

    public void SetAllBackground(Color background)
    {
        foreach (var element in RichData)
        {
            element.Background = background;
        }
    }

    public void SetAllMode(TextMode textMode)
    {
        foreach (var element in RichData)
        {
            element.TextMode = textMode;
        }
    }

    public void SetAll(Color foreground, Color background, TextMode textMode = TextMode.Default)
    {
        foreach (var element in RichData)
        {
            element.Foreground = foreground;
            element.Background = background;
            element.TextMode = textMode;
        }
    }

    protected override void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
    {
        switch (args.PropertyName)
        {
            case nameof(Content):
                Size = new Vector(((string) args.NewValue!).Length, 1);
                VerifyData();
                break;

            case nameof(MaxSize):
            case nameof(MinSize):
                Size = new Vector(Length, 1);
                break;

            case nameof(Size):
                Parent?.SetProperty("RequestedContentSpace", Size);
                if (Parent is not null) Center = Parent.Center;
                break;
        }
    }

    private void VerifyData()
    {
        if (RichData.Count >= Length) return;

        var difference = Length - RichData.Count;

        for (var i = Length - difference; i < Length; i++)
        {
            RichData.Add(new RichTextElement(Content[i], Foreground, Background, TextMode));
        }
    }

    internal override void Render()
    {
        if (Parent is null || Length == 0) return;

        var sliceLength = Math.Min(Width, Length);

        var parentAllowsOverflow = Parent.GetProperty<bool>("AllowTextOverflow");
        var parentAllowedSpace = Parent.GetProperty<Vector>("InnerSize");

        if (!parentAllowsOverflow)
        {
            if (parentAllowedSpace.Y < 1) return;

            sliceLength = Math.Min(sliceLength, parentAllowedSpace.X);
        }

        Display.PrintRich(Center.X, Center.Y, RichData, Alignment, sliceLength);
    }

    public new IEnumerator<RichTextElement> GetEnumerator() => RichData.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class RichTextElement
{
    public RichTextElement(char symbol, Color foreground, Color background, TextMode textMode)
    {
        Symbol = symbol;
        Foreground = foreground;
        Background = background;
        TextMode = textMode;
    }

    public char Symbol { get; set; }
    public Color Foreground { get; set; }
    public Color Background { get; set; }
    public TextMode TextMode { get; set; }
}