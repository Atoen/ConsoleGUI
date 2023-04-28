using ConsoleGUI.ConsoleDisplay;
using ConsoleGUI.UI.Old;
using ConsoleGUI.Utils;
using ConsoleGUI.Visuals;

namespace ConsoleGUI.UI.Widgets;

public abstract class UiElement : Visual
{
    protected UiElement() => SetHandlers();

    public Color DefaultColor { get; set; } = Theme.Colors.DefaultColor;
    public Color HighlightedColor { get; set; } = Theme.Colors.HighlightedColor;
    public Color PressedColor { get; set; } = Theme.Colors.PressedColor;
    public Color InactiveColor { get; set; } = Theme.Colors.InactiveColor;

    public Color ColorTheme
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

    public Color CurrentColor
    {
        get
        {
            if (!Enabled) return InactiveColor;

            return State switch
            {
                State.Pressed => PressedColor,
                State.Highlighted => HighlightedColor,
                _ => DefaultColor
            };
        }
    }

    public Border Border { get; set; } = new()
    {
        Visible = Theme.Border.Visible, Color = Theme.Border.Color, Style = Theme.Border.Style
    };

    public Vector InnerPadding
    {
        get => _innerPadding;
        set => SetField(ref _innerPadding, value);
    }

    public Vector OuterPadding
    {
        get => _outerPadding;
        set => SetField(ref _outerPadding, value);
    }

    public Vector RequestedContentSpace
    {
        get => _requestedContentSpace;
        protected set => SetField(ref _requestedContentSpace, value);
    }

    public Vector PaddedSize => Size + OuterPadding * 2;
    public Vector InnerSize => Size - InnerPadding * 2;

    public int PaddedWidth => Width + OuterPadding.X * 2;
    public int PaddedHeight => Height + OuterPadding.Y * 2;

    public int InnerWidth => Width - InnerPadding.X * 2;
    public int InnerHeight => Height - InnerPadding.Y * 2;

    private Vector _innerPadding = (1, 1);
    private Vector _outerPadding = Vector.Zero;
    private Vector _requestedContentSpace;

    internal override void Render()
    {
        Display.DrawRect(GlobalPosition, Size, CurrentColor);

        if (Border.Visible) Display.DrawBorder(GlobalPosition, Size, Border.Color, Border.Style);
    }

    internal override void Clear() => Display.ClearRect(GlobalPosition, Size);

    public override void Delete() => Display.RemoveFromRenderList(this);

    private void TryToExpandToRequestedSize()
    {
        if (Width < RequestedContentSpace.X) Width = RequestedContentSpace.X;
        if (Height < RequestedContentSpace.Y) Height = RequestedContentSpace.Y;
    }

    protected void Resize()
    {
        switch (ResizeMode)
        {
            case ResizeMode.Grow:
                Size = Size.ExpandTo(RequestedContentSpace + InnerPadding * 2);
                break;

            case ResizeMode.Stretch:
                Size = RequestedContentSpace + InnerPadding * 2;
                break;
            case ResizeMode.Manual:
                break;
            case ResizeMode.Expand:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void SetHandlers()
    {
        void ClearOnSize(UiElement component, PropertyChangedEventArgs args)
        {
            component.ClearOnSizeChanged((Vector) args.OldValue!, (Vector) args.NewValue!);
        }

        void ClearOnMoveAction(UiElement component, PropertyChangedEventArgs args)
        {
            component.ClearOnMove((Vector) args.OldValue! - (Vector) args.NewValue!);
        }

        var handlers = new PropertyHandlerDefinitionCollection<UiElement>
        {
            {(nameof(Size), nameof(Width), nameof(Height), nameof(InnerPadding), nameof(OuterPadding)), ClearOnSize},
            {(nameof(Position), nameof(GlobalPosition)), ClearOnMoveAction},
            {nameof(RequestedContentSpace), (component, _) => component.Resize()},
            {(nameof(MinSize), nameof(MaxSize)), (component, _) => component.TryToExpandToRequestedSize()}
        };

        HandlerManager.AddHandlers(handlers);
    }
}