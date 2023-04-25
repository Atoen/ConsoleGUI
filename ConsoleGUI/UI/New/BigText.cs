using System.Text;
using ConsoleGUI.ConsoleDisplay;
using ConsoleGUI.Utils;
using ConsoleGUI.Visuals.Figlet;

namespace ConsoleGUI.UI.New;

[DebuggerDisplay("BigText {Content}, Fg: {Foreground.ToString()}, Bg: {Background.ToString()}")]
public class BigText : Text
{
    public BigText(string text) : this(text, Font.Default) { }

    public BigText(string text, Font font) : base(text)
    {
        _font = font;
        _result = new string[Font.Height];

        if (text.Length != 0) GenerateNew();
        
        SetHandlers();
    }

    public Font Font
    {
        get => _font;
        set => SetField(ref _font, value);
    }

    public CharacterWidth CharacterWidth
    {
        get => _characterWidth;
        set => SetField(ref _characterWidth, value);
    }

    private string[] _result;
    private string[] _resultSynced = default!;
    private bool _shouldSwap;

    private readonly StringBuilder _builder = new();
    private Font _font;
    private CharacterWidth _characterWidth = CharacterWidth.Fitted;

    private readonly object _syncRoot = new();

    private void SetHandlers()
    {
        var handlers = new PropertyHandlerDefinitionCollection<BigText>
        {
            {(nameof(Content), nameof(CharacterWidth), nameof(Font)), (component, _) => component.GenerateNew()}
        };
        
        HandlerManager.AddHandlers(handlers);
    }

    private void GenerateNew()
    {
        lock (_syncRoot)
        {
            _result = CharacterWidth switch
            {
                CharacterWidth.Smush => GenerateSmush(),
                CharacterWidth.Fitted => GenerateFitted(),
                CharacterWidth.Full => GenerateFull(),
                _ => throw new ArgumentOutOfRangeException(nameof(CharacterWidth))
            };

            _shouldSwap = true;

            Size = new Vector(_result.Max(line => line.Length), Font.Height);
        }
    }

    private string[] GenerateFitted()
    {
        var result = new string[_font.Height];

        for (var line = 0; line < _font.Height; line++)
        {
            foreach (var symbol in Content)
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
            _builder.Append(Font.GetCharacterLine(Content[0], line));
            var lastChar = Content[0];

            for (var charIndex = 1; charIndex < Length; charIndex++)
            {
                var currentChar = Content[charIndex];
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
            foreach (var symbol in Content)
            {
                _builder.Append(Font.GetCharacterLine(symbol, line));
                _builder.Append(' ');
            }

            result[line] = _builder.ToString();
            _builder.Clear();
        }

        return result;
    }

    private void SwapData()
    {
        lock (_syncRoot)
        {
            _resultSynced = _result;
            _shouldSwap = false;
        }
    }

    internal override void Render()
    {
        if (Parent is null || Length == 0) return;

        if (_shouldSwap) SwapData();
        
        var visibleSize = GetVisibleSize();
        if (visibleSize is {X: <= 0, Y: <= 0}) return;

        for (var i = 0; i < visibleSize.Y; i++)
        {
            var offset = i - visibleSize.Y / 2;
            var slice = _resultSynced[i].AsSpan(0, visibleSize.X);

            Display.Print(Center.X, Center.Y + offset, slice, Foreground, Background, Alignment, TextMode);
        }
    }
}