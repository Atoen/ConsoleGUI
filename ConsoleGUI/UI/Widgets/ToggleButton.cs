using ConsoleGUI.UI.Events;

namespace ConsoleGUI.UI.Widgets;

public class ToggleButton<T> : Button
{
    public ToggleButton()
    {
        Text = new Text(nameof(ToggleButton<T>)) {Parent = this};
    }

    public required ToggleStateManager<T> ToggleManager { get; set; }
    public T ToggleState => ToggleManager.Current.Value;

    public bool StartWidthBuiltInText { get; set; } = false;
    private bool _adjustedText;

    public void Select(T state)
    {
        ToggleManager.Select(state);
        SetText();
    }

    protected override void OnMouseLeftDown(MouseEventArgs e)
    {
        ToggleManager.Next();
        SetText();

        base.OnMouseLeftDown(e);
    }

    public override void Resize()
    {
        if (ToggleManager.Count > 0 && !StartWidthBuiltInText) SetText();

        base.Resize();
    }

    public override void Render()
    {
        if (!_adjustedText && !StartWidthBuiltInText)
        {
            SetText();
            _adjustedText = true;
        }

        base.Render();
    }

    private void SetText() => Text.String = ToggleManager.Current.Text;
}

public class ToggleStateManager<T>
{
    public ToggleStateManager(string name = "", params T[] states)
    {
        foreach (var state in states)
        {
            States.Add(new StateEntry(name, $"{name}: {state}", state));
        }
    }

    public readonly record struct StateEntry(string Name, string Text, T Value);

    public readonly List<StateEntry> States = new();
    public int StateIndex { get; private set; }
    public StateEntry Current => States[StateIndex];

    public int Count => States.Count;

    public void Next()
    {
        StateIndex++;

        if (StateIndex >= States.Count) StateIndex = 0;
    }

    public void Select(T state)
    {
        for (var i = 0; i < Count; i++)
        {
            if (!EqualityComparer<T>.Default.Equals(States[i].Value, state)) continue;

            StateIndex = i;
            return;
        }

        throw new ArgumentException("State to select not present in states list.", nameof(state));
    }
}