using ConsoleGUI.Utils;

namespace ConsoleGUI.UI.Widgets;

public class Panel : OldControl, IContainer
{
    public ObservableList<OldControl> Children { get; } = new();
}
