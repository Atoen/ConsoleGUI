using System.Collections;
using ConsoleGUI.ConsoleDisplay;

namespace ConsoleGUI.UI.Widgets;

[DebuggerDisplay("EntryText {Content}, Fg: {Foreground.ToString()}, Bg: {Background.ToString()}")]
public class EntryText : Text
{
    public EntryText(string text) : base(text)
    {
        _enumerator = Cycle();
        HandlerManager.AddPropertyHandler<EntryText>(nameof(Content), (component, _) => component.DelayCaretBink());
        HandlerManager.AddPropertyHandler<EntryText>(nameof(Content), (_, args) => Console.Title = args.NewValue!.ToString()!);
    }

    protected internal EntryText(Component parent) : this(string.Empty) => Parent = parent;

    public int CaretBlinkingSpeed { get; set; } = 10;
    public bool DisplayingCaret { get; private set; }
    public char Caret { get; set; } = '_';
    public int CaretPosition { get; private set; }

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
        if (CaretPosition == Length)
        {
            Content += symbol;
            CaretPosition++;
            return;
        }

        var span = Content.AsSpan();

        var beforeCaret = span[..CaretPosition];
        var afterCaret = span[CaretPosition..];

        Span<char> result = stackalloc char[Length + 1];
        beforeCaret.CopyTo(result);
        result[beforeCaret.Length] = symbol;
        afterCaret.CopyTo(result[(beforeCaret.Length + 1)..]);

        Content = result.ToString();
        CaretPosition++;
    }

    public void RemoveLast(int n = 1)
    {
        n = Math.Min(Length, n);
        if (n == 0) return;

        Content = Content[..^n];

        if (CaretPosition > 0) CaretPosition -= n;
    }

    public void RemoveAtCaret()
    {
        if (Length == 0 || CaretPosition == Length) return;

        var span = Content.AsSpan();

        var beforeCaret = span[..CaretPosition];
        var afterCaret = span[(CaretPosition + 1)..];

        Span<char> result = stackalloc char[Length - 1];
        beforeCaret.CopyTo(result);
        afterCaret.CopyTo(result[beforeCaret.Length..]);

        Content = result.ToString();
    }

    public void RemoveBeforeCaret()
    {
        if (Length == 0 || CaretPosition == 0) return;

        if (CaretPosition == Length)
        {
            RemoveLast();
            return;
        }

        var span = Content.AsSpan();

        var beforeCaret = span[..(CaretPosition - 1)];
        var afterCaret =span[CaretPosition..];

        Span<char> result = stackalloc char[Length - 1];
        beforeCaret.CopyTo(result);
        afterCaret.CopyTo(result[beforeCaret.Length..]);

        Content = result.ToString();
        CaretPosition--;
    }

    public void MoveCaretLeft()
    {
        if (CaretPosition <= 0) return;

        CaretPosition--;
        ForceCaret();
    }

    public void MoveCaretRight()
    {
        if (CaretPosition >= Length) return;

        CaretPosition++;
        ForceCaret();
    }

    public void ClearContent() => RemoveLast(Length);

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

    internal override void Render()
    {
        if (Parent is null) return;

        if (Animating) _enumerator.MoveNext();

        var visibleSize = GetVisibleSize();
        if (visibleSize is {X: <= 0, Y: <= 0}) return;
        var visibleLength = visibleSize.X;

        Span<char> span = stackalloc char[Length];
        Content.CopyTo(span);

        var visibleCaretPosition = GetVisibleCaretPosition(visibleLength, out var caretShift);
        var visibleSlice = GetVisibleSlice(span, visibleLength, caretShift);

        if (DisplayingCaret)
        {
            // Text occupies all available space - shift caret left to avoid it sticking out of the box
            if (visibleCaretPosition == Parent.GetProperty<int>("InnerWidth"))
            {
                visibleCaretPosition--;
                visibleSlice[visibleCaretPosition] = Caret;
            }

            // Text fits inside
            else
            {
                // make sure that span size matches content (with caret)
                // if the caret is not at the end it should not increase the span length
                var caretSpanLength = CaretPosition == Length ? visibleLength + 1 : visibleLength;
                Span<char> caretSpan = stackalloc char[caretSpanLength];

                visibleSlice.CopyTo(caretSpan);
                caretSpan[visibleCaretPosition] = Caret;

                visibleSlice = caretSpan;
            }
        }

        Display.Print(GlobalPosition.X, GlobalPosition.Y, visibleSlice, Foreground, Background, Alignment, TextMode);
    }

    private int GetVisibleCaretPosition(int visibleLength, out int caretShift)
    {
        var visibleCaretPosition = Math.Min(visibleLength, CaretPosition - (Length - visibleLength));

        caretShift = 0;
        if (visibleCaretPosition < 0)
        {
            caretShift = -visibleCaretPosition;
            visibleCaretPosition = 0;
        }

        return visibleCaretPosition;
    }

    private Span<char> GetVisibleSlice(Span<char> span, int visibleLength, int caretShift)
    {
        if (Parent is Entry {AllowTextScrolling: true})
        {
            var start = span.Length - visibleLength - caretShift;
            var end = span.Length - caretShift;
            return span[start..end];
        }

        return span[..visibleLength];
    }

    public override void Delete()
    {
        _removed = true;
        base.Delete();
    }
}