namespace ConsoleGUI.UI.New;

public class Label : Control
{
    public Label()
    {
        _text = new Text(nameof(Label)) {Parent = this};
        _text.PropertyChanged += TextOnPropertyChanged;
        
        Focusable = false;
    }

    public Text Text
    {
        get => _text;
        set => SetField(ref _text, value, onChanged: OnTextChanged);
    }

    public bool AllowTextOverflow
    {
        get => _allowTextOverflow;
        set => SetField(ref _allowTextOverflow, value);
    }

    private void OnTextChanged(Text oldValue, Text newValue)
    {
        oldValue.PropertyChanged -= TextOnPropertyChanged;
        oldValue.Delete();
        
        newValue.Parent = this;
        newValue.PropertyChanged += TextOnPropertyChanged;
    }

    private void TextOnPropertyChanged(object sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == nameof(Size))
        {
            RequestedContentSpace = (Vector) args.NewValue!;
        }
    }

    private Text _text;
    private bool _allowTextOverflow;

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