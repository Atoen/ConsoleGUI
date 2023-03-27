namespace ConsoleGUI.Attributes.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class MethodCallAttribute : Attribute
{
    public MethodCallAttribute(MethodCallMode callMode)
    {
        CallMode = callMode;
    }

    public MethodCallMode CallMode { get; }
}

public enum MethodCallMode
{
    Scheduled,
    OnEvent,
    Manual
}