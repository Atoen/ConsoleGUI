using ConsoleGUI.Utils;

namespace ConsoleGUI.UI.Old.Widgets;

public class Panel : OldControl, IContainer
{
    public ObservableList<OldControl> Children { get; } = new();
}
