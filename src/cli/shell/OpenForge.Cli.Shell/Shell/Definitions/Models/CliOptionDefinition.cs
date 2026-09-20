using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace OpenForge.Cli.Core.Shell.Definitions.Models;

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

    internal bool TryReadFinite(string spelling, [MaybeNullWhen(false)] out T value)
    {
        ArgumentNullException.ThrowIfNull(spelling);
        return FiniteSpellings.TryGetValue(spelling, out value);
    }
}
