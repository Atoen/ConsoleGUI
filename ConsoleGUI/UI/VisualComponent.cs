using ConsoleGUI.Attributes.Attributes;
using ConsoleGUI.ConsoleDisplay;
using ConsoleGUI.Visuals;

namespace ConsoleGUI.UI;

public abstract class VisualComponent : Component, IRenderable
{
    protected VisualComponent()
    {
        Display.AddToRenderList(this);

        PositionChanged += OnPositionChanged;
        SizeChanged += OnSizeChanged;
    }

    public Color DefaultColor { get; set; } = Color.Bisque;
    public Color HighlightedColor { get; set; } = Color.Gray;
    public Color PressedColor { get; set; } = Color.DarkGray;
    public Color InactiveColor { get; set; } = Color.DarkSlateGray;

    public Color Color
    {
        get => DefaultColor;
        set
        {
            DefaultColor = value;
            HighlightedColor = value.Brighter();
            PressedColor = value.Brighter(50);
            InactiveColor = value.Dimmer();
        }
    }

    public bool ShowBorder { get; set; }
    public BorderStyle BorderStyle { get; set; } = BorderStyle.Single;
    public Color BorderColor { get; set; } = Color.Cyan;

    public bool RenderWhenIsRoot { get; set; }
    public virtual bool ShouldRender => (RenderWhenIsRoot || Parent != null) && Visible;

    public bool Visible { get; set; } = true;

    public Color CurrentColor
    {
        get
        {
            if (!Active) return InactiveColor;

            return State switch
            {
                State.Pressed => PressedColor,
                State.Highlighted => HighlightedColor,
                _ => DefaultColor
            };
        }
    }

    [MethodCall(MethodCallMode.Scheduled)]
    public virtual void Render()
    {
        Display.DrawRect(GlobalPosition, Size, CurrentColor);

        if (ShowBorder)
        {
            Display.DrawBorder(GlobalPosition, Size, BorderColor, BorderStyle);
        }
    }

    [MethodCall(MethodCallMode.OnEvent)]
    public virtual void Clear()
    {
        Display.ClearRect(GlobalPosition, Size);
    }

    [MethodCall(MethodCallMode.Manual)]
    public virtual void Remove()
    {
        Display.RemoveFromRenderList(this);
    }

    protected virtual void OnPositionChanged(object sender, PositionChangedEventArgs e)
    {
        Display.ClearRect(GlobalPosition - e.Delta, Size);
    }

    protected virtual void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        Display.ClearRect(GlobalPosition, e.OldSize);
    }
}
