using System.Reflection;
using System.Runtime.CompilerServices;
using ConsoleGUI.ConsoleDisplay;

namespace ConsoleGUI.UI.New;

public delegate void PropertyChangedEventHandler(object sender, PropertyChangedEventArgs args);

public class PropertyChangedEventArgs : EventArgs
{
    public PropertyChangedEventArgs(string propertyName, object? oldValue, object? newValue)
    {
        PropertyName = propertyName;
        NewValue = newValue;
        OldValue = oldValue;
    }

    public string PropertyName { get; }
    public object? OldValue { get; }
    public object? NewValue { get; }
}

public abstract class Component
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public bool Enabled
    {
        get => _enabled;
        private set => SetField(ref _enabled, value);
    }

    public Component? Parent
    {
        get => _parent;
        set => SetField(ref _parent, value);
    }

    public State State
    {
        get => _state;
        protected set => SetField(ref _state, value);
    }

    public ResizeMode ResizeMode
    {
        get => _resizeMode;
        set => SetField(ref _resizeMode, value);
    }

    public ZIndexUpdateMode ZIndexUpdateMode
    {
        get => _zIndexUpdateMode;
        set => SetField(ref _zIndexUpdateMode, value);
    }

    public int ZIndex
    {
        get => _zIndex;
        set => SetField(ref _zIndex, value);
    }

    public Vector MinSize
    {
        get => _minSize;
        set => SetField(ref _minSize, value, onChanged: OnMinSizeChanged);
    }

    public Vector MaxSize
    {
        get => _maxSize;
        set => SetField(ref _maxSize, value, onChanged: OnMaxSizeChanged);
    }

    public Vector Size
    {
        get => _size;
        set => SetField(ref _size, value, ValidateSize);
    }

    public int Width
    {
        get => Size.X;
        set => SetField(ref _size, (value, Height), ValidateSize);
    }

    public int Height
    {
        get => Size.Y;
        set => SetField(ref _size, (Width, value), ValidateSize);
    }

    public Vector Position
    {
        get => _position;
        set => SetField(ref _position, value, onChanged: OnPositionChanged);
    }

    public Vector GlobalPosition
    {
        get => _globalPosition;
        set => SetField(ref _globalPosition, value, onChanged: OnGlobalPositionChanged);
    }

    public Vector Center
    {
        get => GlobalPosition + Size / 2;
        set => SetField(ref _globalPosition, value - Size / 2);
    }

    private readonly Dictionary<string, PropertyInfo> _propertyCache = new();

    private PropertyInfo GetPropertyInfo(string propertyName)
    {
        if (_propertyCache.TryGetValue(propertyName, out var propInfo)) return propInfo;

        propInfo = GetType().GetProperty(propertyName);

        if (propInfo is null) throw new ArgumentException($"{this} does not contain property {propertyName}.");
        _propertyCache.Add(propertyName, propInfo);

        return propInfo;
    }

    internal T GetProperty<T>(string propertyName)
    {
        var propInfo = GetPropertyInfo(propertyName);

        if (propInfo.PropertyType != typeof(T))
        {
            throw new ArgumentException($"Incorrect type provided for property {propertyName}. Actual type: {propInfo.PropertyType}");
        }

        var value = propInfo.GetValue(this);

        return (T) value!;
    }

    internal void SetProperty<T>(string propertyName, T value)
    {
        var propInfo = GetPropertyInfo(propertyName);

        if (propInfo.PropertyType != typeof(T))
        {
            throw new ArgumentException($"Incorrect type provided for property {propertyName}. Actual type: {propInfo.PropertyType}");
        }

        propInfo.SetValue(this, value);
    }

    private bool _minSizeSet;
    private bool _maxSizeSet;

    #region Backing Fields

    private bool _enabled = true;
    private ResizeMode _resizeMode = ResizeMode.Stretch;
    private ZIndexUpdateMode _zIndexUpdateMode = ZIndexUpdateMode.OneHigherThanParent;
    private int _zIndex;
    private Component? _parent;
    private Vector _minSize;
    private Vector _maxSize;
    private Vector _size;
    private Vector _position;
    private Vector _globalPosition;
    private State _state = State.Default;

    #endregion

    public bool ContainsPoint(Vector pos)
    {
        return pos.X >= GlobalPosition.X && pos.X < GlobalPosition.X + Width &&
               pos.Y >= GlobalPosition.Y && pos.Y < GlobalPosition.Y + Height;
    }

    protected virtual void OnPropertyChanged(string propertyName, object? oldValue, object? newValue)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName, oldValue, newValue));
    }

    protected void SetField<T>(ref T field, T value,
        Func<T, T>? onChanging = null,
        Action<T, T>? onChanged = null,
        [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;

        if (onChanging is not null) value = onChanging(value);

        var oldValue = field;
        field = value;

        onChanged?.Invoke(oldValue, value);

        OnPropertyChanged(propertyName!, oldValue, value);
    }

    private void ValidateSize() => Size = ValidateSize(Size);

    private Vector ValidateSize(Vector size)
    {
        if (_minSizeSet)
        {
            if (size.X < MinSize.X) size.X = MinSize.X;
            if (size.Y < MinSize.Y) size.Y = MinSize.Y;
        }

        if (_maxSizeSet)
        {
            if (size.X > MaxSize.X) size.X = MaxSize.X;
            if (size.Y > MaxSize.Y) size.Y = MaxSize.Y;
        }

        return size;
    }

    private void OnMinSizeChanged(Vector oldValue, Vector newValue)
    {
        _minSizeSet = true;
        ValidateSize();
    }

    private void OnMaxSizeChanged(Vector oldValue, Vector newValue)
    {
        _maxSizeSet = true;
        ValidateSize();
    }

    private void OnPositionChanged(Vector oldValue, Vector newValue)
    {
        if (Parent is null) GlobalPosition = newValue;
        else GlobalPosition = Position + Parent.GlobalPosition;
    }

    private void OnGlobalPositionChanged(Vector oldValue, Vector newValue)
    {
        if (Parent is null) Position = newValue;
        else Position = newValue - Parent.GlobalPosition;
    }
}


