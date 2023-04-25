using System.Reflection;
using ConsoleGUI.Utils;

namespace ConsoleGUI.UI.New;

public class Label : Label<Text> { }

public class Label<TText> : Control, ITextWidget<TText> where TText : Text
{
    public Label()
    {
        _text = New.Text.CreateDefault<TText>(this);

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

    public Vector TextOffset
    {
        get => _textOffset;
        set => SetField(ref _textOffset, value);
    }

    private void SetHandlers()
    {
        void CenterText(Label component, PropertyChangedEventArgs args)
        {
            component.Text.Center = component.Center + component.TextOffset;
        }

        var handlers = new PropertyHandlerDefinitionCollection<Label>()
        {
            {(nameof(Position), nameof(GlobalPosition), nameof(TextOffset), nameof(Size), nameof(Width), nameof(Height)), CenterText},
            {nameof(Text), (component, args) => component.ReplaceText(args)}
        };
        
        HandlerManager.AddHandlers(handlers);
    }

    private void ReplaceText(PropertyChangedEventArgs args)
    {
        var oldText = (args.OldValue as Visual)!;
        var newText = (args.NewValue as Visual)!;

        oldText.Delete();

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