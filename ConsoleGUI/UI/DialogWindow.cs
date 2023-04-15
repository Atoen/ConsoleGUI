using ConsoleGUI.ConsoleDisplay;
using ConsoleGUI.UI.Events;

namespace ConsoleGUI.UI;

public class DialogWindow : OldControl
{
    public DialogWindow()
    {
        ZIndex = 10;

        Position = new Vector(4, 5);
        Size = new Vector(16, 5);

        DefaultColor = Color.Red;
    }

    protected override void OnMouseLeftDown(MouseEventArgs e)
    {
        Display.RemoveFromRenderList(this);

        Remove();

        base.OnMouseLeftDown(e);
    }
}