using ConsoleGUI.ConsoleDisplay;
using ConsoleGUI.UI.Events;
using ConsoleGUI.UI.Old.Widgets;
using ConsoleGUI.Utils;

namespace ConsoleGUI.UI.Widgets;

public class Entry : Control, ITextWidget<EntryText>
{
    public Entry()
    {
        SetHandlers();

        Watermark = Widgets.Text.CreateDefault<Text>(this, "Type here...");
        Watermark.Foreground = Color.Gray;
        Watermark.TextMode = TextMode.Italic;

        Text = Widgets.Text.CreateDefault<EntryText>(this, string.Empty);

        var contentSpace = new Vector(Math.Max(Text.Length, Watermark.Length), 1);
        RequestedContentSpace = contentSpace;
        MinSize = contentSpace with {X = contentSpace.X + InnerPadding.X * 2};

        _setWatermarkSize = true;
    }

    public Text Watermark { get; }
    public bool DisplayWatermark { get; set; } = true;

    public AllowedSymbols InputMode { get; set; } = AllowedSymbols.All;
    public TextMode TextModeWhileTyping { get; set; } = TextMode.Default;

    public Alignment TextAlignment
    {
        get => _textAlignment;
        set => SetField(ref _textAlignment, value);
    }

    public int MaxTextLength { get; set; } = int.MaxValue;
    public bool AllowTextScrolling { get; set; } = true;

    public EntryText Text
    {
        get => _text;
        set => SetField(ref _text, value);
    }

    public bool AllowTextOverflow
    {
        get => _allowTextOverflow;
        set => SetField(ref _allowTextOverflow, value);
    }

    public Vector TextPosition
    {
        get => _textPosition;
        set => SetField(ref _textPosition, value);
    }

    private bool _allowTextOverflow;
    private EntryText _text = null!;
    private Vector _textPosition;

    // Watermark should be able to request resize
    private readonly bool _setWatermarkSize;

    private void SetHandlers()
    {
        void AlignText(Entry component, PropertyChangedEventArgs args)
        {
            TextPosition = TextAlignment switch
            {
                Alignment.Center => component.Center,
                Alignment.Left => component.Center - (component.InnerWidth / 2, 0),
                Alignment.Right => component.Center + (component.InnerWidth / 2, 0),
                _ => throw new ArgumentOutOfRangeException(nameof(TextAlignment))
            };

            component.Text.Alignment = TextAlignment;
            component.Watermark.Alignment = TextAlignment;

            component.Text.GlobalPosition = TextPosition;
            component.Watermark.GlobalPosition = TextPosition;
        }

        var handlers = new PropertyHandlerDefinitionCollection<Entry>
        {
            {(nameof(Position), nameof(GlobalPosition), nameof(Size),
                nameof(Width), nameof(Height), nameof(TextAlignment)), AlignText},
            {nameof(Text), (component, args) => component.ReplaceText(args.As<Text?>())},
        };

        HandlerManager.AddHandlers(handlers);
        HandlerManager.OverridePropertyHandler<Entry>(nameof(RequestedContentSpace), (component, _) =>
        {
            if (component.AllowTextOverflow || !_setWatermarkSize) component.Resize();
        });
    }

    private void ReplaceText(PropertyChangedEventArgs<Text?> args)
    {
        var oldText = args.OldValue;
        var newText = args.NewValue!;

        oldText?.Delete();

        newText.Parent = this;
        newText.Center = Center;

        RequestedContentSpace = newText.Size;
    }

    private bool CheckIfAllowed(char symbol)
    {
        return InputMode switch
        {
            AllowedSymbols.Alphanumeric => char.IsLetterOrDigit(symbol),
            AllowedSymbols.Letters => char.IsLetter(symbol),
            AllowedSymbols.Digits => char.IsDigit(symbol),
            _ => !char.IsControl(symbol)
        };
    }

    private TextMode _textModeBeforeEntry;
    private bool _inEntryMode;
    private Alignment _textAlignment = Alignment.Left;

    private void EnterEntryMode()
    {
        if (_inEntryMode) return;

        _textModeBeforeEntry = Text.TextMode;
        Text.TextMode = TextModeWhileTyping;

        _inEntryMode = true;
        Text.Animating = true;

        if (DisplayWatermark)
        {
            Watermark.Visible = false;
            Text.Visible = true;
        }
    }

    private void ExitEntryMode()
    {
        if (!_inEntryMode) return;

        _inEntryMode = false;
        Text.Animating = false;

        Text.TextMode = _textModeBeforeEntry;

        if (DisplayWatermark && Text.Length == 0)
        {
            Watermark.Visible = true;
            Text.Visible = false;
        }
    }

    private void EnterText(KeyboardEventArgs e)
    {
        switch (e.Key)
        {
            case ConsoleKey.Enter:
                ExitEntryMode();
                return;

            case ConsoleKey.Delete:
                Text.RemoveAtCaret();
                break;

            case ConsoleKey.Backspace:
                Text.RemoveBeforeCaret();
                return;

            case ConsoleKey.LeftArrow:
                Text.MoveCaretLeft();
                return;

            case ConsoleKey.RightArrow:
                Text.MoveCaretRight();
                return;
        }

        var maxLength = MaxTextLength;
        if (!AllowTextOverflow && !AllowTextScrolling)
        {
            maxLength = Math.Min(InnerWidth, MaxTextLength);
        }

        if (CheckIfAllowed(e.Char) && Text.Length < maxLength)
        {
            Text.AppendAtCaret(e.Char);
        }
    }

    protected override void OnKeyDown(KeyboardEventArgs e)
    {
        if (IsFocused && !_inEntryMode && CheckIfAllowed(e.Char)) EnterEntryMode();

        if (_inEntryMode) EnterText(e);

        base.OnKeyDown(e);
    }

    protected override void OnMouseLeftDown(MouseEventArgs e)
    {
        State = State.Pressed;

        if (_inEntryMode) ExitEntryMode();
        else EnterEntryMode();

        base.OnMouseLeftDown(e);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Released) State = State.Highlighted;
        base.OnMouseMove(e);
    }

    protected override void OnMouseEnter(MouseEventArgs e)
    {
        State = State.Highlighted;
        base.OnMouseEnter(e);
    }

    protected override void OnMouseExit(MouseEventArgs e)
    {
        State = State.Default;
        base.OnMouseExit(e);
    }

    protected override void OnLostFocus(InputEventArgs e)
    {
        ExitEntryMode();

        base.OnLostFocus(e);
    }
}

public enum AllowedSymbols
{
    All,
    Alphanumeric,
    Letters,
    Digits
}