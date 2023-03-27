using ConsoleGUI.ConsoleDisplay;
using ConsoleGUI.Utils;
using ConsoleGUI.Visuals;

namespace ConsoleGUI.UI.Widgets;

public class Grid : Control
{
    public Grid()
    {
        Focusable = false;
        RenderWhenIsRoot = true;

        Children.ElementChanged += ChildrenOnElementChanged;

        Columns.CollectionChanged += ColumnsOnCollectionChanged;
        Rows.CollectionChanged += RowsOnCollectionChanged;
    }

    public ObservableList<Control> Children { get; } = new();

    public readonly GridElementCollection<Column> Columns = new();
    public readonly GridElementCollection<Row> Rows = new();

    public GridResizeDirection GridResizeDirection { get; set; } = GridResizeDirection.Both;
    public VerticalAlignment VerticalAlignment { get; set; } = VerticalAlignment.Middle;
    public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Middle;

    public bool ShowGridLines { get; set; }
    public Color GridLinesColor { get; set; } = Color.White;
    public GridLineStyle GridLineStyle { get; set; } = GridLineStyle.Single;

    private readonly List<Entry> _entries = new();
    private bool _resizingContentNow;

    public void SetColumnAndRow(Control control, int column, int row, bool addChild = true)
    {
        if (column >= Columns.Count || column < 0)
        {
            throw new InvalidOperationException($"Invalid colum index. Value: {column}");
        }

        if (row >= Rows.Count || row < 0)
        {
            throw new InvalidOperationException($"Invalid row index. Value: {row}");
        }

        var entry = _entries.FirstOrDefault(e => e.RefTarget == control);
        var columnSpan = 1;
        var rowSpan = 1;

        if (entry == null)
        {
            if (addChild) Children.Add(control);

            _entries.Add(new Entry(control, column, row, 1, 1));
        }
        else
        {
            entry.Column = column;
            entry.Row = row;

            columnSpan = entry.ColumnSpan;
            rowSpan = entry.RowSpan;

            if (entry.Column + entry.ColumnSpan > Columns.Count)
            {
                throw new InvalidOperationException($"Invalid column index. Value: {column}");
            }

            if (entry.Row + entry.RowSpan > Rows.Count)
            {
                throw new InvalidOperationException($"Invalid row index. Value: {row}");
            }
        }

        PlaceControl(control, column, row, columnSpan, rowSpan);
        Resize();
    }

    public void SetColumnSpanAndRowSpan(Control control, int columnSpan, int rowSpan)
    {
        if (columnSpan > Columns.Count || columnSpan <= 0)
        {
            throw new InvalidOperationException($"Invalid column span. Value: {columnSpan}");
        }

        if (rowSpan > Rows.Count || rowSpan <= 0)
        {
            throw new InvalidOperationException($"Invalid row span. Value: {rowSpan}");
        }

        var entry = _entries.FirstOrDefault(e => e.Reference.Target == control);
        var column = 0;
        var row = 0;

        if (entry == null)
        {
            _entries.Add(new Entry(control, 0, 0, columnSpan, rowSpan));
        }
        else
        {
            entry.ColumnSpan = columnSpan;
            entry.RowSpan = rowSpan;

            column = entry.Column;
            row = entry.Row;

            if (entry.Column + entry.ColumnSpan > Columns.Count)
            {
                throw new InvalidOperationException($"Invalid column span. Value: {columnSpan}");
            }

            if (entry.Row + entry.RowSpan > Rows.Count)
            {
                throw new InvalidOperationException($"Invalid row span. Value: {rowSpan}");
            }
        }

        PlaceControl(control, column, row, columnSpan, rowSpan);
        Resize();
    }

    private void PlaceControl(Control control, int column, int row, int columnSpan, int rowSpan)
    {
        _resizingContentNow = true;

        if (control is not ContentControl {Content: not null})
        {
            control.Resize();
        }

        AdjustCellSize(control.PaddedSize, column, row, columnSpan, rowSpan);

        var baseOffset = new Vector
        {
            X = Columns.Offset(column) + InnerPadding.X,
            Y = Rows.Offset(row) + InnerPadding.Y
        };

        CalculatePosition(control, baseOffset + control.OuterPadding, column, row, columnSpan, rowSpan);

        _resizingContentNow = false;
    }

    private void CalculatePosition(Control control, Vector offset, int column, int row, int columnSpan, int rowSpan)
    {
        offset.X += HorizontalAlignment switch
        {
            HorizontalAlignment.Middle => (Columns.SpanSize(column, columnSpan) - control.PaddedWidth) / 2,
            HorizontalAlignment.Right => Columns.SpanSize(column, columnSpan) - control.PaddedWidth,
            _ => 0
        };

        offset.Y += VerticalAlignment switch
        {
            VerticalAlignment.Middle => (Rows.SpanSize(row, rowSpan) - control.PaddedHeight) / 2,
            VerticalAlignment.Bottom => Rows.SpanSize(row, rowSpan) - control.PaddedHeight,
            _ => 0
        };

        control.Position = offset;
    }

    private void AdjustCellSize(Vector size, int column, int row, int columnSpan, int rowSpan)
    {
        var availableWidth = Columns.SpanSize(column, columnSpan);
        var availableHeight = Rows.SpanSize(row, rowSpan);

        if (availableWidth < size.X &&
            GridResizeDirection is GridResizeDirection.Horizontal or GridResizeDirection.Both)
        {
            Columns.MatchSize(column, columnSpan, size.X);
        }

        if (availableHeight < size.Y &&
            GridResizeDirection is GridResizeDirection.Vertical or GridResizeDirection.Both)
        {
            Rows.MatchSize(row, rowSpan, size.Y);
        }
    }

    private void AdjustContentPosition()
    {
        _resizingContentNow = true;

        foreach (var entry in _entries)
        {
            if (entry.RefTarget is not { } control) continue;

            if (control.ResizeMode == ResizeMode.Expand)
            {
                var expandSize = new Vector
                {
                    X = Columns.SpanSize(entry.Column, entry.ColumnSpan),
                    Y = Rows.SpanSize(entry.Row, entry.RowSpan)
                };

                expandSize -= control.OuterPadding * 2;

                control.Expand(expandSize);
            }

            var baseOffset = new Vector
            {
                X = Columns.Offset(entry.Column),
                Y = Rows.Offset(entry.Row)
            };

            baseOffset += InnerPadding + control.OuterPadding;

            CalculatePosition(control, baseOffset, entry.Column, entry.Row, entry.ColumnSpan, entry.RowSpan);
        }

        _entries.RemoveAll(e => !e.Reference.IsAlive);

        _resizingContentNow = false;
    }

    private void ChildrenOnElementChanged(object? sender, CollectionChangedEventArgs<Control> e)
    {
        if (e.ChangeType == ChangeType.Remove && e.Element.Parent == this)
        {
            e.Element.Parent = null;
        }

        else if (e.ChangeType == ChangeType.Add)
        {
            e.Element.Parent = this;
        }
    }

    private void ColumnsOnCollectionChanged(object? sender, EventArgs e)
    {
        Columns.SetEvenSizes(InnerWidth);

        if (_entries.Count > 0) Resize();
    }

    private void RowsOnCollectionChanged(object? sender, EventArgs e)
    {
        Rows.SetEvenSizes(InnerHeight);

        if (_entries.Count > 0) Resize();
    }

    private void RenderLines()
    {
        var columns = Columns.Count;
        var rows = Rows.Count;

        Span<Vector> crosses = stackalloc Vector[(columns - 1) * (rows - 1)];

        var linesStart = GlobalPosition + InnerPadding;
        var pos = linesStart;

        for (var i = 0; i < columns - 1; i++)
        {
            pos.X = linesStart.X + Columns.Offset(i) + Columns[i].Size;

            Display.DrawLine(pos, Vector.Down, InnerHeight, GridLinesColor, CurrentColor,
                GridLines.Symbols[GridLineStyle][GridLineFragment.Vertical]);

            for (var c = i; c < crosses.Length; c += columns - 1)
            {
                crosses[c].X = pos.X;
            }
        }

        pos = linesStart;

        for (var i = 0; i < rows - 1; i++)
        {
            pos.Y = linesStart.Y + Rows.Offset(i) + Rows[i].Size;
            
            Display.DrawLine(pos, Vector.Right, InnerWidth, GridLinesColor, CurrentColor,
                GridLines.Symbols[GridLineStyle][GridLineFragment.Horizontal]);

            for (var c = 0; c < columns - 1; c++)
            {
                var index = c + i * (columns - 1);

                crosses[index].Y = pos.Y;
            }
        }

        foreach (var crossPosition in crosses)
        {
            Display.Draw(crossPosition.X, crossPosition.Y, GridLines.Symbols[GridLineStyle][GridLineFragment.Cross],
                GridLinesColor, CurrentColor);
        }
    }

    public override void Render()
    {
        base.Render();

        if (ShowGridLines) RenderLines();
    }

    public override void Remove()
    {
        base.Remove();

        foreach (var child in Children)
        {
            child.Parent = null;
            child.Remove();
        }

        Children.Clear();
    }

    public override void Resize()
    {
        if (_resizingContentNow) return;

        Span<int> columnsMinimumWidth = stackalloc int[Columns.Count];
        Span<int> rowsMinimumHeight = stackalloc int[Rows.Count];

        var multiCellEntries = new List<Entry>();

        foreach (var entry in _entries)
        {
            if (entry.RefTarget is not { } control) continue;

            if (entry.MultiCell)
            {
                multiCellEntries.Add(entry);
                continue;
            }

            var minSize = control.RequiredSpace;

            if (minSize.X > columnsMinimumWidth[entry.Column]) columnsMinimumWidth[entry.Column] = minSize.X;
            if (minSize.Y > rowsMinimumHeight[entry.Row]) rowsMinimumHeight[entry.Row] = minSize.Y;
        }

        for (var i = 0; i < Columns.Count; i++)
        {
            Columns[i].Size = columnsMinimumWidth[i];
        }

        for (var i = 0; i < Rows.Count; i++)
        {
            Rows[i].Size = rowsMinimumHeight[i];
        }

        foreach (var entry in multiCellEntries)
        {
            if (entry.RefTarget is not { } control) continue;
            var minSize = control.RequiredSpace;

            Columns.MatchSize(entry.Column, entry.ColumnSpan, minSize.X);
            Rows.MatchSize(entry.Row, entry.RowSpan, minSize.Y);
        }

        Width = Columns.TotalSize + InnerPadding.X * 2;
        Height = Rows.TotalSize + InnerPadding.Y * 2;

        AdjustContentPosition();
    }

    private class Entry
    {
        public Entry(Control control, int column, int row, int columnSpan, int rowSpan)
        {
            Reference = new WeakReference(control);
            Column = column;
            Row = row;
            ColumnSpan = columnSpan;
            RowSpan = rowSpan;
        }

        public WeakReference Reference { get; }
        public Control? RefTarget => Reference.Target as Control;

        public int Column { get; set; }
        public int Row { get; set; }

        public int ColumnSpan { get; set; }
        public int RowSpan { get; set; }
        public bool MultiCell => ColumnSpan > 1 || RowSpan > 1;
    }
}

public enum HorizontalAlignment
{
    Left,
    Right,
    Middle
}

public enum VerticalAlignment
{
    Top,
    Bottom,
    Middle
}

public enum GridResizeDirection
{
    None,
    Vertical,
    Horizontal,
    Both
}
