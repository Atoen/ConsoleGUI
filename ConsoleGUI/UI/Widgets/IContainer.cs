using ConsoleGUI.Utils;

namespace ConsoleGUI.UI.Widgets;

public interface IContainer
{
    ObservableList<Visual> ContainerElements { get; }

    bool FillScreen { get; }
}
