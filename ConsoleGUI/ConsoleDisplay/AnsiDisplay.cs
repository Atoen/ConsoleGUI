using System.Collections.ObjectModel;
using System.Text;
using ConsoleGUI.UI.New;
using ConsoleGUI.UI.Widgets;
using ConsoleGUI.Utils;
using ConsoleGUI.Visuals;

namespace ConsoleGUI.ConsoleDisplay;

public sealed class AnsiDisplay : IRenderer
{
    private bool _modified = true;

    private readonly StringBuilder _stringBuilder = new();
    private readonly StringBuilder _symbolsBuilder = new();

    private Pixel[,] _currentPixels;
    private Pixel[,] _lastPixels;

    private readonly Func<Color, string> _foregroundConverter = color => $"\x1b[38;2;{color.R};{color.G};{color.B}m";
    private readonly Func<Color, string> _backgroundConverter = color => $"\x1b[48;2;{color.R};{color.G};{color.B}m";
    private readonly Func<Vector, string> _coordConverter = coord => $"\x1b[{coord.Y + 1};{coord.X + 1}f";

    private readonly Cache<Color, string> _foregroundColorCache;
    private readonly Cache<Color, string> _backgroundColorCache;
    private readonly Cache<Vector, string> _coordCache;

    private readonly TextWriter _textWriter = Console.Out;

    private bool _shouldClear;

    public AnsiDisplay(int width, int height)
    {
        _currentPixels = new Pixel[width, height];
        _lastPixels = new Pixel[width, height];

        _foregroundColorCache = new Cache<Color, string>(_foregroundConverter);
        _backgroundColorCache = new Cache<Color, string>(_backgroundConverter);
        _coordCache = new Cache<Vector, string>(_coordConverter);
    }

    public void Draw(int posX, int posY, char symbol, Color fg, Color bg)
    {
        if (posX < 0 || posX >= Display.Width || posY < 0 || posY >= Display.Height) return;

        _currentPixels[posX, posY].Symbol = symbol;
        _currentPixels[posX, posY].Mode = TextMode.Default;
        
        if (fg != Color.Empty) _currentPixels[posX, posY].Fg = fg;
        if (bg != Color.Empty)_currentPixels[posX, posY].Bg = bg;
    }

    public void DrawRect(Vector start, Vector end, Color color, char symbol = ' ')
    {
        if (color == Color.Empty) return;

        for (var x = start.X; x < end.X; x++)
        for (var y = start.Y; y < end.Y; y++)
        {
            _currentPixels[x, y].Symbol = symbol;
            _currentPixels[x, y].Mode = TextMode.Default;
            _currentPixels[x, y].Fg = Color.Empty;
            _currentPixels[x, y].Bg = color;
        }
    }

    public void DrawLine(Vector pos, Vector direction, int length, Color fg, Color bg, char symbol)
    {
        var distance = 0;

        while (distance < length)
        {
            _currentPixels[pos.X, pos.Y].Symbol = symbol;
            _currentPixels[pos.X, pos.Y].Mode = TextMode.Default;

            if (fg != Color.Empty) _currentPixels[pos.X, pos.Y].Fg = fg;
            if (bg != Color.Empty) _currentPixels[pos.X, pos.Y].Bg = bg;

            pos += direction;
            distance++;
        }
    }

    public void Print(int posX, int posY, ReadOnlySpan<char> text, Color fg, Color bg, Alignment alignment,
        TextMode mode)
    {
        if (posY < 0 || posY >= Display.Height) return;

        var startX = alignment switch
        {
            Alignment.Left => posX,
            Alignment.Right => posX - text.Length,
            _ => posX - text.Length / 2
        };

        if (startX >= Display.Width) return;

        var endX = startX + text.Length;
        if (endX >= Display.Width) endX = Display.Width - 1;

        var firstLetterOffset = 0;

        if (startX < 0)
        {
            firstLetterOffset = -startX;
            startX = 0;
        }

        for (int x = startX, i = firstLetterOffset; x < endX; x++, i++)
        {
            _currentPixels[x, posY].Symbol = text[i];
            _currentPixels[x, posY].Mode = mode;

            if (fg != Color.Empty) _currentPixels[x, posY].Fg = fg;
            if (bg != Color.Empty) _currentPixels[x, posY].Bg = bg;
        }
    }

    public void PrintRich(int posX, int posY, IList<RichTextElement> data,
        Alignment alignment, int length)
    {
        if (posY < 0 || posY >= Display.Height) return;

        var startX = alignment switch
        {
            Alignment.Left => posX,
            Alignment.Right => posX - length,
            _ => posX - length / 2
        };

        if (startX >= Display.Width) return;

        var endX = startX + length;
        if (endX >= Display.Width) endX = Display.Width - 1;

        var firstLetterOffset = 0;

        if (startX < 0)
        {
            firstLetterOffset = -startX;
            startX = 0;
        }

        for (int x = startX, i = firstLetterOffset; x < endX; x++, i++)
        {
            var dataTuple = data[i];
            
            _currentPixels[x, posY].Symbol = dataTuple.Symbol;
            _currentPixels[x, posY].Mode = dataTuple.TextMode;
            
            if (dataTuple.Foreground != Color.Empty) _currentPixels[x, posY].Fg = dataTuple.Foreground;
            if (dataTuple.Background != Color.Empty) _currentPixels[x, posY].Bg = dataTuple.Background;
        }
    }
    
    public void DrawBuffer(Vector start, Vector end, Pixel[,] buffer)
    {
        for (var x = start.X; x < end.X; x++)
        for (var y = start.Y; y < end.Y; y++)
        {
            _currentPixels[x, y] = buffer[x - start.X, y - start.Y];
        }
    }

    public void DrawBorder(Vector start, Vector end, Color color, BorderStyle style,
        bool fitsHorizontally, bool fitsVertically)
    {
        if (color == Color.Empty) return;

        for (var x = start.X + 1; x < end.X - 1; x++)
        {
            _currentPixels[x, start.Y].Symbol = Border.Symbols[style][BorderFragment.Horizontal];
            _currentPixels[x, start.Y].Fg = color;

            _currentPixels[x, end.Y - 1].Symbol = Border.Symbols[style][BorderFragment.Horizontal];
            _currentPixels[x, end.Y - 1].Fg = color;
        }

        for (var y = start.Y + 1; y < end.Y - 1; y++)
        {
            _currentPixels[start.X, y].Symbol = Border.Symbols[style][BorderFragment.Vertical];
            _currentPixels[start.X, y].Fg = color;

            _currentPixels[end.X - 1, y].Symbol = Border.Symbols[style][BorderFragment.Vertical];
            _currentPixels[end.X - 1, y].Fg = color;
        }

        _currentPixels[start.X, start.Y].Symbol = Border.Symbols[style][BorderFragment.UpperLeft];
        _currentPixels[start.X, start.Y].Fg = color;

        if (fitsHorizontally)
        {
            _currentPixels[end.X - 1, start.Y].Symbol = Border.Symbols[style][BorderFragment.UpperRight];
            _currentPixels[end.X - 1, start.Y].Fg = color;
        }

        if (fitsVertically)
        {
            _currentPixels[start.X, end.Y - 1].Symbol = Border.Symbols[style][BorderFragment.LowerLeft];
            _currentPixels[start.X, end.Y - 1].Fg = color;
        }

        if (!fitsHorizontally || !fitsVertically) return;

        _currentPixels[end.X - 1, end.Y - 1].Symbol = Border.Symbols[style][BorderFragment.LowerRight];
        _currentPixels[end.X - 1, end.Y - 1].Fg = color;
    }

    public void ClearAt(int posX, int posY)
    {
        if (posX < 0 || posX >= Display.Width || posY < 0 || posY >= Display.Height) return;

        _currentPixels[posX, posY] = Pixel.Cleared;
    }

    public void ClearRect(Vector start, Vector end)
    {
        for (var x = start.X; x < end.X; x++)
        for (var y = start.Y; y < end.Y; y++)
        {
            _currentPixels[x, y] = Pixel.Cleared;
        }
    }

    public void Draw()
    {
        CopyToBuffer();
        if (!_modified) return;

        GenerateDisplayString();

        _modified = false;
    }

    public void ResetStyle() => Console.Write("\x1b[0m");

    public void Clear()
    {
        _modified = true;
        _shouldClear = true;
    }

    public void ResizeBuffer(Vector newBufferSize)
    {
        _currentPixels = new Pixel[newBufferSize.X, newBufferSize.Y];
        _lastPixels = new Pixel[newBufferSize.X, newBufferSize.Y];
    }

    private void CopyToBuffer()
    {
        for (var x = 0; x < Display.Width; x++)
        for (var y = 0; y < Display.Height; y++)
        {
            if (_currentPixels[x, y] == _lastPixels[x, y]) continue;

            _modified = true;
            Array.Copy(_currentPixels, _lastPixels, Display.Width * Display.Height);

            return;
        }
    }

    private readonly ReadOnlyDictionary<TextMode, string> _ansiTextModes = new(
        new Dictionary<TextMode, string>
        {
            {TextMode.Default, "\x1B[0m"},
            {TextMode.Bold, "\x1B[1m"},
            {TextMode.Italic, "\x1B[3m"},
            {TextMode.Underline, "\x1B[4m"},
            {TextMode.DoubleUnderline, "\x1B[21m"},
            {TextMode.Overline, "\x1B[53m"},
            {TextMode.Strikethrough, "\x1B[9m"}
        });

    private void AppendTextMode(TextMode mode, StringBuilder builder)
    {
        if (mode == TextMode.Default)
        {
            builder.Append(_ansiTextModes[TextMode.Default]);

            return;
        }

        foreach (var textMode in _ansiTextModes.Keys)
        {
            if ((mode & textMode) != 0)
            {
                builder.Append(_ansiTextModes[textMode]);
            }
        }
    }

    private void GenerateDisplayString()
    {
        // Current printing values
        var currentFg =  _currentPixels[0, 0].Fg;
        var currentBg = _currentPixels[0, 0].Bg;
        var currentMode = _currentPixels[0, 0].Mode;

        // starting position for printing the gathered pixel symbols
        var streakStartPos = new Vector();
        var oldStreakPos = new Vector();
        var oldStreakLen = 0;
        var previousIsCleared = false;

        if (_shouldClear)
        {
            _stringBuilder.Append("\x1b[2J");
            _shouldClear = false;
        }

        _stringBuilder.Append("\x1b[1;1f");

        var lastLine = 0;

        for (var y = 0; y < Display.Height; y++)
        for (var x = 0; x < Display.Width; x++)
        {
            var newLine = y != lastLine;
            lastLine = y;

            var pixel = _currentPixels[x, y];
            var pixelPropertiesChanged = pixel.Fg != currentFg || pixel.Bg != currentBg ||
                                         pixel.Mode != currentMode ||
                                         (previousIsCleared && pixel.IsEmpty);

            // Printing the already gathered pixels if next one has different visual properties
            if (pixelPropertiesChanged || newLine)
            {
                if (_symbolsBuilder.Length != 0)
                {
                    // Need to specify new coords for printing
                    if (oldStreakPos.Y != y || oldStreakPos.X + oldStreakLen != streakStartPos.X)
                    {
                        _stringBuilder.Append(_coordCache.GetOrAdd(streakStartPos));
                    }

                    AppendTextMode(currentMode, _stringBuilder);

                    // Resetting the colors to clear the pixels
                    if (previousIsCleared)
                    {
                        _stringBuilder.Append("\x1b[0m");
                    }

                    // Applying the colors for gathered pixels
                    else
                    {
                        _stringBuilder.Append(_foregroundColorCache.GetOrAdd(currentFg));
                        _stringBuilder.Append(_backgroundColorCache.GetOrAdd(currentBg));
                    }

                    // Starting new streak of pixels
                    oldStreakLen = _symbolsBuilder.Length;
                    oldStreakPos = streakStartPos;

                    _stringBuilder.Append(_symbolsBuilder);
                    _symbolsBuilder.Clear();
                }

                currentFg = pixel.Fg;
                currentBg = pixel.Bg;
                currentMode = pixel.Mode;
            }

            // Setting the start pos of the collected pixel symbols when collecting the first one
            if (_symbolsBuilder.Length == 0)
            {
                streakStartPos.X = x;
                streakStartPos.Y = y;
            }

            // Collecting the pixels with same colors together
            if (!pixel.IsEmpty) _symbolsBuilder.Append(pixel.Symbol);

            previousIsCleared = pixel.IsCleared;

            // Marking the pixel as empty to not draw it again unnecessarily
            if (pixel.IsCleared) _currentPixels[x, y] = Pixel.Empty;
        }

        // If the screen buffer ends while symbolsBuilder still has unprinted content
        if (_symbolsBuilder.Length > 0)
        {
            var lastPixel = _currentPixels[Display.Width - 1, Display.Height - 1];

            _stringBuilder.Append(_coordCache.GetOrAdd(streakStartPos));

            AppendTextMode(currentMode, _stringBuilder);

            _stringBuilder.Append(_foregroundColorCache.GetOrAdd(lastPixel.Fg));
            _stringBuilder.Append(_backgroundColorCache.GetOrAdd(lastPixel.Bg));

            _stringBuilder.Append(_symbolsBuilder);

            _symbolsBuilder.Clear();
        }

        // Resetting the console style after full draw
        _stringBuilder.Append("\x1b[0m");

        var dataRead = 0;

        Span<char> span = stackalloc char[1000];
        while (dataRead < _stringBuilder.Length)
        {
            var chunkSize = Math.Min(_stringBuilder.Length - dataRead, 1000);
            _stringBuilder.CopyTo(dataRead, span, chunkSize);

            _textWriter.Write(span);
            span.Clear();
            dataRead += chunkSize;
        }

        _stringBuilder.Clear();
    }
}
