using ConsoleGUI.Utils;

namespace ConsoleGUI.UI.Widgets;

public sealed class GridElementCollection<T> : ObservableList<T> where T : class, IGridLayoutElement
{
    public int Padding => 1;

    public int TotalPadding => Count > 0 ? Count - 1 : 0;
    public int TotalSize => this.Sum(e => e.Size) + TotalPadding;

    public int Offset(int index)
    {
        if (index >= Count || index < 0) throw new IndexOutOfRangeException();

        var offset = 0;
        for (var i = 0; i < index; i++)
        {
            offset += this[i].Size + Padding;
        }

        return offset;
    }

    public int SpanSize(int firstElement, int span)
    {
        var size = 0;
        if (span > 1) size = (span - 1) * Padding;

        for (var i = 0; i < span; i++)
        {
            size += this[firstElement + i].Size;
        }

        return size;
    }

    public void SetEvenSizes(int totalSize)
    {
        var sizeToDivide = totalSize - TotalPadding;

        var (size, remainder) = int.DivRem(sizeToDivide, Count);

        for (var i = 0; i < Count; i++)
        {
            var elementSize = size;
            if (remainder > 0)
            {
                elementSize++;
                remainder--;
            }

            this[i].Size = elementSize;
        }
    }

    public void MatchSize(int firstElement, int span, int totalSize)
    {
        if (span == 1)
        {
            this[firstElement].Size = totalSize;
            return;
        }

        var sizeToMatch = totalSize - (span - 1) * Padding;

        var slice = this.Skip(firstElement).Take(span).ToArray();

        var currentSize = slice.Sum(e => e.Size);

        while (currentSize < sizeToMatch)
        {
            var smallest = slice.OrderBy(e => e.Size).First();
            smallest.Size++;

            currentSize++;
        }
    }
}

public sealed class Column : IGridLayoutElement
{
    public int Size { get; set; }
    public GridUnitType UnitType { get; set; } = GridUnitType.Pixel;
}

public sealed class Row : IGridLayoutElement
{
    public int Size { get; set; }
    public GridUnitType UnitType { get; set; } = GridUnitType.Pixel;
}

public interface IGridLayoutElement
{
    public int Size { get; set; }
    public GridUnitType UnitType { get; set; }
}

public enum GridUnitType
{
    Auto,
    Pixel,
    Weighted
}
