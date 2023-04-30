using ConsoleGUI.ConsoleDisplay;
using ConsoleGUI.UI.Events;

namespace ConsoleGUI.UI.Old.Widgets;

public class Canvas : ContentOldControl
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

    public PixelBuffer Buffer { get; }
    public Vector MinBufferSize { get; set; } = new(7, 5);
    public Color EmptyColor { get; set; } = Color.White;

    public bool InResizeMode { get; private set; }
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
        var sizeDifference = e.RelativeCursorPosition - GripPosition;

        if (sizeDifference == Vector.Zero) return;

        var newSize = Buffer.Size + sizeDifference;
        newSize = newSize.ExpandTo(MinBufferSize);

        if (newSize == Buffer.Size) return;

        Buffer.Resize(newSize, EmptyColor);
        Resize();
    }

    private void EnterResizeMode(MouseEventArgs e)
    {
        if (!CanGripResize) return;

        InResizeMode = true;
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

        if (DisplayingGrip)
        {
            var globalGripPosition = GlobalPosition + GripPosition;

            Display.Draw(globalGripPosition.X, globalGripPosition.Y, 'O', Color.Empty, ResizeGripColor);
        }

        if (InResizeMode)
        {
            PrintDimensions();
        }
    }

    private void PrintDimensions()
    {
        Span<char> span = stackalloc char[8];

        var bufferWidth = Buffer.Size.X;
        var bufferHeight = Buffer.Size.Y;

        bufferWidth.TryFormat(span, out var length);
        span[length++] = 'x';

        bufferHeight.TryFormat(span[length..], out var secondLength);
        length += secondLength;

        span = span[..length];

        Display.Print(Center.X, Center.Y, span, Color.Gray, Color.Empty, mode: TextMode.Italic);
    }
}
