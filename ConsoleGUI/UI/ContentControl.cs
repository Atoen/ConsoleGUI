namespace ConsoleGUI.UI;

public abstract class ContentControl : Control, IContent
{
    private Control? _content;
    public Control? Content
    {
        get => _content;
        set => SetContent(value);
    }

    public override void Remove()
    {
        if (_content != null)
        {
            _content.Parent = null;
            _content.Remove();
        }

        base.Remove();
    }

    public override void Clear()
    {
        base.Clear();
        Content?.Clear();
    }

    public override void Resize()
    {
        if (_content == null) return;

        var contentSize = _content.PaddedSize + InnerPadding * 2;
        MinSize = MinSize.ExpandTo(contentSize);

        ApplyResizing();

        _content.Center = Center;
    }

    private void SetContent(Control? value)
    {
        if (value == this)
        {
            throw new InvalidOperationException($"Control {value} cannot be its own content.");
        }

        if (_content != null)
        {
            _content.Parent = null;
            _content.Remove();
        }

        _content = value;

        if (_content == null)
        {
            Resize();
            return;
        }

        _content.Parent = this;

        Resize();
        _content.Resize();
        if (_content.ResizeMode == ResizeMode.Expand) _content.Expand();
    }
}

public interface IContent
{
    public Control? Content { get; set; }
}
