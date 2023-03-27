using ConsoleGUI.Utils;

namespace ConsoleGUI.UI;

public interface IContainer
{
    public ObservableList<Control> Children { get; }
}
