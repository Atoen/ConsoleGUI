using ConsoleGUI.ConsoleDisplay;
using ConsoleGUI.UI.Widgets;

namespace ConsoleGUI.UI.New;

public interface IText : IPosition, ISize, IEnumerable<char>
{
    string Content { get; set; }
    int Length { get; }

    Color Foreground { get; set; }
    Color Background { get; set; }

    TextMode TextMode { get; set; }
    Alignment Alignment { get; set; }
}