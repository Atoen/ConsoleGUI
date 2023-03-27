using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace ConsoleGUI.Attributes.Attributes;

public class MinSizeAttribute : ValidateInputAttribute<Vector>
{
    public MinSizeAttribute(int minWidth, int minHeight) : base(vector => vector.X >= minWidth && vector.Y >= minHeight)
    {
        MinWidth = minWidth;
        MinHeight = minHeight;
    }

    public int MinWidth { get; }
    public int MinHeight { get; }

    [DoesNotReturn]
    protected override void Throw(MethodBase method, Vector input)
    {
        var message = input switch
        {
            var (x, y) when x < MinWidth && y >= MinHeight =>
                $"Method {method} argument width must be greater than or equal to {MinWidth}. Actual value: {input}",
            var (x, y) when y < MinHeight && x >= MinWidth =>
                $"Method {method} argument height must be greater than or equal to {MinHeight}. Actual value: {input}",
            _ =>
                $"Method {method} argument size must be greater than or equal to {new Vector(MinWidth, MinHeight)}. Actual value: {input}"
        };

        throw new ArgumentException(message);
    }
}

