using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace ConsoleGUI.Attributes.Attributes;

public class RangeAttribute : ValidateInputAttribute<int>
{
    public RangeAttribute(int minValue = int.MinValue, int maxValue = int.MaxValue) :
        base(num => num >= minValue && num < maxValue)
    {
        MinValue = minValue;
        MaxValue = maxValue;
    }

    public int MinValue { get; }
    public int MaxValue { get; }

    [DoesNotReturn]
    protected override void Throw(MethodBase method, int input)
    {
        var message = this switch
        {
            { MinValue: int.MinValue } => $"Method {method} argument must be less than {MaxValue}. Actual value: {input}",
            { MaxValue: int.MaxValue } => $"Method {method} argument must be greater than or equal to {MinValue}. Actual value: {input}",
            _ => $"Method {method} argument must be between {MinValue} (inclusive) and {MaxValue} (exclusive). Actual value: {input}"
        };

        throw new ArgumentException(message);
    }
}