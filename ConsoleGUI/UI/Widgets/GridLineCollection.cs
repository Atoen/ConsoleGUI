using ConsoleGUI.Visuals;

namespace ConsoleGUI.UI.Widgets;

public class GridLineCollection
{
    public List<(Vector position, Vector direction, int length)> Lines { get; } = new();
    public List<(Vector position, GridLineFragment connector)> Connectors { get; } = new();

    public void AddLine(Vector position, Vector direction, int length)
    {
        Lines.Add(new ValueTuple<Vector, Vector, int>(position, direction, length));
    }

    public void AddConnector(Vector position, GridLineFragment connector)
    {
        Connectors.Add(new ValueTuple<Vector, GridLineFragment>(position, connector));
    }

    public void Clear()
    {
        Lines.Clear();
        Connectors.Clear();

        var a = new MyClass(13);
        int x = a;
    }
}

public class MyClass
{
    private int _value;

    public MyClass(int value)
    {
        _value = value;
    }

    public static implicit operator int(MyClass myClass)
    {
        return myClass._value;
    }
}