using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace ConsoleGUI.Utils;

internal static class ScreenResizer
{
    public static int ResizeRetryAttempts { get; set; } = 1;
    public static int ResizeFails { get; private set; }

    public static Vector MinBufferSize { get; } = new(30, 20);
    public static Vector BufferSize { get; private set; }
    public static int BufferSizeStep { get; set; } = 5;

    public static Vector ScreenSize { get; private set; }
    public static int ScreenWidth => ScreenSize.X;
    public static int ScreenHeight => ScreenSize.Y;

    public static void Init()
    {
        var windowWidth = Console.WindowWidth;
        var windowHeight = Console.WindowHeight;

        ScreenSize = new Vector(windowWidth, windowHeight);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Console.SetBufferSize(windowWidth, windowHeight);
        }
    }

    public static bool Resize(bool modifyConsoleBufferSize)
    {
#pragma warning disable CA1416
        if (modifyConsoleBufferSize) ChangeConsoleBufferSize();
#pragma warning restore CA1416
        else ScreenSize = new Vector(Console.WindowWidth, Console.WindowHeight);

        var bufferChanged = ChangeDisplayBufferSize();

        return bufferChanged;
    }

    [SupportedOSPlatform("windows")]
    private static void ChangeConsoleBufferSize(int attempt = 1)
    {
        var windowWidth = Console.WindowWidth;
        var windowHeight = Console.WindowHeight;

        ScreenSize = new Vector(windowWidth, windowHeight);

        var bufferWidth = Console.BufferWidth;
        var bufferHeight = Console.BufferHeight;

        if (windowWidth == bufferWidth && windowHeight == bufferHeight) return;

        if (windowWidth < MinBufferSize.X || windowHeight < MinBufferSize.Y) return;

        var top = Console.WindowTop;
        var left = Console.WindowLeft;

        try
        {
            Console.SetBufferSize(windowWidth + left, windowHeight + top);
        }
        catch (Exception exception)
        {
            ResizeFails++;
            Debug.WriteLine($"Resize failed: {exception}");

            if (attempt <= ResizeRetryAttempts) ChangeConsoleBufferSize(attempt + 1);
        }
    }

    private static bool ChangeDisplayBufferSize()
    {
        var bufferWidth = ScreenSize.X.RoundTo(BufferSizeStep);
        var bufferHeight = ScreenSize.Y.RoundTo(BufferSizeStep);

        var newBufferSize = new Vector(bufferWidth, bufferHeight);

        if (newBufferSize == BufferSize) return false;

        BufferSize = newBufferSize;
        return true;
    }
}

public static class IntExtensions
{
    public static int RoundTo(this int num, int step) => (int)Math.Ceiling((double)num / step) * step;
}
