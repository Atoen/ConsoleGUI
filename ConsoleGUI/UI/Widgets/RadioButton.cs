using ConsoleGUI.ConsoleDisplay;
using ConsoleGUI.UI.Events;

namespace ConsoleGUI.UI.Widgets;

public class RadioButton : Button
{
    public RadioButton(Variable variable, int value)
    {
        Text = new Text(nameof(RadioButton)) {Parent = this};

        _variable = variable;
        _value = value;

        if (IsSelected) State = State.Highlighted;
    }

    public bool IsSelected => _variable.Val == _value;

    public TextMode SelectedTextMode { get; set; } = TextMode.DoubleUnderline;

    private readonly Variable _variable;
    private readonly int _value;

    protected override void OnMouseLeftDown(MouseEventArgs e)
    {
        _variable.Val = _value;

        base.OnMouseLeftDown(e);
    }

    public override void Render()
    {
        Text.TextMode = IsSelected ? SelectedTextMode : TextMode.Default;

        if (IsSelected && State == State.Default) State = State.Highlighted;
        else if (!IsSelected && !IsMouseOver) State = State.Default;

        base.Render();
    }
}

public sealed class Variable
{
    public Variable(int val = 0) => Val = val;

    public int Val
    {
        get => _val;
        set
        {
            if (value != _val) OnValueChanged?.Invoke(this, value);
            _val = value;
        }
    }

    private int _val;

    public delegate void ValueChangedEventHandler(Variable variable, int newValue);
    public event ValueChangedEventHandler? OnValueChanged;
}
