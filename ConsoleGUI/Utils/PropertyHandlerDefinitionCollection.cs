using ConsoleGUI.UI.Widgets;

namespace ConsoleGUI.Utils;

public class PropertyHandlerDefinitionCollection<TComponent> : Dictionary<PropertyHandlerTargetCollection, PropertyHandler<TComponent>> where TComponent : Component
{
}


