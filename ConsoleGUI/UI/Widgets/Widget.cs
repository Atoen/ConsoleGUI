using ConsoleGUI.ConsoleDisplay;

namespace ConsoleGUI.UI.Widgets;

public abstract class Widget : Control
{
    public GridResizeDirection GridResizeDirection { get; init; } = GridResizeDirection.None;

    // public abstract Widget Grid(int row, int column, int rowSpan = 1, int columnSpan = 1,
    //     GridAlignment alignment = GridAlignment.Center);

    // protected T Grid<T>(int row, int column, int rowSpan = 1, int columnSpan = 1,
    //     GridAlignment alignment = GridAlignment.Center)
    //     where T : Widget
    // {
    //     // if (AutoResize) Resize();
    //     //
    //     // Parent.Grid(this, row, column, rowSpan, columnSpan, alignment);
    //     //
    //     // Display.AddToRenderList(this);
    //     //
    //     // return this as T ?? throw new InvalidOperationException();
    // }

    // public abstract Widget Place(int posX, int posY);

    // protected T Place<T>(int posX, int posY) where T : Widget
    // {
    //     // if (AutoResize) Resize();
    //     //
    //     // Parent.Place(this, posX, posY);
    //     //
    //     // Display.AddToRenderList(this);
    //     //
    //     // return this as T ?? throw new InvalidOperationException();
    // }

    public override void Remove()
    {
        Display.RemoveFromRenderList(this);

        base.Remove();
    }
}

public enum Alignment
{
    Left,
    Right,
    Center
}


