using ConsoleGUI.Visuals;

namespace ConsoleGUI.UI.Old;

public static class Theme
{
    public static class Colors
    {
        public static Color DefaultColor { get; set; } = Color.Bisque;
        public static Color HighlightedColor { get; set; } = Color.Beige;
        public static Color PressedColor { get; set; } = Color.DarkGray;
        public static Color InactiveColor { get; set; } = Color.DarkSlateGray;
    }

    public static class Border
    {
        public static bool Visible { get; set; } = false;
        public static BorderStyle Style { get; set; } = BorderStyle.Single;
        public static Color Color { get; set; } = Color.White;
    }

    public static class FocusedBorder
    {
        public static bool Visible { get; set; } = true;
        public static BorderStyle Style { get; set; } = BorderStyle.Dotted;
        public static Color Color { get; set; } = Color.Black;
    }
}