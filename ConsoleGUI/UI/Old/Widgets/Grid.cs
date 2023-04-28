using ConsoleGUI.ConsoleDisplay;
using ConsoleGUI.UI.Widgets;
using ConsoleGUI.Utils;
using ConsoleGUI.Visuals;

namespace ConsoleGUI.UI.Old.Widgets;

public partial class Grid : OldControl
{
    public Grid()
    {
        Focusable = false;
        RenderWhenIsRoot = true;

        Children.ElementChanged += ChildrenOnElementChanged;

        Columns.CollectionChanged += ColumnsOnCollectionChanged;
        Rows.CollectionChanged += RowsOnCollectionChanged;
    }

    public ObservableList<OldControl> Children { get; } = new();

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
    private bool _shouldRegenerateLines;

    public void SetColumnAndRow(OldControl oldControl, int column, int row, bool addChild = true)
    {
        if (column >= Columns.Count || column < 0)
        {
            throw new InvalidOperationException($"Invalid colum index. Value: {column}");
        }

        if (row >= Rows.Count || row < 0)
        {
            throw new InvalidOperationException($"Invalid row index. Value: {row}");
        }

        var entry = _entries.FirstOrDefault(e => e.RefTarget == oldControl);
        var columnSpan = 1;
        var rowSpan = 1;

        if (entry == null)
        {
            if (addChild) Children.Add(oldControl);

            _entries.Add(new Entry(oldControl, column, row, 1, 1));
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

        PlaceControl(oldControl, column, row, columnSpan, rowSpan);
        Resize();
    }

    public void SetColumnSpanAndRowSpan(OldControl oldControl, int columnSpan, int rowSpan)
    {
        if (columnSpan > Columns.Count || columnSpan <= 0)
        {
            throw new InvalidOperationException($"Invalid column span. Value: {columnSpan}");
        }

        if (rowSpan > Rows.Count || rowSpan <= 0)
        {
            throw new InvalidOperationException($"Invalid row span. Value: {rowSpan}");
        }

        var entry = _entries.FirstOrDefault(e => e.Reference.Target == oldControl);
        var column = 0;
        var row = 0;

        if (entry == null)
        {
            _entries.Add(new Entry(oldControl, 0, 0, columnSpan, rowSpan));
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

        PlaceControl(oldControl, column, row, columnSpan, rowSpan);
        Resize();
    }

    public void FitToScreen()
    {
        if (ResizeMode != ResizeMode.Expand) return;

        ScreenResizer.ScreenResized += delegate
        {
            Resize();
        };
    }

    private void ChildrenOnElementChanged(object? sender, CollectionChangedEventArgs<OldControl> e)
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
        CreateLineSegments();
        // Columns.SetEvenSizes(InnerWidth);
        Columns.FitSize(Display.Width - 2 * (InnerPadding.X + OuterPadding.X));

        if (_entries.Count > 0 || ResizeMode == ResizeMode.Expand) Resize();
    }

    private void RowsOnCollectionChanged(object? sender, EventArgs e)
    {
        CreateLineSegments();
        Rows.FitSize(Display.Height - 2 * (InnerPadding.Y + OuterPadding.Y));

        if (_entries.Count > 0 || ResizeMode == ResizeMode.Expand) Resize();
    }

    private void PlaceControl(OldControl oldControl, int column, int row, int columnSpan, int rowSpan)
    {
        _resizingContentNow = true;

        if (oldControl is not ContentOldControl {Content: not null})
        {
            oldControl.Resize();
        }

        AdjustCellSize(oldControl.PaddedSize, column, row, columnSpan, rowSpan);

        var baseOffset = new Vector
        {
            X = Columns.Offset(column) + InnerPadding.X,
            Y = Rows.Offset(row) + InnerPadding.Y
        };

        CalculatePosition(oldControl, baseOffset + oldControl.OuterPadding, column, row, columnSpan, rowSpan);

        _resizingContentNow = false;
    }

    private void CalculatePosition(OldControl oldControl, Vector offset, int column, int row, int columnSpan, int rowSpan)
    {
        offset.X += HorizontalAlignment switch
        {
            HorizontalAlignment.Middle => (Columns.SpanSize(column, columnSpan) - oldControl.PaddedWidth) / 2,
            HorizontalAlignment.Right => Columns.SpanSize(column, columnSpan) - oldControl.PaddedWidth,
            _ => 0
        };

        offset.Y += VerticalAlignment switch
        {
            VerticalAlignment.Middle => (Rows.SpanSize(row, rowSpan) - oldControl.PaddedHeight) / 2,
            VerticalAlignment.Bottom => Rows.SpanSize(row, rowSpan) - oldControl.PaddedHeight,
            _ => 0
        };

        oldControl.Position = offset;
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
        _shouldRegenerateLines = true;
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

        if (ResizeMode == ResizeMode.Expand)
        {
            Expand();

            Columns.FitSize(Display.Width - 2 * (InnerPadding.X + OuterPadding.X));
            Rows.FitSize(Display.Height - 2 * (InnerPadding.Y + OuterPadding.Y));

            foreach (var entry in _entries)
            {
                if (entry is {RefTarget: not null, MultiCell: true})
                {
                    HideLinesForMultiCell(entry.Column, entry.Row, entry.ColumnSpan, entry.RowSpan);
                }
            }

            _shouldRegenerateLines = true;

        }
        else
        {
            ResizeStretch();
        }

        AdjustContentPosition();

    }

    private void ResizeStretch()
    {
        Span<int> columnsMinimumWidth = stackalloc int[Columns.Count];
        Span<int> rowsMinimumHeight = stackalloc int[Rows.Count];

        var multiCellEntries = new List<Entry>();

        foreach (var entry in _entries)
        {
            if (entry.RefTarget is not { } control) continue;

            if (entry.MultiCell)
            {
                HideLinesForMultiCell(entry.Column, entry.Row, entry.ColumnSpan, entry.RowSpan);

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
            var minSize = entry.RefTarget!.RequiredSpace;

            Columns.MatchSize(entry.Column, entry.ColumnSpan, minSize.X);
            Rows.MatchSize(entry.Row, entry.RowSpan, minSize.Y);
        }

        Width = Columns.TotalSize + InnerPadding.X * 2;
        Height = Rows.TotalSize + InnerPadding.Y * 2;
    }

    private class Entry
    {
        public Entry(OldControl oldControl, int column, int row, int columnSpan, int rowSpan)
        {
            Reference = new WeakReference(oldControl);
            Column = column;
            Row = row;
            ColumnSpan = columnSpan;
            RowSpan = rowSpan;
        }

        public WeakReference Reference { get; }
        public OldControl? RefTarget => Reference.Target as OldControl;

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


