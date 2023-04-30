using ConsoleGUI.Attributes.Attributes;
using ConsoleGUI.UI.Old.Widgets;

namespace ConsoleGUI.UI.Widgets;

internal struct GridCell
{

    public GridCell()
    {
        ElementSize = default;
        ConnectionDirection = GridCellConnectionDirection.None;
    }

    public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Middle;
    public VerticalAlignment VerticalAlignment { get; set; } = VerticalAlignment.Middle;
    public Vector ElementSize { get; set; }
    public GridCellConnectionDirection ConnectionDirection { get; set; }
    public bool Connected { get; set; }

    public Vector Size
    {
        get => _size;
        set => _size = value;
    }

    public int Width
    {
        get => _size.X;
        set => _size.X = value;
    }

    public int Height
    {
        get => _size.Y;
        set => _size.Y = value;
    }

    private Vector _size;
}

public enum GridCellConnectionDirection
{
    None,
    Up,
    Down,
    Left,
    Right
}