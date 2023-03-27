namespace ConsoleGUI.UI;

public interface IRenderable : IComparable<IRenderable>
{
    void Render();

    bool ShouldRender { get; }

    void Clear();

    int ZIndex { get; }
    event Component.ZIndexChangeEventHandler? ZIndexChanged;

    int IComparable<IRenderable>.CompareTo(IRenderable? other)
    {
        if (other == null) return 1;

        if (ZIndex < other.ZIndex) return -1;
        return ZIndex > other.ZIndex ? 1 : 0;
    }
}