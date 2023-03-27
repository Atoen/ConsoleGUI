using ConsoleGUI.ConsoleDisplay;
using ConsoleGUI.UI.Events;

namespace ConsoleGUI.UI.Widgets;

public class Canvas : ContentControl
{
    public Canvas(int width, int height)
    {
        Buffer = new PixelBuffer(width, height);
        Buffer.Fill(Color.Empty, Color.White);

        ShowFocusedBorder = false;
    }

    public bool Resizable { get; set; } = true;
    public Color ResizeGripColor { get; set; } = Color.Gray;

    public readonly PixelBuffer Buffer;

    public bool InResizeMode { get; private set; }
    private Vector _resizeStartPos;

    protected bool IsCorrectDrawPosition(Vector pos)
    {
        var clickPos = pos - InnerPadding;

        return clickPos.X < Buffer.Size.X && clickPos.Y < Buffer.Size.Y && clickPos is {X: >= 0, Y: >= 0};
    }

    public override void Resize()
    {
        MinSize = Buffer.Size + InnerPadding * 2;

        ApplyResizing();
    }

    private void GripResize(MouseEventArgs e)
    {
        var sizeDifference = e.CursorPosition - _resizeStartPos;

        if (sizeDifference == Vector.Zero) return;

        var newSize = Buffer.Size + sizeDifference;

        Buffer.Resize(newSize);
        Buffer.Fill(Color.Empty, Color.White);
        Resize();

        _resizeStartPos = e.CursorPosition;
    }

    private void EnterResizeMode(MouseEventArgs e)
    {
        if (!Resizable) return;

        InResizeMode = true;
        _resizeStartPos = e.CursorPosition;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (InResizeMode) GripResize(e);

        base.OnMouseLeftDown(e);
    }

    protected override void OnMouseLeftDown(MouseEventArgs e)
    {
        if (e.RelativeCursorPosition == InnerSize) EnterResizeMode(e);

        base.OnMouseLeftDown(e);
    }

    protected override void OnMouseLeftUp(MouseEventArgs e)
    {
        InResizeMode = false;
        base.OnMouseLeftUp(e);
    }

    protected override void OnMouseExit(MouseEventArgs e)
    {
        InResizeMode = false;
        base.OnMouseExit(e);
    }

    public override void Render()
    {
        base.Render();

        var pos = GlobalPosition + InnerPadding;

        Display.DrawBuffer(pos, Buffer);

        if (!Resizable) return;

        var lowerRightEdge = GlobalPosition + InnerSize;

        Display.Draw(lowerRightEdge.X, lowerRightEdge.Y, 'J', Color.Empty, ResizeGripColor);
        Display.Draw(lowerRightEdge.X - 1, lowerRightEdge.Y, '\\', Color.Empty, ResizeGripColor);
        Display.Draw(lowerRightEdge.X, lowerRightEdge.Y - 1, '\\', Color.Empty, ResizeGripColor);
    }
}
