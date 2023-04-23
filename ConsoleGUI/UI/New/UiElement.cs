using ConsoleGUI.ConsoleDisplay;
using ConsoleGUI.Visuals;

namespace ConsoleGUI.UI.New;

public abstract class UiElement : Visual
{
    protected UiElement() => PropertyChanged += OnPropertyChanged;

    public Color DefaultColor { get; set; } = Color.Bisque;
    public Color HighlightedColor { get; set; } = Color.Gray;
    public Color PressedColor { get; set; } = Color.DarkGray;
    public Color InactiveColor { get; set; } = Color.DarkSlateGray;

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

    public bool ShowBorder { get; set; }
    public BorderStyle BorderStyle { get; set; } = BorderStyle.Single;
    public Color BorderColor { get; set; } = Color.White;

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

        if (ShowBorder) Display.DrawBorder(GlobalPosition, Size, BorderColor, BorderStyle);
    }

    internal override void Clear() => Display.ClearRect(GlobalPosition, Size);

    public override void Delete() => Display.RemoveFromRenderList(this);

    // private void ClearOnMove(Vector positionDelta)
    // {
    //     Display.ClearRect(GlobalPosition + positionDelta, Size);
    // }
    //
    // private void ClearOnSizeChanged(Vector oldSize, Vector newSize)
    // {
    //     if (newSize.X >= oldSize.X && newSize.Y >= oldSize.Y) return;
    //
    //     Display.ClearRect(GlobalPosition, oldSize);
    // }

    private void TryToExpandToRequestedSize()
    {
        if (Width < RequestedContentSpace.X) Width = RequestedContentSpace.X;
        if (Height < RequestedContentSpace.Y) Height = RequestedContentSpace.Y;
    }

    private void Resize()
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

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
    {
        var property = args.PropertyName;
        var oldValue = args.OldValue;
        var newValue = args.NewValue;

        switch (property)
        {
            case nameof(Size):
            case nameof(InnerPadding):
            case nameof(OuterPadding):
                ClearOnSizeChanged((Vector) oldValue!, (Vector) newValue!);
                break;

            case nameof(Position):
            case nameof(GlobalPosition):
                ClearOnMove((Vector) oldValue! - (Vector) newValue!);
                break;

            case nameof(RequestedContentSpace):
                Resize();
                break;

            case nameof(MaxSize):
            case nameof(MinSize):
                TryToExpandToRequestedSize();
                break;
        }
    }
}