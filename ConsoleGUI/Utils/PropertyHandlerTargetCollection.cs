using System.Collections;

namespace ConsoleGUI.Utils;

public class PropertyHandlerTargetCollection : IEnumerable<string>
{
    public List<string> Targets { get; } = new();

    public static implicit operator PropertyHandlerTargetCollection(string target) => new() {Targets = {target}};

    public static implicit operator PropertyHandlerTargetCollection((string, string) targets) =>
        new() {Targets = {targets.Item1, targets.Item2}};

    public static implicit operator PropertyHandlerTargetCollection((string, string, string) targets) =>
        new() {Targets = {targets.Item1, targets.Item2, targets.Item3}};

    public static implicit operator PropertyHandlerTargetCollection((string, string, string, string) targets) =>
        new() {Targets = {targets.Item1, targets.Item2, targets.Item3, targets.Item4}};

    public static implicit operator PropertyHandlerTargetCollection((string, string, string, string, string) targets) =>
        new() {Targets = {targets.Item1, targets.Item2, targets.Item3, targets.Item4, targets.Item5}};

    public static implicit operator PropertyHandlerTargetCollection((string, string, string, string, string, string) targets) =>
        new() {Targets = {targets.Item1, targets.Item2, targets.Item3, targets.Item4, targets.Item5, targets.Item6}};

    public static implicit operator PropertyHandlerTargetCollection((string, string, string, string, string, string, string) targets) =>
        new() {Targets = {targets.Item1, targets.Item2, targets.Item3, targets.Item4, targets.Item5, targets.Item6, targets.Item7}};

    public static implicit operator PropertyHandlerTargetCollection((string, string, string, string, string, string, string, string) targets) =>
        new() {Targets = {targets.Item1, targets.Item2, targets.Item3, targets.Item4, targets.Item5, targets.Item6, targets.Item7, targets.Item8}};

    public IEnumerator<string> GetEnumerator() => Targets.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}