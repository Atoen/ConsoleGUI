using ConsoleGUI.UI.Events;

namespace ConsoleGUI.UI.Widgets;

public class ToggleButton<T> : Button
{
    public ToggleButton(string name = "")
    {
        Text.Content = nameof(ToggleButton<T>);
        _name = name;
    }

    public required T[] States
    {
        get => _states;
        init
        {
            if (value is not {Length: > 0})
            {
                throw new ArgumentException("States must be a non-empty array", nameof(States));
            }

            _states = value;
            CreateStates(Name, Connector);
        }
    }

    public string Name
    {
        get => _name;
        set
        {
            if (value != _name) CreateStates(value, Connector);
            _name = value;
        }
    }

    public string Connector
    {
        get => _connector;
        set
        {
            if (value != _connector) CreateStates(Name, value);
            _connector = value;
        }
    }

    public int Index
    {
        get => _index;
        private set => SetField(ref _index, value, onChanged: OnIndexChanged);
    }

    private void OnIndexChanged(int oldValue, int newValue)
    {
        Text.Content = CurrentState.Text;
    }

    protected override void OnMouseLeftDown(MouseEventArgs e)
    {
        Next();
        base.OnMouseLeftDown(e);
    }

    public T CurrentValue => StatesEntries[Index].Value;
    public StateEntry CurrentState => StatesEntries[Index];

    public readonly List<StateEntry> StatesEntries = new();
    public record StateEntry(string Text, T Value);

    private readonly T[] _states = null!;
    private string _name;
    private int _index;
    private string _connector = ": ";

    public void Next() => Index = (Index + 1) % States.Length;

    public void Select(T state)
    {
        for (var i = 0; i < States.Length; i++)
        {
            if (!EqualityComparer<T>.Default.Equals(StatesEntries[i].Value, state)) continue;

            Index = i;
            return;
        }

        throw new ArgumentException("Selected state not present in states list.", nameof(state));
    }

    private void CreateStates(string statesName, string connector)
    {
        StatesEntries.Clear();

        StatesEntries.AddRange(statesName == string.Empty
            ? _states.Select(state => new StateEntry($"{state}", state))
            : _states.Select(state => new StateEntry($"{statesName}{connector}{state}", state)));

        Text.Content = CurrentState.Text;
    }
}

