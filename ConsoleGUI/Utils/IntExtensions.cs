namespace ConsoleGUI.Utils;

public static class IntExtensions
{
    public static int RoundTo(this int num, int step) => (int) Math.Ceiling((double) num / step) * step;
}