using ConsoleGUI.UI.New;
using ConsoleGUI.UI.Widgets;
using ConsoleGUI.Visuals;
using Microsoft.Win32.SafeHandles;
using static ConsoleGUI.NativeConsole;

namespace ConsoleGUI.ConsoleDisplay;

public sealed class NativeDisplay : IRenderer
{
    private bool _modified;

    private readonly SafeFileHandle _fileHandle;
    private readonly SCoord _startPos = new();

    private SCoord _displaySize;
    private DisplayRect _screenRect;

    private CharInfo[] _currentBuffer;
    private CharInfo[] _lastBuffer;

    public NativeDisplay(int width, int height)
    {
        _fileHandle = CreateFile("CONOUT$",
            0x40000000,
            2,
            nint.Zero,
            FileMode.Open,
            0,
            nint.Zero);

        if (_fileHandle.IsInvalid) throw new IOException("Console buffer file is invalid");

        _displaySize = new SCoord { X = (short)width, Y = (short)height };
        _screenRect = new DisplayRect { Left = 0, Top = 0, Right = (short)width, Bottom = (short)height };

        _currentBuffer = new CharInfo[width * height];
        _lastBuffer = new CharInfo[width * height];
    }

    public void Draw(int posX, int posY, char symbol, Color fg, Color bg)
    {
        if (posX < 0 || posX >= _displaySize.X || posY < 0 || posY >= _displaySize.Y) return;

        var bufferIndex = posY * _displaySize.X + posX;

        var color = Colors.CombineToShort(fg, bg);

        _currentBuffer[bufferIndex].Symbol = symbol;
        _currentBuffer[bufferIndex].Color = color;
    }

    public void DrawRect(Vector start, Vector end, Color color, char symbol = ' ')
    {
        var consoleColor = (short) (color.ConsoleColorIndex() << 4);

        for (var x = start.X; x < end.X; x++)
        for (var y = start.Y; y < end.Y; y++)
        {
            var bufferIndex = y * _displaySize.X + x;

            _currentBuffer[bufferIndex].Symbol = symbol;
            _currentBuffer[bufferIndex].Color = consoleColor;
        }
    }

    public void DrawLine(Vector pos, Vector direction, int length, Color fg, Color bg, char symbol)
    {
        var consoleColor = Colors.CombineToShort(fg, bg);
        var distance = 0;

        while (distance < length)
        {
            var bufferIndex = pos.Y * _displaySize.X + pos.X;

            _currentBuffer[bufferIndex].Symbol = symbol;
            _currentBuffer[bufferIndex].Color = consoleColor;

            pos += direction;
            distance++;
        }
    }

    public void Print(int posX, int posY, ReadOnlySpan<char> text, Color fg, Color bg, Alignment alignment, TextMode _)
    {
        if (posY < 0 || posY >= _displaySize.Y) return;

        var startX = alignment switch
        {
            Alignment.Left => posX,
            Alignment.Right => posX - text.Length,
            _ => posX - text.Length / 2
        };

        if (startX < 0) startX = 0;
        if (startX >= _displaySize.X) startX = _displaySize.X - 1;

        var endX = startX + text.Length;

        if (endX >= _displaySize.X) endX = _displaySize.X - 1;

        var color = Colors.CombineToShort(fg, bg);

        var bufferIndex = posY * _displaySize.X + startX;

        for (var i = 0; i < endX - startX; i++)
        {
            _currentBuffer[bufferIndex + i].Color = color;
            _currentBuffer[bufferIndex + i].Symbol = text[i];
        }
    }

    public void PrintRich(int posX, int posY, IList<RichTextElement> data, Alignment alignment, int length)
    {
        
    }

    public void DrawBuffer(Vector start, Vector end, Pixel[,] buffer)
    {
        for (var x = start.X; x < end.X; x++)
        for (var y = start.Y; y < end.Y; y++)
        {
            var color = Colors.CombineToShort(buffer[x, y].Fg, buffer[x, y].Bg);
            var bufferIndex = y * _displaySize.X + x;
            var pixel = buffer[x, y];

            _currentBuffer[bufferIndex].Symbol = pixel.Symbol;
            _currentBuffer[bufferIndex].Color = color;
        }
    }

    public void DrawBorder(Vector pos, Vector size, Color color, BorderStyle style, bool fitsHorizontally,
        bool fitsVertically)
    {
    }


    public void ClearAt(int posX, int posY)
    {
        if (posX < 0 || posX >= _displaySize.X || posY < 0 || posY >= _displaySize.Y) return;

        var bufferIndex = posY * _displaySize.X + posX;

        _currentBuffer[bufferIndex] = CharInfo.Empty;
    }

    public void ClearRect(Vector start, Vector end)
    {
        for (var x = start.X; x < end.X; x++)
        for (var y = start.Y; y < end.Y; y++)
        {
            var bufferIndex = y * _displaySize.X + x;

            _currentBuffer[bufferIndex] = CharInfo.Empty;
        }
    }

    public void Draw()
    {
        CopyToBuffer();

        if (!_modified) return;

        WriteConsoleOutput(_fileHandle, _currentBuffer, _displaySize, _startPos, ref _screenRect);

        _modified = false;
    }

    public void ResetStyle() => Console.ResetColor();

    private unsafe void CopyToBuffer()
    {
        fixed (CharInfo* current = _currentBuffer, last = _lastBuffer)
        {
            for (var i = 0; i < _displaySize.X * _displaySize.Y; i++)
            {
                if (current[i].Symbol == last[i].Symbol && current[i].Color == last[i].Color) continue;

                _modified = true;
                Array.Copy(_currentBuffer, _lastBuffer, _displaySize.X * _displaySize.Y);

                return;
            }
        }
    }

    public void Clear() => _modified = true;

    public void ResizeBuffer(Vector newBufferSize)
    {
        _currentBuffer = new CharInfo[newBufferSize.X * newBufferSize.Y];
        _lastBuffer = new CharInfo[newBufferSize.X * newBufferSize.Y];

        _displaySize.X = (short)newBufferSize.X;
        _displaySize.Y = (short)newBufferSize.Y;

        _screenRect.Right = (short)newBufferSize.X;
        _screenRect.Bottom = (short)newBufferSize.Y;
    }
}
