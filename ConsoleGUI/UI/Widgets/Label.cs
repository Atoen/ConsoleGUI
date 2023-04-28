using ConsoleGUI.Utils;

namespace ConsoleGUI.UI.Widgets;

public class Label : Label<Text> { }

public class Label<TText> : Control, ITextWidget<TText> where TText : Text
{
    public Label()
    {
        _text = Widgets.Text.CreateDefault<TText>(this);

        Focusable = false;
        
        SetHandlers();
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

    private void SetHandlers()
    {
        void CenterText(Label<TText> component, PropertyChangedEventArgs args)
        {
            component.Text.Center = component.Center + component.TextPosition;
        }

        var handlers = new PropertyHandlerDefinitionCollection<Label<TText>>()
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

    private TText _text;
    private bool _allowTextOverflow;
    private Vector _textOffset = Vector.Zero;

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