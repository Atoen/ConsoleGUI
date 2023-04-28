namespace ConsoleGUI.UI.Widgets;

public interface ISize
{
    Vector Size { get; set; }
    Vector MaxSize { get; set; }
    Vector MinSize { get; set; }

    int Width { get; set; }
    int Height { get; set; }
}