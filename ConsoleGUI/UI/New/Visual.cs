using ConsoleGUI.ConsoleDisplay;

namespace ConsoleGUI.UI.New;

public abstract class Visual : Component
{
    protected Visual() => Display.AddToRenderList(this);

    public virtual bool Visible { get; set; } = true;

    internal abstract void Render();

    internal abstract void Clear();

    public virtual void Delete() => Display.RemoveFromRenderList(this);

    protected void ClearOnMove(Vector positionDelta)
    {
        Display.ClearRect(GlobalPosition + positionDelta, Size);
    }

    protected void ClearOnSizeChanged(Vector oldSize, Vector newSize)
    {
        if (newSize.X >= oldSize.X && newSize.Y >= oldSize.Y) return;

        Display.ClearRect(GlobalPosition, oldSize);
    }
}