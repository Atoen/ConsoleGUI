﻿using ConsoleGUI.ConsoleDisplay;
using ConsoleGUI.UI.Widgets;

namespace ConsoleGUI.UI.Old;

public abstract class OldComponent
{
    public State State { get; protected set; } = State.Default;
    public bool Enabled { get; private set; } = true;
    public bool Active { get; set; } = true;

    public ResizeMode ResizeMode { get; set; } = ResizeMode.Stretch;
    public ZIndexUpdateMode ZIndexUpdateMode { get; set; } = ZIndexUpdateMode.OneHigherThanParent;

    private int _zIndex;
    public int ZIndex
    {
        get => GetZIndexInternal();
        set => SetZIndexInternal(value);
    }

    private OldComponent? _parent;
    public OldComponent? Parent
    {
        get => _parent;
        set => SetParentInternal(value);
    }

    public delegate void PositionChangeEventHandler(OldComponent sender, PositionChangedEventArgs e);
    public delegate void SizeChangeEventHandler(OldComponent sender, SizeChangedEventArgs e);
    public delegate void ZIndexChangeEventHandler(OldComponent sender, ZIndexChangedEventArgs e);

    public event PositionChangeEventHandler? PositionChanged;
    public event SizeChangeEventHandler? SizeChanged;
    public event ZIndexChangeEventHandler? ZIndexChanged;

    public Vector InnerPadding = new(1, 1);
    public Vector OuterPadding = Vector.Zero;

    public Vector PaddedSize => Size + OuterPadding * 2;
    public int PaddedWidth => Width + OuterPadding.X * 2;
    public int PaddedHeight => Height + OuterPadding.Y * 2;

    public Vector InnerSize => Size - InnerPadding * 2;
    public int InnerWidth => Width - InnerPadding.X * 2;
    public int InnerHeight => Height - InnerPadding.Y * 2;

    public Vector MinSize { get; set; }
    public Vector MaxSize { get; set; }

    internal Vector RequiredSpace => ResizeMode == ResizeMode.Expand ? MinSize + OuterPadding * 2 : PaddedSize;

    private Vector _localPosition;
    private Vector _globalPosition;

    public Vector Position
    {
        get => _localPosition;
        set => SetPositionInternal(value, true);
    }

    public Vector GlobalPosition
    {
        get => _parent == null ? _localPosition : _parent.GlobalPosition + _localPosition;
        set => SetPositionInternal(value, false);
    }

    public Vector Center
    {
        get => GlobalPosition + Size / 2;
        set
        {
            GlobalPosition = value - Size / 2;

            if (_parent != null)
            {
                Position = GlobalPosition - _parent.GlobalPosition;
            }
        }
    }

    private Vector _size;

    public Vector Size
    {
        get => _size;

        set
        {
            var sizeBefore = _size;
            _size = value;

            if (value != sizeBefore)
            {
                SizeChanged?.Invoke(this, new SizeChangedEventArgs(sizeBefore, value));
            }
        }
    }

    public int Width
    {
        get => _size.X;
        set
        {
            var widthBefore = _size.X;
            _size.X = value;

            if (value != widthBefore)
            {
                SizeChanged?.Invoke(this, new SizeChangedEventArgs(_size with {X = widthBefore}, _size));
            }
        }
    }

    public int Height
    {
        get => _size.Y;
        set
        {
            var heightBefore = _size.Y;
            _size.Y = value;

            if (value != heightBefore)
            {
                SizeChanged?.Invoke(this, new SizeChangedEventArgs(_size with {Y = heightBefore}, _size));
            }
        }
    }

    protected void ApplyResizing()
    {
        Size = ResizeMode switch
        {
            ResizeMode.Grow or ResizeMode.Expand => Size.ExpandTo(MinSize),
            ResizeMode.Stretch => MinSize,
            _ => Size
        };
    }

    public virtual void Resize()
    {
    }

    internal void Expand(Vector maxSize = default)
    {
        if (maxSize == default)
        {
            maxSize = Parent?.InnerSize ?? Display.Size;
        }

        Size = MinSize.ExpandTo(maxSize);
    }

    public void SetEnabled(bool enabled)
    {
        if (!Enabled && enabled) State = State.Default;
        else if (!enabled) State = State.Default;

        Enabled = enabled;
    }

    public bool ContainsPoint(Vector pos)
    {
        return pos.X >= GlobalPosition.X && pos.X < GlobalPosition.X + Width &&
               pos.Y >= GlobalPosition.Y && pos.Y < GlobalPosition.Y + Height;
    }

    private void SetZIndexInternal(int value)
    {
        if (ZIndexUpdateMode != ZIndexUpdateMode.Manual) return;

        var indexBefore = GetZIndexInternal();

        _zIndex = value;

        if (value != indexBefore)
        {
            ZIndexChanged?.Invoke(this, new ZIndexChangedEventArgs(indexBefore, value));
        }
    }

    private int GetZIndexInternal()
    {
        if (ZIndexUpdateMode == ZIndexUpdateMode.Manual || _parent == null) return _zIndex;

        return ZIndexUpdateMode switch
        {
            ZIndexUpdateMode.SameAsParent => _parent.ZIndex,
            ZIndexUpdateMode.OneHigherThanParent => _parent.ZIndex + 1,
            ZIndexUpdateMode.TwoHigherThatParent => _parent.ZIndex + 2,
            _ => _zIndex
        };
    }

    private void SetPositionInternal(Vector value, bool isLocal)
    {
        var positionBefore = isLocal ? _localPosition : _globalPosition;

        if (_parent == null)
        {
            _globalPosition = value;
            _localPosition = value;
        }
        else
        {
            if (isLocal)
            {
                _localPosition = value;
                _globalPosition = _localPosition + _parent.GlobalPosition;
            }
            else
            {
                _globalPosition = value;
                _localPosition = value - _parent.GlobalPosition;
            }
        }

        if (value != positionBefore)
        {
            PositionChanged?.Invoke(this, new PositionChangedEventArgs(positionBefore, value));
        }
    }

    private void SetParentInternal(OldComponent? value)
    {
        if (value == this)
        {
            throw new InvalidOperationException($"Component {value} cannot be its own parent.");
        }

        if (value != _parent && _parent != null)
        {
            _parent.PositionChanged -= OnPositionChanged;
            SizeChanged -= _parent.OnChildSizeChanged;
        }

        var oldZIndex = GetZIndexInternal();

        if (value == null)
        {
            Position = GlobalPosition;
            _parent = null;

            return;
        }

        _parent = value;
        _localPosition = _globalPosition - _parent.GlobalPosition;

        var newZIndex = GetZIndexInternal();

        if (oldZIndex != newZIndex)
        {
            ZIndexChanged?.Invoke(this, new ZIndexChangedEventArgs(oldZIndex, newZIndex));
        }

        _parent.PositionChanged += OnPositionChanged;
        SizeChanged += _parent.OnChildSizeChanged;
    }

    protected virtual void OnPositionChanged(OldComponent sender, PositionChangedEventArgs e)
    {
        PositionChanged?.Invoke(sender, e);
    }

    protected virtual void OnChildSizeChanged(OldComponent sender, SizeChangedEventArgs e)
    {
        if (ResizeMode != ResizeMode.Manual)
        {
            Resize();
        }
    }
}

public class PositionChangedEventArgs : EventArgs
{
    public PositionChangedEventArgs(Vector oldPos, Vector newPos)
    {
        OldPos = oldPos;
        NewPos = newPos;
    }

    public Vector OldPos { get; }
    public Vector NewPos { get; }
}

public class SizeChangedEventArgs : EventArgs
{
    public SizeChangedEventArgs(Vector oldSize, Vector newSize)
    {
        OldSize = oldSize;
        NewSize = newSize;
    }

    public Vector OldSize { get; }
    public Vector NewSize { get; }
}

public class ZIndexChangedEventArgs : EventArgs
{
    public ZIndexChangedEventArgs(int oldIndex, int newIndex)
    {
        OldIndex = oldIndex;
        NewIndex = newIndex;
    }

    public int OldIndex { get; }
    public int NewIndex { get; }
}
