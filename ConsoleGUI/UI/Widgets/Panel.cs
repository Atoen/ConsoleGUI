using ConsoleGUI.Utils;

namespace ConsoleGUI.UI.Widgets;

public class Panel : Control, IContainer
{
    public ObservableList<Control> Children { get; } = new();
}
