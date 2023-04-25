using ConsoleGUI.UI.New;

namespace ConsoleGUI.Utils;

public delegate void PropertyHandler<in TComponent>(TComponent component, PropertyChangedEventArgs args);

public class PropertyChangedHandlerManager
{
    public PropertyChangedHandlerManager(Component component)
    {
        _component = component;
    }

    private readonly Component _component;
    private readonly Dictionary<string, List<PropertyHandler<Component>>> _handlers = new();

    public void Handle(string propertyName, object? oldValue, object? newValue)
    {
        if (!_handlers.TryGetValue(propertyName, out var propertyHandlers)) return;

        var args = new PropertyChangedEventArgs(propertyName, oldValue, newValue);
        
        foreach (var handler in propertyHandlers)
        {
            handler.Invoke(_component, args);
        }
    }

    public void SetPropertyHandler<TComponent>(string propertyName, PropertyHandler<TComponent> handler) where TComponent : Component
    {
        if (!_handlers.TryGetValue(propertyName, out var propertyHandlers))
        {
            var list = new List<PropertyHandler<Component>> {handler.ToBase()};
            _handlers.Add(propertyName, list);

            return;
        }

        propertyHandlers.Clear();
        propertyHandlers.Add(handler.ToBase());
    }

    public void SetPropertyHandlers<TComponent>(string propertyName, IEnumerable<PropertyHandler<TComponent>> handlers) where TComponent : Component
    {
        _handlers[propertyName] = handlers.ToBaseList();
    }

    public void SetHandlers<TComponent>(Dictionary<string, PropertyHandler<TComponent>> handlers) where TComponent : Component
    {
        foreach (var (key, value) in handlers)
        {
            SetPropertyHandler(key, value);
        }
    }
    
    public void AddHandlers<TComponent>(Dictionary<string, PropertyHandler<TComponent>> handlers) where TComponent : Component
    {
        foreach (var (key, value) in handlers)
        {
            AddPropertyHandler(key, value);
        }
    }

    public void AddPropertyHandler<TComponent>(string propertyName, PropertyHandler<TComponent> handler) where TComponent : Component
    {
        if (!_handlers.TryGetValue(propertyName, out var propertyHandlers))
        {
            var list = new List<PropertyHandler<Component>> {handler.ToBase()};
            _handlers.Add(propertyName, list);

            return;
        }

        propertyHandlers.Add(handler.ToBase());
    }

    public void AddPropertyHandlers<TComponent>(string propertyName, IEnumerable<PropertyHandler<TComponent>> handlers) where TComponent : Component
    {
        if (!_handlers.TryGetValue(propertyName, out var propertyHandlers))
        {
            _handlers.Add(propertyName, handlers.ToBaseList());
            return;
        }

        propertyHandlers.AddRange(handlers.ToBaseList());
    }

    public void ClearPropertyHandlers(string propertyName)
    {
        _handlers.Remove(propertyName);
    }
}

file static class HandlerExtensions
{
    public static PropertyHandler<Component> ToBase<TComponent>(this PropertyHandler<TComponent> handler)
        where TComponent : Component =>
        (component, args) => handler.Invoke((TComponent) component, args);

    public static List<PropertyHandler<Component>> ToBaseList<TComponent>(
        this IEnumerable<PropertyHandler<TComponent>> handlers) where TComponent : Component =>
        handlers.Select(handler => handler.ToBase()).ToList();
}