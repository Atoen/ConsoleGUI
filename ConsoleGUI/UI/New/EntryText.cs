using System.Collections;
using ConsoleGUI.ConsoleDisplay;

namespace ConsoleGUI.UI.New;

[DebuggerDisplay("EntryText {Content}, Fg: {Foreground.ToString()}, Bg: {Background.ToString()}")]
public class EntryText : Text
{
    public EntryText(string text) : base(text)
    {
        _enumerator = Cycle();
        
        SetHandlers();
    }

    protected internal EntryText(Component parent) : this(string.Empty) => Parent = parent;

    public int CaretBlinkingSpeed { get; set; } = 10;
    public bool DisplayingCaret { get; private set; }
    public char Caret { get; set; } = '_';
    public int CaretPosition { get; set; }

    public bool Animating
    {
        get => _animating;
        set
        {
            if (!value) DisplayingCaret = false;
            _animating = value;
        }
    }

    private readonly IEnumerator _enumerator;
    private bool _removed;
    private bool _animating;

    public void AppendAtCaret(string text)
    {
        var contentBefore = Content;
        Content += text;

        CaretPosition += contentBefore == string.Empty ? text.Length - 1 : text.Length;
    }

    public void AppendAtCaret(char symbol)
    {
        var contentBefore = Content;
        Content += symbol;
        
        if (contentBefore != string.Empty) CaretPosition++;
    }

    public void RemoveLast(int n = 1)
    {
        n = Math.Min(Length, n);
        if (n == 0) return;
        
        Content = Content[..^n];
        
        if (CaretPosition > 0) CaretPosition -= n;
    }

    public void ClearContent() => Content = string.Empty;

    private void DelayCaretBink()
    {
        _cycleState = 0;
        DisplayingCaret = false;
    }

    public void ForceCaret()
    {
        _cycleState = 0;
        DisplayingCaret = true;
    }

    private void SetCaretPosition()
    {
        CaretPosition = Math.Max(0, Length - 1);
    }

    private int _cycleState;
    private IEnumerator Cycle()
    {
        while (!_removed)
        {
            if (++_cycleState >= CaretBlinkingSpeed)
            {
                DisplayingCaret = !DisplayingCaret;
                _cycleState = 0;
            }

            yield return null;
        }
    }

    private void SetHandlers()
    {
        HandlerManager.AddPropertyHandler<EntryText>(nameof(Content), (component, _) => component.DelayCaretBink());
    }

    internal override void Render()
    {
        if (Parent is null || Length == 0) return;

        if (Animating) _enumerator.MoveNext();

        var visibleSize = GetVisibleSize();
        if (visibleSize is {X: <= 0, Y: <= 0}) return;

        Span<char> span = stackalloc char[Length];
        Content.CopyTo(span);

        var visibleSlice = Parent is Entry && Parent.GetProperty<bool>("AllowTextScrolling")
            ? span[^visibleSize.X..]
            : span[..visibleSize.X];

        if (DisplayingCaret) visibleSlice[CaretPosition] = Caret;

        Display.Print(Center.X, Center.Y, visibleSlice, Foreground, Background, Alignment, TextMode);
    }

    public override void Delete()
    {
        _removed = true;
        base.Delete();
    }
}