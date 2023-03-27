namespace ConsoleGUI.Utils;

public static class EnumExtensions
{
    public static bool HasValue<T>(this T value, T flag) where T : Enum
    {
        var f = Convert.ToInt32(flag);
        return (Convert.ToInt32(value) & f) == f;
    }
}