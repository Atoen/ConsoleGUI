using ConsoleGUI.Attributes.Attributes;
using ConsoleGUI.ConsoleDisplay;
using ConsoleGUI.UI.Widgets;
using ConsoleGUI.Visuals;

namespace ConsoleGUI.UI.Old;

public abstract class VisualOldComponent : OldComponent, IRenderable
{
    protected VisualOldComponent()
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
        var delta = e.OldPos - e.NewPos;
        var globalPos = GlobalPosition;

        if (delta.X < 0)
        {
            Display.DrawRect(globalPos, Size with {X = delta.X}, Color.Red);
        }
        else if (delta.X > 0)
        {
            Display.DrawRect(globalPos with {X = globalPos.X + Size.X - 1}, Size with {X = delta.X}, Color.Red);
        }

        if (delta.Y < 0)
        {
            Display.DrawRect(globalPos, Size with {Y = delta.Y}, Color.Red);
        }
        else if (delta.Y > 0)
        {
            Display.DrawRect(globalPos with {Y = globalPos.Y + Size.Y - 1}, Size with {Y = delta.Y}, Color.Red);
        }
    }

    protected virtual void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        var newSize = e.NewSize;
        var oldSize = e.OldSize;

        if (newSize.X >= oldSize.X && newSize.Y >= oldSize.Y) return;

        var globalPos = GlobalPosition;

        var verticalPos = globalPos with { X = globalPos.X + oldSize.X - 1 };
        var horizontalPos = globalPos with { Y = globalPos.Y + oldSize.Y - 1 };

        var verticalFragment = oldSize with { X = oldSize.X - newSize.X };
        var horizontalFragment = oldSize with { Y = oldSize.Y - newSize.Y };

        Display.ClearRect(verticalPos, verticalFragment);
        Display.ClearRect(horizontalPos, horizontalFragment);
    }
}
