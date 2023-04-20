using ConsoleGUI.ConsoleDisplay;
using ConsoleGUI.UI.Events;
using ConsoleGUI.UI.New;

namespace ConsoleGUI.UI.Widgets;

public class Button : ContentOldControl
{
    public Button() => _text = new Text(nameof(Button)) {Parent = this};

    private Text _text;
    public Text Text
    {
        get => _text;
        set
        {
            _text.Remove();
            _text.Parent = null!;

            _text = value;
            _text.Parent = this;
        }
    }

    public Action? OnClick { get; set; }

    internal override void Resize()
    {
        MinSize = InnerPadding * 2 + _text.Size;

        ApplyResizing();

        base.Resize();
    }

    public override void Remove()
    {
        Display.RemoveFromRenderList(_text);
        base.Remove();
    }

    public override void Clear()
    {
        base.Clear();
        _text.Clear();
    }

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
}
