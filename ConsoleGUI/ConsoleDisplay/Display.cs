using System.Runtime.InteropServices;
using System.Text;
using ConsoleGUI.UI;
using ConsoleGUI.UI.Widgets;
using ConsoleGUI.Utils;
using ConsoleGUI.Visuals;
using VisualComponent = ConsoleGUI.UI.New.VisualComponent;

namespace ConsoleGUI.ConsoleDisplay;

public static class Display
{
    public static Vector Size => ScreenResizer.ScreenSize;
    public static int Width => ScreenResizer.ScreenWidth;
    public static int Height => ScreenResizer.ScreenHeight;
    public static DisplayMode Mode { get; private set; }

    public static int RefreshRate
    {
        get => _refreshRate;
        set
        {
            if (value < 1) throw new ArgumentException("Refresh rate must be greater than 0", nameof(RefreshRate));

            _refreshRate = value;
            _frameTime = 1000 / value;
        }
    }

    private static int _refreshRate = 20; // Hz
    private static int _frameTime = 1000 / 20;
    private static volatile bool _refreshing;

    private static readonly List<IRenderable> Renderables = new();
    private static readonly List<IRenderable> RemovedRenderables = new();
    
    private static readonly List<VisualComponent> Visuals = new();
    private static readonly List<VisualComponent> VisualsToRemove = new();

    private static IRenderer _renderer = null!;
    private static readonly bool ShouldTryToResizeConsoleBuffer = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    private static readonly ReaderWriterLockSlim LockSlim = new();

    public static void Init(DisplayMode mode = DisplayMode.Auto)
    {
        Console.Clear();
        Console.CursorVisible = false;
        Console.OutputEncoding = Encoding.UTF8;

        ScreenResizer.Init();

        Input.WindowEvent += WindowResize;

        Mode = mode;

        if (Mode == DisplayMode.Auto) GetDisplayMode();

        if (Mode == DisplayMode.Native)
        {
            ScreenResizer.BufferSizeStep = 2;
            _renderer = new NativeDisplay(Width, Height);
        }
        else _renderer = new AnsiDisplay(Width, Height);

        new Thread(Start)
        {
            Name = "Display Thread"
        }.Start();
    }

    private static void WindowResize(object? sender, NativeConsole.SCoord size)
    {
        Console.CursorVisible = false;
        _renderer.Clear();
    }

    public static void Stop() => _refreshing = false;

    internal static void AddToRenderList(IRenderable renderable)
    {
        renderable.ZIndexChanged += RenderableOnZIndexChanged;

        LockSlim.EnterWriteLock();
        Renderables.Add(renderable);
        LockSlim.ExitWriteLock();
    }

    internal static void RemoveFromRenderList(IRenderable renderable)
    {
        renderable.ZIndexChanged -= RenderableOnZIndexChanged;

        LockSlim.EnterWriteLock();
        Renderables.Remove(renderable);
        RemovedRenderables.Add(renderable);
        LockSlim.ExitWriteLock();
    }
    
    internal static void AddToRenderList(VisualComponent visual)
    {
        // renderable.ZIndexChanged += RenderableOnZIndexChanged;

        LockSlim.EnterWriteLock();
        Visuals.Add(visual);
        LockSlim.ExitWriteLock();
    }

    internal static void RemoveFromRenderList(VisualComponent visual)
    {
        // renderable.ZIndexChanged -= RenderableOnZIndexChanged;

        LockSlim.EnterWriteLock();
        Visuals.Remove(visual);
        VisualsToRemove.Add(visual);
        LockSlim.ExitWriteLock();
    }

    public static void Draw(int posX, int posY, char symbol, Color foreground, Color background)
    {
        _renderer.Draw(posX, posY, symbol, foreground, background);
    }

    public static void ClearAt(Vector pos) => _renderer.ClearAt(pos.X, pos.Y);

    public static void ClearAt(int posX, int posY) => _renderer.ClearAt(posX, posY);

    public static void ResetStyle() => _renderer.ResetStyle();

    public static void Print(int posX, int posY, ReadOnlySpan<char> text, Color foreground, Color background,
        Alignment alignment = Alignment.Center, TextMode mode = TextMode.Default)
    {
        _renderer.Print(posX, posY, text, foreground, background, alignment, mode);
    }

    public static void DrawRect(Vector pos, Vector size, Color color, char symbol = ' ')
    {
        if (!CalculateDrawArea(pos, size, out var start, out var end)) return;

        _renderer.DrawRect(start, end, color, symbol);
    }

    public static void DrawLine(Vector pos, Vector direction, int length, Color foreground, Color background, char symbol)
    {
        if (!CalculateLineLength(pos, direction, length, out var start, out var calculatedLenght)) return;

        _renderer.DrawLine(start, direction, calculatedLenght, foreground, background, symbol);
    }

    public static void ClearRect(Vector pos, Vector size)
    {
        if (!CalculateDrawArea(pos, size, out var start, out var end)) return;

        _renderer.ClearRect(start, end);
    }

    public static void DrawBorder(Vector pos, Vector size, Color color, BorderStyle style)
    {
        if (!CalculateDrawArea(pos, size, out var start, out var end)) return;

        var calculatedSize = end - start;
        var fitsHorizontally = size.X == calculatedSize.X;
        var fitsVertically = size.Y == calculatedSize.Y;

        _renderer.DrawBorder(start, end, color, style, fitsHorizontally, fitsVertically);
    }

    public static void DrawBuffer(Vector pos, PixelBuffer buffer)
    {
        if (!CalculateDrawArea(pos, buffer.Size, out var start, out var end)) return;

        _renderer.DrawBuffer(start, end, buffer.Data);
    }

    private static void Start()
    {
#if !DEBUG
        try
        {
#endif
            MainLoop();
#if !DEBUG
        }
        catch (Exception exception)
        {
            Application.Exit(exception);

            Throw(exception);
        }
#endif
    }

    [Conditional("DEBUG")]
    private static void Throw(Exception exception) => throw exception;

    private static void MainLoop()
    {
        var stopwatch = new Stopwatch();

        _refreshing = true;

        while (_refreshing)
        {
            stopwatch.Start();

            var shouldResizeBuffer = ScreenResizer.Resize(ShouldTryToResizeConsoleBuffer);
            if (shouldResizeBuffer) _renderer.ResizeBuffer(ScreenResizer.BufferSize);

            Draw();

            stopwatch.Stop();
            var sleepTime = _frameTime - (int) stopwatch.ElapsedMilliseconds;

            stopwatch.Reset();

            if (sleepTime > 0) Thread.Sleep(sleepTime);
        }
    }

    private static bool CalculateDrawArea(Vector position, Vector size, out Vector start, out Vector end)
    {
        start = new Vector();
        end = new Vector();

        if (position.X >= Width || position.Y >= Height || size == Vector.Zero) return false;

        start.X = Math.Max(position.X, 0);
        start.Y = Math.Max(position.Y, 0);

        end.X = Math.Min(Math.Max(position.X + size.X, 0), Width);
        end.Y = Math.Min(Math.Max(position.Y + size.Y, 0), Height);

        return start.X != end.X && start.Y != end.Y;
    }

    private static bool CalculateLineLength(Vector position, Vector direction, int originalLength,
        out Vector start, out int calculatedLenght)
    {
        start = new Vector();
        calculatedLenght = 0;

        if (originalLength == 0 || direction == Vector.Zero) return false;

        if (position.X < 0 && direction.X <= 0) return false;
        if (position.X >= Width && direction.X >= 0) return false;
        if (position.Y < 0 && direction.Y <= 0) return false;
        if (position.Y >= Height && direction.Y >= 0) return false;

        start.X = Math.Max(Math.Min(position.X, Width - 1), 0);
        start.Y = Math.Max(Math.Min(position.Y, Height - 1), 0);

        // Skipping unnecessary operations if line is not diagonal
        if (direction is {X: not 0, Y: not 0})
        {
            var adjusted = false;

            if (start.X != position.X)
            {
                var difference = position.X - start.X;
                if (Math.Abs(difference) > originalLength) return false;

                start.Y += direction.Y * difference;
                start.Y = Math.Max(Math.Min(start.Y, Height - 1), 0);

                adjusted = true;
            }

            if (start.Y != position.Y && !adjusted)
            {
                var difference = position.Y - start.Y;
                if (Math.Abs(difference) > originalLength) return false;

                start.X += direction.X * difference;
                start.X = Math.Max(Math.Min(start.X, Width - 1), 0);
            }
        }

        var end = new Vector
        {
            X = Math.Min(Math.Max(position.X + direction.X * originalLength, 0), Width - 1),
            Y = Math.Min(Math.Max(position.Y + direction.Y * originalLength, 0), Height - 1)
        };

        var lengthX = Math.Abs(end.X - start.X);
        var lengthY = Math.Abs(end.Y - start.Y);

        calculatedLenght = direction switch
        {
            {X: not 0, Y: 0} => lengthX,
            {X: 0, Y: not 0} => lengthY,
            _ => Math.Min(lengthX, lengthY)
        };

        return calculatedLenght > 0;
    }

    private static void RenderableOnZIndexChanged(Component sender, ZIndexChangedEventArgs e) => SortRenderables();

    private static void SortRenderables()
    {
        LockSlim.EnterWriteLock();
        Renderables.Sort((r1, r2) => r1.CompareTo(r2));
        LockSlim.ExitWriteLock();
    }

    private static void Draw()
    {
        LockSlim.EnterWriteLock();

        // foreach (var removed in RemovedRenderables) removed.Clear();
        //
        // RemovedRenderables.Clear();

        foreach (var toRemove in VisualsToRemove) toRemove.Clear();
        VisualsToRemove.Clear();

        // foreach (var renderable in Renderables)
        // {
        //     if (renderable.ShouldRender) renderable.Render();
        // }

        foreach (var visual in Visuals)
        {
            if (visual.Visible) visual.Render();
        }

        LockSlim.ExitWriteLock();

        _renderer.Draw();
    }

    private static void GetDisplayMode()
    {
        var handle = NativeConsole.HandleOut;

        var mode = 0u;
        if (handle == (nint) (-1L) || !NativeConsole.GetConsoleMode(handle, ref mode))
        {
            Mode = DisplayMode.Native;
            return;
        }

        NativeConsole.SetConsoleMode(handle, mode | 4U);
        Mode = DisplayMode.Ansi;
    }
}

public enum DisplayMode
{
    Auto,
    Native,
    Ansi
}
