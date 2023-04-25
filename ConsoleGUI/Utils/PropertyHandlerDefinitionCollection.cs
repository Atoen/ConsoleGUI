using ConsoleGUI.UI.New;

namespace ConsoleGUI.Utils;

public class PropertyHandlerDefinitionCollection<TComponent> : Dictionary<PropertyHandlerTargetCollection, PropertyHandler<TComponent>> where TComponent : Component
{
}
