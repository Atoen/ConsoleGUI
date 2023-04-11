using System.Collections;
using System.Text;
using ConsoleGUI.ConsoleDisplay;

namespace ConsoleGUI.UI.Widgets;

public class TextBoxText : Text
{
    public TextBoxText(string text) : this(text, Color.Black) { }

    public TextBoxText(string text, Color foreground) : this(text, foreground, Color.Empty) { }

    public TextBoxText(string text, Color foreground, Color background) : base(text, foreground, background)
    {
        _enumerator = Cycle();
    }

    public char Caret { get; set; } = '_';
    public int CaretCycleSpeed { get; set; } = 10;

    public bool Animating
    {
        get => _animating;
        set
        {
            if (!value) _displayingCaret = false;
            _animating = value;
        }
    }

    public List<string> Lines { get; } = new();

    public override string String
    {
        get
        {
            _builder.Append(string.Join(' ', Lines));
            _builder.Append(TextInternal);

            var result = _builder.ToString();
            _builder.Clear();

            return result;
        }
    }

    public string CurrentLine
    {
        get => TextInternal;
        set => TextInternal = value;
    }

    public int MaxLineLength { get; set; } = 20;

    private bool _displayingCaret;
    private bool _animating;
    private readonly IEnumerator _enumerator;
    private readonly StringBuilder _builder = new();

    public void Append(string text) => TextInternal += text;

    public void Append(char symbol) => TextInternal += symbol;

    public void RemoveLast(int n = 1) => TextInternal = TextInternal[..^n];

    public void AddLine()
    {
        Lines.Add(TextInternal);
        TextInternal = string.Empty;

        Size = (20, Lines.Count + 1);
    }

    public void RemoveLine()
    {
        var line = Lines[^1];
        Lines.Remove(line);

        TextInternal = line;

        Size = (20, Lines.Count + 1);
    }

    private IEnumerator Cycle()
    {
        var i = 0;

        while (Enabled)
        {
            i++;

            if (i >= CaretCycleSpeed)
            {
                _displayingCaret = !_displayingCaret;
                i = 0;
            }

            yield return null;
        }
    }

    public void ClearText()
    {
        Lines.Clear();
        TextInternal = string.Empty;

        Size = (20, Lines.Count + 1);
    }

    public override void Render()
    {
        if (Parent == null) return;

        if (Animating) _enumerator.MoveNext();

        var position = Parent.GlobalPosition + Parent.InnerPadding;
        var posX = position.X;
        if (Length % 2 != 0 && Animating) posX--;

        var offset = 0;
        foreach (var t in Lines)
        {
            Display.Print(position.X, position.Y + offset, t, Foreground, Background, Alignment, TextMode);
            offset++;
        }

        Display.Print(posX, position.Y + offset, TextInternal, Foreground, Background, Alignment, TextMode);

        if (_displayingCaret)
        {
            var caretX = Alignment switch
            {
                Alignment.Left => position.X,
                Alignment.Right => position.X - Length + 1,
                _ => position.X - (Length + 1) / 2
            };

            Display.Draw(caretX + TextInternal.Length, position.Y + offset, Caret, Foreground, Background);
        }
    }
}