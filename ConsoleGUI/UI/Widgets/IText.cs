using ConsoleGUI.ConsoleDisplay;
using ConsoleGUI.UI.Old.Widgets;

namespace ConsoleGUI.UI.Widgets;

public interface IText : IPosition, ISize, IEnumerable<char>
{
    string Content { get; set; }
    int Length { get; }

    Color Foreground { get; set; }
    Color Background { get; set; }

    TextMode TextMode { get; set; }
    Alignment Alignment { get; set; }
}