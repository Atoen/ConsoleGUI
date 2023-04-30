using ConsoleGUI.Utils;

namespace ConsoleGUI.UI.Old.Widgets;

public sealed class GridElementCollection<T> : ObservableList<T> where T : GridLayoutElement
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

    public void FitSize(int size)
    {
        if (Count < 2) return;

        var manualSize = 0;
        var autoSizeElements = 0;

        Span<int> originalSizes = stackalloc int[Count];
        Span<int> autoSizeIndexes = stackalloc int[Count];

        for (var i = 0; i < Count; i++)
        {
            var element = this[i];

            originalSizes[i] = element.Size;

            if (!element.AutoSize)
            {
                manualSize += element.Size;
            }
            else
            {
                autoSizeIndexes[autoSizeElements] = i;
                autoSizeElements++;
            }
        }

        var sizeToDivide = size - manualSize;

        if (autoSizeElements > 0)
        {
            MatchSize(0, autoSizeElements, sizeToDivide);

            for (var i = 0; i < autoSizeElements; i++)
            {
                var size1 = this[autoSizeIndexes[i]].Size;
                var size2 = this[i].Size;

                this[i].Size = size1;
                this[autoSizeIndexes[i]].Size = size2;
            }
        }

        for (var i = 0; i < Count; i++)
        {
            var element = this[i];
            if (!element.AutoSize) element.Size = originalSizes[i];
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

        if (currentSize < sizeToMatch)
        {
            while (currentSize < sizeToMatch)
            {
                var smallest = slice.OrderBy(e => e.Size).First();
                smallest.Size++;

                currentSize++;
            }
        }
        else
        {
            while (currentSize > sizeToMatch)
            {
                var biggest = slice.OrderBy(e => e.Size).Last();
                biggest.Size--;

                currentSize--;
            }
        }
    }
}

public sealed class Column : GridLayoutElement
{
    public Column(int width = 0) : base(width) { }
}

public sealed class Row : GridLayoutElement
{
    public Row(int height = 0) : base(height) { }
}

public class GridLayoutElement
{
    protected GridLayoutElement(int size)
    {
        Size = size;

        if (Size == 0) AutoSize = true;
    }

    public int Size { get; set; }
    public bool AutoSize { get; set; }
}

