using ConsoleGUI.ConsoleDisplay;
using ConsoleGUI.UI.Events;

namespace ConsoleGUI.UI.Widgets;

public class Canvas : ContentControl
{
    public Canvas(int width, int height)
    {
        Buffer = new PixelBuffer(width, height);
        Buffer.Fill(Color.Empty, EmptyColor);

        ShowFocusedBorder = false;
    }

    public bool CanGripResize { get; set; }
    public Vector GripPosition => InnerSize;
    public Color ResizeGripColor { get; set; } = Color.Gray;

    public readonly PixelBuffer Buffer;
    public Color EmptyColor { get; set; } = Color.White;

    public bool InResizeMode { get; private set; }
    private Vector _resizeStartPos;
    protected bool DisplayingGrip;

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

        Buffer.Resize(newSize, EmptyColor);
        Resize();

        _resizeStartPos = e.CursorPosition;
    }

    private void EnterResizeMode(MouseEventArgs e)
    {
        if (!CanGripResize) return;

        InResizeMode = true;
        _resizeStartPos = e.CursorPosition;
    }

    protected override void OnMouseEnter(MouseEventArgs e)
    {
        if (CanGripResize) DisplayingGrip = true;
        
        base.OnMouseEnter(e);
    }

    protected override void OnMouseExit(MouseEventArgs e)
    {
        if (CanGripResize) DisplayingGrip = false;
        InResizeMode = false;
        
        base.OnMouseExit(e);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (InResizeMode) GripResize(e);

        base.OnMouseLeftDown(e);
    }

    protected override void OnMouseLeftDown(MouseEventArgs e)
    {
        if (e.RelativeCursorPosition == GripPosition) EnterResizeMode(e);

        base.OnMouseLeftDown(e);
    }

    protected override void OnMouseLeftUp(MouseEventArgs e)
    {
        InResizeMode = false;
        if (IsMouseOver) DisplayingGrip = true;
        
        base.OnMouseLeftUp(e);
    }

    protected override void OnMouseRightUp(MouseEventArgs e)
    {
        if (IsMouseOver) DisplayingGrip = true;
        
        base.OnMouseRightUp(e);
    }

    public override void Render()
    {
        base.Render();

        var pos = GlobalPosition + InnerPadding;

        Display.DrawBuffer(pos, Buffer);

        if (!CanGripResize || !DisplayingGrip) return;

        var globalGripPosition = GlobalPosition + GripPosition;

        Display.Draw(globalGripPosition.X, globalGripPosition.Y, 'O', Color.Empty, ResizeGripColor);
    }
}
