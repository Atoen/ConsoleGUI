namespace ConsoleGUI.ConsoleDisplay;

public static class Colors
{
    public static IEnumerable<Color> Gradient(Color start, Color end, int steps)
    {
        if (steps < 2)
        {
            throw new ArgumentException("Cannot create gradient from less than 2 colors", nameof(steps));
        }

        var stepA = (end.A - start.A) / (steps - 1);
        var stepR = (end.R - start.R) / (steps - 1);
        var stepG = (end.G - start.G) / (steps - 1);
        var stepB = (end.B - start.B) / (steps - 1);

        for (var i = 0; i < steps; i++)
        {
            yield return Color.FromArgb(start.A + stepA * i,
                start.R + stepR * i,
                start.G + stepG * i,
                start.B + stepB * i);
        }
    }

    public static Color Brighter(this Color color, int brighteningPercent = 20)
    {
        if (brighteningPercent < -100)
        {
            throw new ArgumentOutOfRangeException(nameof(brighteningPercent), "Value cannot be lower that -100");
        }

        var factor = 100 + brighteningPercent;

        var a = Math.Min(color.A * factor / 100, 255);
        var r = Math.Min(color.R * factor / 100, 255);
        var g = Math.Min(color.G * factor / 100, 255);
        var b = Math.Min(color.B * factor / 100, 255);

        return Color.FromArgb(a, r, g, b);
    }

    public static Color Dimmer(this Color color, int dimmingPercent = 20) => Brighter(color, -dimmingPercent);

    public static short CombineToShort(Color foreground, Color background)
    {
        var foregroundIndex = ConsoleColorIndex(foreground);
        var backgroundIndex = ConsoleColorIndex(background);

        return (short) (foregroundIndex | (short) (backgroundIndex << 4));
    }

    public static short ConsoleColorIndex(this Color color)
    {
        var index = color.R > 128 | color.G > 128 | color.B > 128 ? 8 : 0; // Bright bit
        index |= color.R > 64 ? 4 : 0; // Red bit
        index |= color.G > 64 ? 2 : 0; // Green bit
        index |= color.B > 64 ? 1 : 0; // Blue bit

        return (short) index;
    }

    public static ConsoleColor ConsoleColorFg(this Color color)
    {
        return (ConsoleColor) ConsoleColorIndex(color);
    }

    public static ConsoleColor ConsoleColorBg(this Color color)
    {
        return (ConsoleColor) (ConsoleColorIndex(color) << 4);
    }
}