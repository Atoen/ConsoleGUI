using ConsoleGUI.ConsoleDisplay;
using ConsoleGUI.UI.Events;

namespace ConsoleGUI.UI.New;

public class RadioButton : Button
{
    public RadioButton(RadioVariable variable, int buttonValue)
    {
        Text.Content = nameof(RadioButton);

        _variable = variable;
        _value = buttonValue;
    }

    public TextMode SelectedTextMode { get; set; } = TextMode.DoubleUnderline;
    public bool IsSelected => _variable.Value == _value;

    private readonly RadioVariable _variable;
    private readonly int _value;

    protected override void OnMouseLeftDown(MouseEventArgs e)
    {
        _variable.Value = _value;
        base.OnMouseLeftDown(e);
    }

    internal override void Render()
    {
        Text.TextMode = IsSelected ? SelectedTextMode : TextMode.Default;

        if (IsSelected && State == State.Default) State = State.Highlighted;
        else if (!IsSelected && !IsMouseOver) State = default;

        base.Render();
    }
}

public sealed class RadioVariable
{
    public RadioVariable(int value = 0) => Value = value;

    public int Value { get; set; }
}