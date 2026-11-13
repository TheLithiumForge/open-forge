using OpenForge.Cli.Core.Framework.Tools.Models.Registration;
using OpenForge.Cli.Core.Framework.Tools.Shared.Validation;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Registration;

internal static class ToolOptionalComponents
{
    internal const string Yes = "yes";
    internal const string No = "no";

    internal static bool ParsePromptHook(string value) => value switch
    {
        Yes => true,
        No => false,
        _ => throw new ArgumentException("The prompt-hook choice requires yes or no.", nameof(value)),
    };

    // Missing components keep their description and default for the Operations prompt or required-flag finding.
    internal static (Dictionary<string, bool> Choices, ToolOptionalComponent[] Missing) Resolve(
        ToolOptionalComponent[] components, IReadOnlyDictionary<string, bool> supplied, IReadOnlyDictionary<string, bool> saved)
    {
        var choices = new Dictionary<string, bool>(StringComparer.Ordinal);
        var missing = new List<ToolOptionalComponent>();
        var ids = components.Select(component => component.Id).ToHashSet(StringComparer.Ordinal);
        if (ids.Count != components.Length || supplied.Keys.Any(id => !ids.Contains(id)))
            throw new ArgumentException("The component selection is unknown or duplicated.", nameof(supplied));
        foreach (var component in components.OrderBy(component => component.Id, StringComparer.Ordinal))
        {
            if (!ToolIdentity.IsId(component.Id) || string.IsNullOrWhiteSpace(component.Description))
                throw new InvalidDataException("An optional component requires an exact ID and an explanation.");
            if (component.Flag != ToolComponentFlags.PromptHook) throw new InvalidDataException("A definition cannot register an arbitrary component flag.");
            if (supplied.TryGetValue(component.Id, out var selected)) choices.Add(component.Id, selected);
            else if (saved.TryGetValue(component.Id, out var recorded)) choices.Add(component.Id, recorded);
            else missing.Add(component);
        }
        return (choices, [.. missing]);
    }
}
