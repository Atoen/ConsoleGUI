using ConsoleGUI.ConsoleDisplay;
using ConsoleGUI.UI.Events;
using ConsoleGUI.Utils;

namespace ConsoleGUI.UI.New;

public class Entry : Control, ITextWidget<EntryText>
{
    public Entry()
    {
        SetHandlers();

        Watermark = New.Text.CreateDefault<Text>(this, "Type here...");
        Watermark.Foreground = Color.Gray;
        Watermark.TextMode = TextMode.Italic;

        Text = New.Text.CreateDefault<EntryText>(this, string.Empty);

        var contentSpace = new Vector(Math.Max(Text.Length, Watermark.Length), 1);
        RequestedContentSpace = contentSpace;
        MinSize = contentSpace with {X = contentSpace.X + InnerPadding.X * 2};
    }

    public Text Watermark { get; }
    public bool DisplayWatermark { get; set; } = true;

    public AllowedSymbols InputMode { get; set; } = AllowedSymbols.All;
    public TextMode TextModeWhileTyping { get; set; } = TextMode.Italic;

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

    public Vector TextOffset
    {
        get => _textOffset;
        set => SetField(ref _textOffset, value);
    }

    private bool _allowTextOverflow;
    private EntryText _text = null!;
    private Vector _textOffset;

    private void SetHandlers()
    {
        void CenterText(Entry component, PropertyChangedEventArgs args)
        {
            var center = component.Center + component.TextOffset;
            component.Text.Center = center;
            component.Watermark.Center = center;
        }

        var handlers = new PropertyHandlerDefinitionCollection<Entry>
        {
            {(nameof(Position), nameof(GlobalPosition), nameof(TextOffset), nameof(Size), nameof(Width), nameof(Height)), CenterText},
            {nameof(Text), (component, args) => component.ReplaceText(args)}
        };

        HandlerManager.AddHandlers(handlers);
    }

    private void ReplaceText(PropertyChangedEventArgs args)
    {
        var oldText = (Text?) args.OldValue;
        var newText = (Text) args.NewValue!;

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

    private void MoveCaret()
    {

    }

    private void EnterText(KeyboardEventArgs e)
    {
        switch (e.Key)
        {
            case ConsoleKey.Enter:
                ExitEntryMode();
                return;

            case ConsoleKey.Backspace:
                Text.RemoveLast();
                return;

            case ConsoleKey.LeftArrow:
                Text.CaretPosition--;
                Text.ForceCaret();
                return;

            case ConsoleKey.RightArrow:
                Text.CaretPosition++;
                Text.ForceCaret();
                return;
        }


        int maxLength;
        if (AllowTextOverflow || AllowTextScrolling)
        {
            maxLength = MaxTextLength;
        }
        else
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