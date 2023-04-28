namespace ConsoleGUI.UI.Old.Widgets;

public class Label : ContentOldControl
{
    public Label()
    {
        _text = new Text(nameof(Label)) {Parent = this};
        Focusable = false;
    }

    private Text _text;
    public Text Text
    {
        get => _text;
        set
        {
            _text.Remove();
            _text.Parent = null;

            _text = value;
            _text.Parent = this;
        }
    }

    public override void Remove()
    {
        _text.Remove();
        base.Remove();
    }

    public override void Clear()
    {
        _text.Clear();
        base.Clear();
    }

    public override void Resize()
    {
        MinSize = InnerPadding * 2 + _text.Size;

        ApplyResizing();

        base.Resize();
    }
}
