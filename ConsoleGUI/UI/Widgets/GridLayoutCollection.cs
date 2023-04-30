using System.Diagnostics.Contracts;
using ConsoleGUI.Utils;

namespace ConsoleGUI.UI.Widgets;

public class GridLayoutCollection<T> : ObservableList<GridLayoutElement> where T : GridLayoutElement
{
    public int Padding => 1;
    public int TotalPadding => Count > 0 ? Count - 1 : 0;
    public int TotalSize => this.Sum(element => element.Size) + TotalPadding;

    [Pure]
    public int StartOffset(int index)
    {
        if (index >= Count || index < 0) throw new IndexOutOfRangeException();

        var offset = 0;
        for (var i = 0; i < index; i++)
        {
            offset += this[i].Size + Padding;
        }

        return offset;
    }

    [Pure]
    public int EndOffset(int index) => StartOffset(index) + this[index].Size;

    public void MatchSize(int targetSize)
    {
        if (Count == 0) return;
        if (Count == 1)
        {
            var element = this[0];
            if (element.AutoSize) element.Size = targetSize;
            return;
        }

        var adjustableElements = this.Where(element => element.AutoSize).ToList();
        if (adjustableElements.Count == 0) return;

        var currentSize = TotalSize;

        if (currentSize < targetSize)
        {
            while (currentSize < targetSize)
            {
                adjustableElements.Sort((one, two) => one.Size.CompareTo(two.Size));
                var smallest = adjustableElements[0];
                
                if (smallest.Size == 0) return;
                
                smallest.Size++;
                currentSize++;
            }
        }
        else
        {
            while (currentSize > targetSize)
            {
                adjustableElements.Sort((one, two) => two.Size.CompareTo(one.Size));
                var biggest = adjustableElements[0];
                
                biggest.Size--;
                currentSize--;
            }
        }
    }
}