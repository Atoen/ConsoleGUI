using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using MethodBoundaryAspect.Fody.Attributes;

namespace ConsoleGUI.Attributes.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
public abstract class ValidateInputAttribute<T> : OnMethodBoundaryAspect
{
    protected ValidateInputAttribute(Predicate<T> predicate)
    {
        Predicate = predicate;

        AttributeTargetMemberAttributes = MulticastAttributes.Default;
    }

    public Predicate<T> Predicate { get; }

    public override void OnEntry(MethodExecutionArgs arg)
    {
        if (arg.Arguments is not [T input]) return;

        if (!Predicate(input)) Throw(arg.Method, input);
    }

    [DoesNotReturn]
    protected virtual void Throw(MethodBase method, T input)
    {
        throw new ArgumentException($"Method {method} argument is invalid. Actual value: {input}");
    }
}
