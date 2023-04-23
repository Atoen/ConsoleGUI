using System.Collections;
using ConsoleGUI.ConsoleDisplay;

namespace ConsoleGUI.UI.New;

[DebuggerDisplay("RichText {Content}, Fg: {Foreground.ToString()}, Bg: {Background.ToString()}")]
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
    
    public List<RichTextElement> RichData { get; }

    private Color _foreground = Color.Black;
    private Color _background = Color.Empty;
    private TextMode _textMode = TextMode.Default;

    private object _syncRoot = new();
    private bool _shouldSwap;
    private List<RichTextElement> _syncedData = default!;



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
        if (RichData.Count == Length) return;

        lock (_syncRoot)
        {
            var difference = Length - RichData.Count;
            if (difference > 0)
            {
                for (var i = RichData.Count; i < Length; i++)
                {
                    RichData.Add(new RichTextElement(Content[i], Foreground, Background, TextMode));
                }
            }

            else
            {
                difference = -difference;
            
                for (var i = 0; i < Content.Length; i++)
                {
                    var symbol = Content[i];
                    if (RichData[i].Symbol == symbol) continue;

                    RichData[i] = new RichTextElement(symbol, Foreground, Background, TextMode);
                }
        
                RichData.RemoveRange(RichData.Count - difference, difference);
            }

            _shouldSwap = true;
        }
    }

    private void SwapData()
    {
        lock (_syncRoot)
        {
            _syncedData = RichData;
            _shouldSwap = false;
        }
    }

    internal override void Render()
    {
        if (Parent is null || Length == 0) return;

        if (_shouldSwap) SwapData();
        
        var visibleSize = GetVisibleSize();
        if (visibleSize.Y == 0) return;

        Display.PrintRich(Center.X, Center.Y, _syncedData, Alignment, visibleSize.X);
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