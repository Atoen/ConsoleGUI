using ConsoleGUI.Utils;

namespace ConsoleGUI.UI.Old;

public interface IContainer
{
    public ObservableList<OldControl> Children { get; }
}
