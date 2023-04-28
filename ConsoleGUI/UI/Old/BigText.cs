using System.Text;
using ConsoleGUI.ConsoleDisplay;
using ConsoleGUI.Visuals.Figlet;

namespace ConsoleGUI.UI.Old;

public class BigText : Text
{
    public BigText(string text, Font? font = null, CharacterWidth characterWidth = CharacterWidth.Fitted) : base(text,
        Color.Black)
    {
        _characterWidth = characterWidth;
        _font = font ?? Font.Default;

        Result = new string[_font.Height];

        if (text.Length == 0) return;

        GenerateNew();
    }

    public string[] Result { get; private set; }

    public Font Font
    {
        get => _font;
        set
        {
            if (value == _font) return;

            _font = value;
            GenerateNew();
        }
    }

    public CharacterWidth CharacterWidth
    {
        get => _characterWidth;
        set
        {
            if (value == _characterWidth) return;

            _characterWidth = value;
            GenerateNew();
        }
    }

    public override string String
    {
        get => TextInternal;
        set
        {
            if (value == TextInternal) return;

            TextInternal = value;
            GenerateNew();
        }
    }

    private readonly StringBuilder _builder = new();
    private Font _font;
    private CharacterWidth _characterWidth;

    private void GenerateNew()
    {
        Result = CharacterWidth switch
        {
            CharacterWidth.Smush => GenerateSmush(),
            CharacterWidth.Fitted => GenerateFitted(),
            CharacterWidth.Full => GenerateFull(),
            _ => throw new ArgumentOutOfRangeException(nameof(CharacterWidth))
        };

        Size = new Vector(Result.Max(line => line.Length), Font.Height);
    }

    private string[] GenerateFitted()
    {
        var result = new string[_font.Height];

        for (var line = 0; line < _font.Height; line++)
        {
            foreach (var symbol in TextInternal)
            {
                _builder.Append(Font.GetCharacterLine(symbol, line));
            }

            result[line] = _builder.ToString();
            _builder.Clear();
        }

        return result;
    }

    private string[] GenerateSmush()
    {
        var result = new string[_font.Height];

        for (var line = 0; line < _font.Height; line++)
        {
            _builder.Append(Font.GetCharacterLine(TextInternal[0], line));
            var lastChar = TextInternal[0];

            for (var charIndex = 1; charIndex < Length; charIndex++)
            {
                var currentChar = TextInternal[charIndex];
                var currentCharacterLine = Font.GetCharacterLine(currentChar, line);

                if (lastChar != ' ' && currentChar != ' ')
                {
                    if (_builder[^1] == ' ')
                    {
                        _builder[^1] = currentCharacterLine[0];
                    }

                    _builder.Append(currentCharacterLine[1..]);
                }
                else
                {
                    _builder.Append(currentCharacterLine);
                }

                lastChar = currentChar;
            }

            result[line] = _builder.ToString();
            _builder.Clear();
        }

        return result;
    }

    private string[] GenerateFull()
    {
        var result = new string[_font.Height];

        for (var line = 0; line < _font.Height; line++)
        {
            foreach (var symbol in TextInternal)
            {
                _builder.Append(Font.GetCharacterLine(symbol, line));
                _builder.Append(' ');
            }

            result[line] = _builder.ToString();
            _builder.Clear();
        }

        return result;
    }

    public override void Render()
    {
        if (Parent == null) return;

        var position = Parent.Center;

        for (var i = 0; i < Height; i++)
        {
            var offset = i - Height / 2;

            Display.Print(position.X, position.Y + offset, Result[i], Foreground, Background, Alignment, TextMode);
        }
    }
}