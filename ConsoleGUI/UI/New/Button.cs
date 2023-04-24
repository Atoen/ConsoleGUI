using ConsoleGUI.UI.Events;

namespace ConsoleGUI.UI.New;

public class Button : Button<Text> { }

public class Button<TText> : Control, ITextWidget<TText> where TText : class, IText
{
    public Button()
    {
        _text = CreateDefaultText();
        RequestedContentSpace = _text.Size;
        
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

    public Action? OnClick { get; set; }
    
    private TText CreateDefaultText()
    {
        var defaultArgs = new object[] { nameof(Button) };

        var text = (TText) Activator.CreateInstance(typeof(TText), defaultArgs)!;
        (text as Component)!.Parent = this;

        return text;
    }
    
    // private void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
    // {
    //     switch (args.PropertyName)
    //     {
    //         case nameof(Text):
    //             ReplaceText(args);
    //             break;
    //
    //         case nameof(Position):
    //         case nameof(GlobalPosition):
    //         case nameof(TextOffset):
    //         case nameof(Size):
    //         case nameof(Width):
    //         case nameof(Height):
    //             Text.Center = Center + TextOffset;
    //             break;
    //     }
    // }

    private void SetHandlers()
    {
        
    }

    private void ReplaceText(PropertyChangedEventArgs args)
    {
        var oldText = (Text) args.OldValue!;
        var newText = (Text) args.NewValue!;

        oldText.Delete();

        newText.Parent = this;
        newText.Center = Center;

        RequestedContentSpace = newText.Size;
    }

    private TText _text;
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
        (Text as Visual)?.Clear();
        base.Clear();
    }

    public override void Delete()
    {
        (Text as Visual)?.Delete();
        base.Delete();
    }
}