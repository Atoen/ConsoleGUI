using System.Runtime.InteropServices;
using static ConsoleGUI.NativeConsole;

namespace ConsoleGUI;

public static class Clipboard
{
    private static readonly object SyncRoot = new();

    public static string GetText()
    {
        lock (SyncRoot)
        {
            if (!OpenClipboard(nint.Zero)) return string.Empty;

            var clipboardData = GetClipboardData(ClipboardUnicodeFormat);
            var clipboardContent = Marshal.PtrToStringUni(clipboardData);

            CloseClipboard();

            return clipboardContent ?? string.Empty;
        }
    }

    public static void SetText(string text)
    {
        lock (SyncRoot)
        {
            if (!OpenClipboard(nint.Zero)) return;

            var hGlobal = Marshal.StringToHGlobalUni(text);

            EmptyClipboard();
            SetClipboardData(ClipboardUnicodeFormat, hGlobal);
            CloseClipboard();

            Marshal.FreeHGlobal(hGlobal);
        }
    }
}
