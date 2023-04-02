using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace ConsoleGUI;

internal static partial class NativeConsole
{
    [LibraryImport("kernel32.dll")]
    public static partial nint GetStdHandle(uint nStdHandle);

    public static nint HandleIn => GetStdHandle(StdHandleIn);
    public static nint HandleOut => GetStdHandle(StdHandleOut);

    [LibraryImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool GetConsoleMode(nint hConsoleInput, ref uint mode);

    [LibraryImport("kernel32.dll")]
    public static partial void SetConsoleMode(nint handle, uint mode);

    [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern SafeFileHandle CreateFile(string fileName,
        [MarshalAs(UnmanagedType.U4)] uint fileAccess,
        [MarshalAs(UnmanagedType.U4)] uint fileShare,
        nint securityAttributes,
        [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
        [MarshalAs(UnmanagedType.U4)] int flags,
        nint template);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool WriteConsoleOutput(
        SafeFileHandle hConsoleOutput,
        CharInfo[] lpBuffer,
        SCoord dwBufferSize,
        SCoord dwBufferSCoord,
        ref DisplayRect lpWriteRegion);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "ReadConsoleInput")]
    public static extern bool ReadConsoleInput(nint consoleInput, [Out] InputRecord[] buffer, uint length,
        ref uint numberOfEventsRead);

    [StructLayout(LayoutKind.Explicit)]
    public struct CharInfo
    {
        public static readonly CharInfo Empty = new() {Symbol = '\0', Color = 0};

        [FieldOffset(0)] public char Symbol;
        [FieldOffset(2)] public short Color;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DisplayRect
    {
        public short Left;
        public short Top;
        public short Right;
        public short Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SCoord
    {
        public short X;
        public short Y;

        public override string ToString() => $"({X} {Y})";
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct InputRecord
    {
        [FieldOffset(0)] public ushort EventType;
        [FieldOffset(4)] public KeyEventRecord KeyEventRecord;
        [FieldOffset(4)] public MouseEventRecord MouseEventRecord;
        [FieldOffset(4)] public WindowState WindowBufferSizeEventRecord;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct MouseEventRecord
    {
        [FieldOffset(0)] public SCoord MousePosition;
        [FieldOffset(4)] public uint ButtonState;
        [FieldOffset(12)] public uint EventFlags;
    }

    public struct WindowState
    {
        public SCoord Size;

        public int Width => Size.X;
        public int Height => Size.Y;
    }

    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
    public struct KeyEventRecord
    {
        [FieldOffset(0)] public readonly bool KeyDown;
        [FieldOffset(4)] public readonly ushort RepeatCount;
        [FieldOffset(6)] public readonly ushort VirtualKeyCode;
        [FieldOffset(8)] public readonly ushort VirtualScanCode;
        [FieldOffset(10)] public readonly char UnicodeChar;
        [FieldOffset(10)] public readonly byte AsciiChar;
        [FieldOffset(12)] public readonly uint ControlKeyState;
    }

    public const ushort KeyEventCode = 0x1,
        MouseEventCode = 0x2,
        WindowBufferSizeEvent = 0x4;

    public const uint EnableMouseInput = 0x0010,
        EnableQuickEditMode = 0x0040,
        EnableWindowInput = 0x0008;

    public const uint StdHandleOut = unchecked((uint) -11),
        StdHandleIn = unchecked((uint) -10);
}