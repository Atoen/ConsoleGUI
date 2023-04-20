using ConsoleGUI.ConsoleDisplay;
using ConsoleGUI.UI.Events;
using ConsoleGUI.UI.New;

namespace ConsoleGUI.UI.Widgets;

public class TextBox : ContentOldControl
{
    public TextBox()
    {
        _text = new TextBoxText("") { Parent = this };
        Watermark.Parent = this;
    }

    public Text Watermark { get; } = new("Enter Text", Color.Gray)
    {
        TextMode = TextMode.Italic,
        Alignment = Alignment.Left
    };

    public bool DisplayWatermark { get; set; } = true;

    public AllowedSymbols InputMode { get; set; } = AllowedSymbols.All;
    public TextMode TextModeWhileTyping { get; set; } = TextMode.Italic;
    public Color TextBlockBackground { get; set; } = Color.White;

    public int MaxLineLength { get; set; } = 20;
    public int MaxTextLength { get; set; } = 200;

    public bool ReadOnly { get; set; }

    private bool _inEntryMode;

    private TextBoxText _text;
    public TextBoxText Text
    {
        get => _text;
        set
        {
            _text.Remove();
            _text.Parent = null!;

            _text = value;
            _text.Parent = this;
        }
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

    protected override void OnKeyDown(KeyboardEventArgs e)
    {
        if (Focused && !_inEntryMode && CheckIfAllowed(e.Char)) EnterEntryMode();

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

    private void EnterEntryMode()
    {
        if (ReadOnly) return;

        _inEntryMode = true;
        Text.Animating = true;
        Text.TextMode = TextModeWhileTyping;
    }

    private void ExitEntryMode()
    {
        _inEntryMode = false;
        Text.Animating = false;
        Text.TextMode = TextMode.Default;
    }

    private void EnterText(KeyboardEventArgs e)
    {
        if (e.Key == ConsoleKey.Enter)
        {
            if (e.Modifiers == KeyModifiers.Shift) Text.AddLine();
            else ExitEntryMode();

            return;
        }

        if (CheckIfAllowed(e.Char))
        {
            if (Text.Length >= MaxLineLength) Text.AddLine();
            Text.Append(e.Char);

            return;
        }

        if (e.Key == ConsoleKey.Backspace)
        {
            if (Text.Length > 0) Text.RemoveLast();
            else if (Text.Lines.Count > 0) Text.RemoveLine();
        }
    }

    internal override void Resize()
    {
        MinSize = InnerPadding * 2 + (MaxLineLength + 1, Text.Lines.Count + 1);

        ApplyResizing();

        base.Resize();
    }

    public override void Remove()
    {
        _text.Remove();
        Watermark.Remove();
        base.Remove();
    }

    public override void Clear()
    {
        _text.Clear();
        Watermark.Clear();
        base.Clear();
    }

    public override void Render()
    {
        base.Render();

        var textBlockStart = GlobalPosition + InnerPadding;
        Display.DrawRect(textBlockStart, InnerSize, TextBlockBackground);

        if (Text.String == string.Empty && DisplayWatermark && !_inEntryMode)
        {
            Watermark.Position = textBlockStart;

            Text.Visible = false;
            Watermark.Visible = true;
        }
        else
        {
            Text.Visible = true;
            Watermark.Visible = false;
        }
    }
}