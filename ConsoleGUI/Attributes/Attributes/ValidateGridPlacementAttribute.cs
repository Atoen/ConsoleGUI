using System.Reflection;
using ConsoleGUI.UI.Widgets;
using MethodBoundaryAspect.Fody.Attributes;

namespace ConsoleGUI.Attributes.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class ValidateGridPlacementAttribute : OnMethodBoundaryAspect
{

    public ValidateGridPlacementAttribute(GridValidationMode validationMode)
    {
        ValidationMode = validationMode;
    }

    public GridValidationMode ValidationMode { get; }

    public override void OnEntry(MethodExecutionArgs arg)
    {
        ValidateGridEntry(arg);
    }

    private void ValidateGridEntry(MethodExecutionArgs arg)
    {
        if (arg is not {Instance: Grid grid, Arguments: [Visual visual, int columnOrSpan, int rowOrSpan]})
        {
            throw new TargetException();
        }

        var columns = grid.Columns;
        var rows = grid.Rows;

        if (ValidationMode == GridValidationMode.ColumnAndRow)
        {
            if (columnOrSpan >= columns.Count || columnOrSpan < 0)
            {
                throw new InvalidOperationException($"Invalid colum index. Value: {columnOrSpan}");
            }

            if (rowOrSpan >= rows.Count || rowOrSpan < 0)
            {
                throw new InvalidOperationException($"Invalid row index. Value: {rowOrSpan}");
            }
        }
        else
        {
            if (columnOrSpan > columns.Count || columnOrSpan <= 0)
            {
                throw new InvalidOperationException($"Invalid column span. Value: {columnOrSpan}");
            }

            if (rowOrSpan > rows.Count || rowOrSpan <= 0)
            {
                throw new InvalidOperationException($"Invalid row span. Value: {rowOrSpan}");
            }
        }

        var type = grid.GetType();
        var field = type.GetField("_entries", BindingFlags.NonPublic | BindingFlags.Instance);
        var entries = (List<Grid.Entry>) field!.GetValue(grid)!;

        var entry = entries.FirstOrDefault(e => e.Element == visual);

        if (entry is null) return;

        if (ValidationMode == GridValidationMode.ColumnAndRow)
        {
            if (columnOrSpan + entry.ColumnSpan > columns.Count)
            {
                throw new InvalidOperationException($"Invalid column index. Value: {columnOrSpan}");
            }

            if (rowOrSpan + entry.RowSpan > rows.Count)
            {
                throw new InvalidOperationException($"Invalid row index. Value: {rowOrSpan}");
            }
        }
        else
        {
            if (entry.Column + columnOrSpan > columns.Count)
            {
                throw new InvalidOperationException($"Invalid column span. Value: {columnOrSpan}");
            }

            if (entry.Row + rowOrSpan > rows.Count)
            {
                throw new InvalidOperationException($"Invalid row span. Value: {rowOrSpan}");
            }
        }
    }
}

public enum GridValidationMode
{
    ColumnAndRow,
    ColumnSpanAndRowSpan
}