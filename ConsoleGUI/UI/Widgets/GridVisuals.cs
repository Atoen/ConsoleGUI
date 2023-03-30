using ConsoleGUI.ConsoleDisplay;
using ConsoleGUI.Visuals;

namespace ConsoleGUI.UI.Widgets;

public partial class Grid
{
    private readonly List<(Vector position, Vector direction, int length)> _lines = new();
    private readonly List<(Vector position, GridLineFragment fragment)> _lineConnectors = new();

    private bool[,] _verticalLineSegments = new bool[0, 0];
    private bool[,] _horizontalLineSegments = new bool[0, 0];

    private void CalculateLinesPositions()
    {
        _lines.Clear();
        _lineConnectors.Clear();

        var globalPos = GlobalPosition;
        var linesStart = globalPos + InnerPadding;
        var columns = Columns.Count;
        var rows = Rows.Count;

        for (var col = 0; col < columns - 1; col++)
        {
            var lineLength = 0;
            var pos = linesStart + new Vector(Columns.Offset(col) + Columns[col].Size, 0);
            var lastWasSkipped = false;

            for (var row = 0; row < rows; row++)
            {
                if (!_verticalLineSegments[col, row])
                {
                    if (lineLength == 0) continue;

                    _lines.Add((pos, Vector.Down, lineLength));

                    pos.Y = Rows.Offset(row) + globalPos.Y;
                    _lineConnectors.Add((pos, SelectConnector(col, row)));
                    pos.Y++;

                    lineLength = 0;
                    lastWasSkipped = true;
                }
                else
                {
                    if (lastWasSkipped)
                    {
                        pos.Y = Rows.Offset(row) + globalPos.Y;
                        _lineConnectors.Add((pos, SelectConnector(col, row)));
                        pos.Y++;
                        lastWasSkipped = false;
                    }

                    if (lineLength != 0)
                    {
                        _lineConnectors.Add((pos + new Vector(0, lineLength), SelectConnector(col, row)));
                        lineLength += Rows.Padding;
                    }

                    lineLength += Rows[row].Size;
                }
            }

            if (lineLength > 0)
            {
                _lines.Add((pos, Vector.Down, lineLength));
            }
        }

        for (var i = 0; i < rows - 1; i++)
        {
            var lineLength = 0;
            var lastWasSkipped = false;
            var pos = linesStart with {Y = linesStart.Y + Rows.Offset(i) + Rows[i].Size};

            for (var l = 0; l < columns; l++)
            {
                if (!_horizontalLineSegments[l, i])
                {
                    if (lineLength == 0) continue;

                    _lines.Add((pos, Vector.Right, lineLength));
                    lineLength = 0;
                    lastWasSkipped = true;

                    continue;
                }

                if (lastWasSkipped)
                {
                    pos.X = Columns.Offset(l) + globalPos.X;
                    pos.X++;
                    lastWasSkipped = false;
                }

                if (lineLength != 0)
                {
                    lineLength += Columns.Padding;
                }
                lineLength += Columns[l].Size;
            }

            if (lineLength > 0)
            {
                _lines.Add((pos, Vector.Right, lineLength));
            }
        }
    }

    private GridLineFragment SelectConnector(int x, int y)
    {
        var direction = ConnectorDirection.None;

        if (_verticalLineSegments[x, y]) direction |= ConnectorDirection.Down;
        if (_verticalLineSegments[x, y - 1]) direction |= ConnectorDirection.Up;

        if (_horizontalLineSegments[x, y - 1]) direction |= ConnectorDirection.Left;
        if (_horizontalLineSegments[x + 1, y - 1]) direction |= ConnectorDirection.Right;

        return direction switch
        {
            ConnectorDirection.All => GridLineFragment.Cross,
            ConnectorDirection.Vertical | ConnectorDirection.Left => GridLineFragment.VerticalLeft,
            ConnectorDirection.Vertical | ConnectorDirection.Right => GridLineFragment.VerticalRight,
            ConnectorDirection.Horizontal | ConnectorDirection.Down => GridLineFragment.HorizontalDown,
            ConnectorDirection.Horizontal | ConnectorDirection.Up => GridLineFragment.HorizontalUp,
            _ => GridLineFragment.Cross
        };
    }

    private void RenderLines()
    {
        if (_shouldRegenerateLines)
        {
            CalculateLinesPositions();
            _shouldRegenerateLines = false;
        }

        foreach (var (position, direction, length) in _lines)
        {
            var fragment = direction == Vector.Down ? GridLineFragment.Vertical : GridLineFragment.Horizontal;

            Display.DrawLine(position, direction, length, GridLinesColor, Color.Empty,
                GridLines.Symbols[GridLineStyle][fragment]);
        }

        foreach (var (position, fragment) in _lineConnectors)
        {
            Display.Draw(position.X, position.Y, GridLines.Symbols[GridLineStyle][fragment], GridLinesColor,
                Color.Empty);
        }
    }

    private void CreateLineSegments()
    {
        var columnsSpacers = Columns.Count > 0 ? Columns.Count - 1 : 0;
        var rowsSpacers = Rows.Count > 0 ? Rows.Count - 1 : 0;

        _verticalLineSegments = new bool[columnsSpacers, Rows.Count];
        _horizontalLineSegments = new bool[Columns.Count, rowsSpacers];

        for (var i = 0; i < columnsSpacers; i++)
        for (var j = 0; j < Rows.Count; j++)
        {
            _verticalLineSegments[i, j] = true;
        }

        for (var i = 0; i < Columns.Count; i++)
        for (var j = 0; j < rowsSpacers; j++)
        {
            _horizontalLineSegments[i, j] = true;
        }
    }

    private void HideLinesForMultiCell(int column, int row, int columnSpan, int rowSpan)
    {
        for (var i = column; i < column + columnSpan; i++)
        for (var j = row; j < row + rowSpan - 1; j++)
        {
            _horizontalLineSegments[i, j] = false;
        }

        for (var i = column; i < column + columnSpan - 1; i++)
        for (var j = row; j < row + rowSpan; j++)
        {
            _verticalLineSegments[i, j] = false;
        }
    }
}

[Flags]
file enum ConnectorDirection
{
    None = 0,
    Up = 1,
    Down = 2,
    Left = 4,
    Right = 8,
    Vertical = Up | Down,
    Horizontal = Left | Right,
    All = Up | Down | Left | Right
}