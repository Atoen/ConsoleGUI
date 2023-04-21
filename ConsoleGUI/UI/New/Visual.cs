using ConsoleGUI.ConsoleDisplay;

namespace ConsoleGUI.UI.New;

public abstract class Visual : Component
{
    protected Visual() => Display.AddToRenderList(this);

    public virtual bool Visible { get; set; } = true;

    internal abstract void Render();

    internal abstract void Clear();

    public virtual void Delete() => Display.RemoveFromRenderList(this);
}