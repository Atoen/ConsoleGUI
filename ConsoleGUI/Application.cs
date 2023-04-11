using ConsoleGUI.ConsoleDisplay;

namespace ConsoleGUI;

public static class Application
{
    static Application()
    {
        Console.CancelKeyPress += delegate
        {
            Input.Stop();
            Display.Stop();
            Display.ResetStyle();

            DisplayUsedMemory();

            Environment.Exit(Environment.ExitCode);
        };

        AppDomain.CurrentDomain.ProcessExit += delegate
        {
            ApplicationExit?.Invoke(null, EventArgs.Empty);
        };
    }

    public static event EventHandler? ApplicationExit;

    public static void Start(DisplayMode displayMode = DisplayMode.Auto)
    {
        Display.Init(displayMode);
        Input.Init();
    }

    private static void DisplayUsedMemory()
    {
        using var process = Process.GetCurrentProcess();

        process.Refresh();
        var peakPhysical = process.PeakWorkingSet64;
        var peakPaged = process.PeakPagedMemorySize64;

        Console.Clear();
        Console.WriteLine("Exiting...");

        const double bytesPerMByte = 1_048_576D;
        Console.WriteLine(
            $"Memory usage - Physical: {peakPhysical / bytesPerMByte:.00}MB, Paged: {peakPaged / bytesPerMByte:.00}MB");

    }

    public static void Exit(Exception? exception = null)
    {
        try
        {
            Input.Stop();
            Display.Stop();
            Display.ResetStyle();
        }
        finally
        {
            Console.Clear();

            if (exception != null) Console.WriteLine(exception);
            else DisplayUsedMemory();
        }
    }
}