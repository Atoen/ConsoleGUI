using ConsoleGUI.Utils;

namespace ConsoleGUI.UI;

public interface IContainer
{
    public ObservableList<OldControl> Children { get; }
}
