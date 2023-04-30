using ConsoleGUI.Attributes.Attributes;
using ConsoleGUI.UI.Old.Widgets;
using ConsoleGUI.Utils;
using ConsoleGUI.Visuals;

namespace ConsoleGUI.UI.Widgets;

public class Grid : Control, IContainer
{
    public Grid()
    {
        Focusable = false;

        _cellCollection = new GridCellCollection(this);
        _cellCollection.CollectionUpdated += CellCollectionOnCollectionUpdated;
        
        HandlerManager.AddHandlers<Grid>(new()
        {
            {(nameof(MinSize), nameof(MaxSize)), (_, _) => _cellCollection.UpdateTargetSize()},
            {nameof(Size), (component, _) => _cellCollection.UpdateTargetSize(component.InnerSize)}
        });
    }

    public ObservableList<Visual> ContainerElements { get; } = new();
    public bool FillScreen { get; set; }
    public GridLayoutCollection<Column> Columns => _cellCollection.Columns;
    public GridLayoutCollection<Row> Rows => _cellCollection.Rows;

    public GridLinesPreset Lines { get; set; } = new();

    private readonly List<Entry> _entries = new();
    private readonly GridCellCollection _cellCollection;

    [ValidateGridPlacement(GridValidationMode.ColumnAndRow)]
    public void SetColumnAndRow(Visual visual, int column, int row)
    {
        var entry = _entries.FirstOrDefault(e => e.Element == visual);

        var columnSpan = 1;
        var rowSpan = 1;

        if (entry is null)
        {
            ContainerElements.Add(visual);

            _entries.Add(new Entry(visual, column, row, 1, 1));
        }
        else
        {
            entry.Column = column;
            entry.Row = row;

            columnSpan = entry.ColumnSpan;
            rowSpan = entry.RowSpan;
        }

        PlaceElement(visual, column, row, columnSpan, rowSpan);
    }

    [ValidateGridPlacement(GridValidationMode.ColumnSpanAndRowSpan)]
    public void SetColumnSpanAndRowSpan(Visual visual, int columnSpan, int rowSpan)
    {
        var entry = _entries.FirstOrDefault(e => e.Element == visual);

        var column = 0;
        var row = 0;

        if (entry is null)
        {
            ContainerElements.Add(visual);

            _entries.Add(new Entry(visual, 0, 0, columnSpan, rowSpan));
        }
        else
        {
            entry.ColumnSpan = columnSpan;
            entry.RowSpan = rowSpan;

            column = entry.Column;
            row = entry.Row;
        }

        PlaceElement(visual, column, row, columnSpan, rowSpan);
    }

    private void PlaceElement(Visual visual, int column, int row, int columnSpan, int rowSpan)
    {

    }

    private void AdjustCellSize()
    {

    }

    private void CellCollectionOnCollectionUpdated(object? sender, EventArgs e)
    {
        var layoutSize = _cellCollection.Size + InnerPadding * 2;
        Size = layoutSize;
    }

    internal override void Render()
    {
        base.Render();

        if (Lines.Visible) _cellCollection.RenderLines();
    }

    internal class Entry
    {
        public Entry(Visual element, int column, int row, int columnSpan, int rowSpan)
        {
            Element = element;
            Column = column;
            Row = row;
            ColumnSpan = columnSpan;
            RowSpan = rowSpan;
        }

        public Visual Element { get; set; }

        public int Column { get; set; }
        public int Row { get; set; }

        public int ColumnSpan { get; set; }
        public int RowSpan { get; set; }
        public bool IsMultiCell => ColumnSpan > 1 || RowSpan > 1;
    }

    private class Cell
    {
        public Vector Size { get; set; }
        public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Middle;
        public VerticalAlignment VerticalAlignment { get; set; } = VerticalAlignment.Middle;
    }
}

public sealed class GridLinesPreset
{
    public bool Visible { get; set; }
    public Color Color { get; set; } = Color.White;
    public GridLineStyle Style { get; set; } = GridLineStyle.Single;
}

public sealed class Column : GridLayoutElement
{
    public Column(int width = 0) : base(width) { }
    public Column(Relative relativeSize) : base(relativeSize) { }
}

public sealed class Row : GridLayoutElement
{
    public Row(int height = 0) : base(height) { }
    public Row(Relative relativeSize) : base(relativeSize) { }
}

public class GridLayoutElement
{
    protected GridLayoutElement(int size)
    {
        Size = size;

        if (Size == 0) AutoSize = true;
    }

    protected GridLayoutElement(Relative relativeSize)
    {
        RelativeSize = relativeSize;
        AutoSize = true;
    }

    public Relative RelativeSize { get; set; } = Relative.Default;
    public int Size { get; set; }
    public bool AutoSize { get; set; }
}

public enum Relative
{
    Default,
    One,
    Two,
    Three,
    Four
}