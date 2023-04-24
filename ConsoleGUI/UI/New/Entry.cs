using ConsoleGUI.ConsoleDisplay;
using ConsoleGUI.UI.Events;
using ConsoleGUI.Utils;

namespace ConsoleGUI.UI.New;

public class Entry : Control, ITextWidget<EntryText>
{
    public Entry()
    {
        _text = new EntryText(this);
        Watermark = new Text("Text", this)
        {
            Foreground = Color.Gray,
            TextMode = TextMode.Italic
        };
        
        SetHandlers();
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
    private EntryText _text;
    private Vector _textOffset;

    // private void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
    // {
    //     switch (args.PropertyName)
    //     {
    //         case nameof(Text):
    //             ReplaceText(args);
    //             break;
    //
    //         case nameof(Position):
    //         case nameof(GlobalPosition):
    //         case nameof(TextOffset):
    //         case nameof(Size):
    //         case nameof(Width):
    //         case nameof(Height):
    //             Text.Center = Center + TextOffset;
    //             Watermark.Center = Center + TextOffset;
    //             break;
    //     }
    // }

    private void SetHandlers()
    {
        void MoveAction(Entry component, PropertyChangedEventArgs args)
        {
            component.Text.Center = component.Center + component.TextOffset;
            component.Watermark.Center = component.Center + component.TextOffset;
        }

        var handlers = new Dictionary<string, PropertyHandler<Entry>>
        {
            {nameof(Position), MoveAction},
            {nameof(GlobalPosition), MoveAction},
            {nameof(TextOffset), MoveAction},
            {nameof(Size), MoveAction},
            {nameof(Width), MoveAction},
            {nameof(Height), MoveAction},
            {nameof(Text), (component, args) => component.ReplaceText(args)}
        };
        
        HandlerManager.SetHandlers(handlers);

    }

    private void ReplaceText(PropertyChangedEventArgs args)
    {
        var oldText = (Text) args.OldValue!;
        var newText = (Text) args.NewValue!;

        oldText.Delete();

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
        if (_inEntryMode) ExitEntryMode();
        else EnterEntryMode();

        base.OnMouseLeftDown(e);
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