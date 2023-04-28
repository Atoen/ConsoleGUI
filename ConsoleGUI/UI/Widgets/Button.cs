using ConsoleGUI.UI.Events;
using ConsoleGUI.Utils;

namespace ConsoleGUI.UI.Widgets;

public class Button : Button<Text> { }

public class Button<TText> : Control, ITextWidget<TText> where TText : Text
{
    public Button()
    {
        AddHandlers();
        
        Text = Widgets.Text.CreateDefault<TText>(this);
    }

    public TText Text
    {
        get => _text;
        set => SetField(ref _text, value);
    }

    public bool AllowTextOverflow
    {
        get => _allowTextOverflow;
        set => SetField(ref _allowTextOverflow, value);
    }

    public Vector TextPosition
    {
        get => _textOffset;
        set => SetField(ref _textOffset, value);
    }

    public Action? OnClick { get; set; }

    private void AddHandlers()
    {
        void CenterText(Button<TText> component, PropertyChangedEventArgs args)
        {
            component.Text.Center = component.Center + component.TextPosition;
        }

        var handlers = new PropertyHandlerDefinitionCollection<Button<TText>>
        {
            {(nameof(Position), nameof(GlobalPosition), nameof(TextPosition), nameof(Size), nameof(Width), nameof(Height)), CenterText},
            {nameof(Text), (component, args) => component.ReplaceText(args.As<TText?>())}
        };

        HandlerManager.AddHandlers(handlers);
    }

    private void ReplaceText(PropertyChangedEventArgs<TText?> args)
    {
        var oldText = args.OldValue;
        var newText = args.NewValue!;

        oldText?.Delete();

        newText.Parent = this;
        newText.Center = Center;

        RequestedContentSpace = newText.Size;
    }

    private TText _text = null!;
    private bool _allowTextOverflow;
    private Vector _textOffset = Vector.Zero;

    protected override void OnMouseEnter(MouseEventArgs e)
    {
        State = State.Highlighted;
        base.OnMouseEnter(e);
    }

    protected override void OnMouseExit(MouseEventArgs e)
    {
        State = State.Default;
        base.OnMouseExit(e);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Released) State = State.Highlighted;
        base.OnMouseMove(e);
    }

    protected override void OnMouseLeftDown(MouseEventArgs e)
    {
        State = State.Pressed;
        OnClick?.Invoke();

        base.OnMouseLeftDown(e);
    }

    internal override void Clear()
    {
        Text.Clear();
        base.Clear();
    }

    public override void Delete()
    {
        Text.Delete();
        base.Delete();
    }
}