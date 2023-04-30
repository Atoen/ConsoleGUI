using ConsoleGUI.ConsoleDisplay;
using ConsoleGUI.Utils;
using ConsoleGUI.Visuals;

namespace ConsoleGUI.UI.Widgets;

public class GridCellCollection
{
    public GridCellCollection(Grid grid)
    {
        _grid = grid;

        Columns.CollectionChanged += ColumnsOnCollectionChanged;
        Rows.CollectionChanged += RowsOnCollectionChanged;
    }

    public event EventHandler? CollectionUpdated;

    public GridLayoutCollection<Column> Columns { get; } = new();
    public GridLayoutCollection<Row> Rows { get; } = new();

    public Vector Size { get; private set; }

    private GridCell[,] _cells = new GridCell[0, 0];
    private readonly Grid _grid;

    private bool _shouldRegenerateLines;
    private readonly GridLineCollection _gridLines = new();

    public void UpdateTargetSize()
    {
        Columns.MatchSize(_grid.MinSize.X - _grid.InnerPadding.X * 2);
        Rows.MatchSize(_grid.MinSize.Y - _grid.InnerPadding.Y * 2);

        SetCellsSizes();

        _shouldRegenerateLines = true;
        
        CollectionUpdated?.Invoke(this, EventArgs.Empty);

    }
    
    public void UpdateTargetSize(Vector size)
    {
        Columns.MatchSize(size.X);
        Rows.MatchSize(size.Y);

        SetCellsSizes();

        _shouldRegenerateLines = true;
        
        CollectionUpdated?.Invoke(this, EventArgs.Empty);

    }

    internal void RenderLines()
    {
        if (_shouldRegenerateLines)
        {
            CalculateLinesPositions();
            _shouldRegenerateLines = false;
        }

        foreach (var (position, direction, length) in _gridLines.Lines)
        {
            var fragment = direction == Vector.Down ? GridLineFragment.Vertical : GridLineFragment.Horizontal;

            Display.DrawLine(position, direction, length, _grid.Lines.Color, Color.Empty,
                GridLines.Symbols[_grid.Lines.Style][fragment]);
        }

        foreach (var (position, fragment) in _gridLines.Connectors)
        {
            Display.Draw(position.X, position.Y, GridLines.Symbols[_grid.Lines.Style][fragment],
                _grid.Lines.Color, Color.Empty);
        }
    }

    private void CalculateLinesPositions()
    {
        _gridLines.Clear();

        var globalPos = _grid.GlobalPosition;
        var linesStart = globalPos + _grid.InnerPadding;
        var columns = Columns.Count;
        var rows = Rows.Count;

        var pos = linesStart;
        var columnLineLength = Math.Min(Rows.TotalSize, _grid.InnerHeight);

        for (var i = 0; i < columns - 1; i++)
        {
            pos.X = linesStart.X + Columns.EndOffset(i);
            _gridLines.AddLine(pos, Vector.Down, columnLineLength);
        }

        pos.X = linesStart.X;
        var rowsLineLength = Math.Min(Columns.TotalSize, _grid.InnerWidth);

        for (var i = 0; i < rows - 1; i++)
        {
            pos.Y = linesStart.Y + Rows.EndOffset(i);
            _gridLines.AddLine(pos, Vector.Right, rowsLineLength);
        }
    }

    private void ColumnsOnCollectionChanged(object? sender, EventArgs e)
    {
        _cells = new GridCell[Columns.Count, Rows.Count];
        Columns.MatchSize(_grid.InnerWidth);

        SetCellsSizes();

        CollectionUpdated?.Invoke(this, EventArgs.Empty);
    }

    private void RowsOnCollectionChanged(object? sender, EventArgs e)
    {
        _cells = new GridCell[Columns.Count, Rows.Count];
        Rows.MatchSize(_grid.InnerHeight);

        SetCellsSizes();

        CollectionUpdated?.Invoke(this, EventArgs.Empty);
    }
    
    private void SetCellsSizes()
    {
        for (var c = 0; c < Columns.Count; c++)
        for (var r = 0; r < Rows.Count; r++)
        {
            _cells[c, r].Width = Columns[c].Size;
            _cells[c, r].Height = Rows[r].Size;
        }

        var width = Columns.Select(c => c.Size).Sum();
        var columnsPadding = Columns.Count > 1 ? Columns.Count - 1 : 0;

        var height = Rows.Select(r => r.Size).Sum();
        var rowsPadding = Rows.Count > 1 ? Rows.Count - 1 : 0;

        Size = (width + columnsPadding, height + rowsPadding);
        

        _shouldRegenerateLines = true;
    }
}