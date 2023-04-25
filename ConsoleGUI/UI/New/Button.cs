using System.Diagnostics.CodeAnalysis;
using ConsoleGUI.UI.Events;
using ConsoleGUI.Utils;

namespace ConsoleGUI.UI.New;

public class Button : Button<Text> { }

public class Button<TText> : Control, ITextWidget<TText> where TText : Text
{
    public Button()
    {
        AddHandlers();
        
        Text = New.Text.CreateDefault<TText>(this);
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

    public Action? OnClick { get; set; }

    private void AddHandlers()
    {
        void CenterText(Button<TText> component, PropertyChangedEventArgs args)
        {
            component.Text.Center = component.Center + component.TextOffset;
        }

        var handlers = new PropertyHandlerDefinitionCollection<Button<TText>>()
        {
            {(nameof(Position), nameof(GlobalPosition), nameof(TextOffset), nameof(Size), nameof(Width), nameof(Height)), CenterText},
            {nameof(Text), (component, args) => component.ReplaceText(args)}
        };
        
        HandlerManager.AddHandlers(handlers);
    }

    private void ReplaceText(PropertyChangedEventArgs args)
    {
        var oldText = (TText?) args.OldValue;
        var newText = (TText) args.NewValue!;

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