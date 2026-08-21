using System.Collections.ObjectModel;

namespace OpenForge.Cli.Core.Shell.Definitions;

internal enum CliOptionArity
{
    None,
    ExactlyOne,
}

internal sealed record CliSyntaxDefinition
{
    internal CliSyntaxDefinition(string name, string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        Name = name;
        Description = description;
    }

    internal string Name { get; }

    internal string Description { get; }
}

internal sealed class CliOptionDefinition<T>
{
    internal CliOptionDefinition(
        string name,
        string description,
        CliOptionArity arity,
        T defaultValue,
        string? valueName = null,
        IReadOnlyDictionary<string, T>? finiteSpellings = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        if (!Enum.IsDefined(arity))
        {
            throw new ArgumentOutOfRangeException(nameof(arity), arity, "The option arity is not defined.");
        }

        if (arity == CliOptionArity.ExactlyOne)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(valueName);
        }

        Name = name;
        Description = description;
        Arity = arity;
        DefaultValue = defaultValue;
        ValueName = valueName;
        FiniteSpellings = new ReadOnlyDictionary<string, T>(
            finiteSpellings is null
                ? new Dictionary<string, T>(StringComparer.Ordinal)
                : new Dictionary<string, T>(finiteSpellings, StringComparer.Ordinal));
    }

    internal string Name { get; }

    internal string Description { get; }

    internal CliOptionArity Arity { get; }

    internal T DefaultValue { get; }

    internal string? ValueName { get; }

    internal IReadOnlyDictionary<string, T> FiniteSpellings { get; }

    internal bool TryReadFinite(string spelling, out T value)
    {
        ArgumentNullException.ThrowIfNull(spelling);
        return FiniteSpellings.TryGetValue(spelling, out value!);
    }
}

internal static class CliSyntaxDefinitions
{
    internal const string ExecutableName = "open-forge";

    internal static readonly CliSyntaxDefinition Root = new(
        ExecutableName,
        "Inspect and maintain an Open Forge workspace.");

    internal static readonly CliOptionDefinition<string?> Workspace = new(
        "--workspace",
        "Select the workspace explicitly.",
        CliOptionArity.ExactlyOne,
        null,
        "path");

    internal static readonly CliOptionDefinition<bool> Json = new(
        "--json",
        "Write the complete structured result as JSON.",
        CliOptionArity.None,
        false);

    internal static readonly CliOptionDefinition<CliView> View = new(
        "--view",
        "Select compact or expanded human output.",
        CliOptionArity.ExactlyOne,
        CliView.Expanded,
        "view",
        new Dictionary<string, CliView>(StringComparer.Ordinal)
        {
            [CliPresentationDefinitions.Compact] = CliView.Compact,
            [CliPresentationDefinitions.Expanded] = CliView.Expanded,
        });

    internal static readonly CliOptionDefinition<bool> Verbose = new(
        "--verbose",
        "Write bounded diagnostics to stderr.",
        CliOptionArity.None,
        false);

    internal static readonly CliOptionDefinition<bool> Help = new(
        "--help",
        "Show help and exit.",
        CliOptionArity.None,
        false);

    internal static readonly CliOptionDefinition<bool> Version = new(
        "--version",
        "Show the executable version and exit.",
        CliOptionArity.None,
        false);
}
