using System.Reflection;

namespace ConsoleGUI.UI.New;

public class Label : Label<Text> { }

public class Label<TText> : Control, ITextWidget<TText> where TText : class, IText
{
    public Label()
    {
        _text = CreateDefaultText();

        Focusable = false;
        PropertyChanged += OnPropertyChanged;
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
    
    private TText CreateDefaultText()
    {
        var defaultArgs = new object[] { nameof(Label) };

        var text = (TText) Activator.CreateInstance(typeof(TText), defaultArgs)!;
        (text as Component)!.Parent = this;

        return text;
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
    {
        switch (args.PropertyName)
        {
            case nameof(Text):
                ReplaceText(args);
                break;

            case nameof(Position):
            case nameof(GlobalPosition):
            case nameof(TextOffset):
            case nameof(Size):
            case nameof(Width):
            case nameof(Height):
                Text.Center = Center + TextOffset;
                break;
        }
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
        (Text as Visual)?.Clear();
        base.Clear();
    }

    public override void Delete()
    {
        (Text as Visual)?.Delete();
        base.Delete();
    }
}