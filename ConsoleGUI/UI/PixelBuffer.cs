using System.Collections;
using ConsoleGUI.ConsoleDisplay;
using ConsoleGUI.Utils;

namespace ConsoleGUI.UI;

public class PixelBuffer : IEnumerable<Pixel>
{
    public PixelBuffer(int width, int height)
    {
        Data = new Pixel[width, height];

        _size.X = width;
        _size.Y = height;
    }

    public bool KeepContentWhenResizing { get; set; } = true;
    public Pixel[,] Data { get; private set; }
    public Vector Size => _size;
    private Vector _size;

    public ref Pixel this[int x, int y] => ref Data[x, y];
    public ref Pixel this[Vector pos] => ref Data[pos.X, pos.Y];

    public void Resize(Vector newSize, Color colorToFill)
    {
        var newBuffer = new Pixel[newSize.X, newSize.Y];

        for (var x = 0; x < newSize.X; x++)
        for (var y = 0; y < newSize.Y; y++)
        {
            if (KeepContentWhenResizing && x < _size.X && y < _size.Y)
            {
                newBuffer[x, y] = Data[x, y];
            }
            else
            {
                newBuffer[x, y].Bg = colorToFill;
                newBuffer[x, y].Symbol = ' ';
            }
        }

        Data = newBuffer;
        _size = newSize;
    }

    public void Fill(Color foreground, Color background)
    {
        for (var x = 0; x < _size.X; x++)
        for (var y = 0; y < _size.Y; y++)
        {
            Data[x, y].Fg = foreground;
            Data[x, y].Bg = background;
            Data[x, y].Symbol = ' ';
        }
    }

    public IEnumerator<Pixel> GetEnumerator() => new Enumerator(this);

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public struct Enumerator : IEnumerator<Pixel>
    {
        public Enumerator(PixelBuffer buffer)
        {
            _buffer = buffer;
            _x = -1;
            _y = 0;
        }

        private readonly PixelBuffer _buffer;
        private int _x;
        private int _y;

        public bool MoveNext()
        {
            _x++;
            if (_x < _buffer.Size.X) return _y < _buffer.Size.Y;
            
            _x = 0.RoundTo(1);
            _y++;

            return _y < _buffer.Size.Y;
        }

        public void Reset()
        {
            _x = -1;
            _y = 0;
        }

        public Pixel Current => _buffer[_x, _y];

        object IEnumerator.Current => Current;

        public void Dispose()
        {
        }
    }
}