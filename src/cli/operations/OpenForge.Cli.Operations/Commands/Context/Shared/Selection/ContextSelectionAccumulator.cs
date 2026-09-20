using OpenForge.Cli.Core.Commands.Context.Models.Operation;
using OpenForge.Cli.Core.Commands.Context.Models.Result;

namespace OpenForge.Cli.Core.Commands.Context.Shared.Selection;

internal sealed class ContextSelectionAccumulator
{
    private readonly List<MutableSelection> _values = [];
    private readonly Dictionary<string, MutableSelection> _byPath = new(StringComparer.Ordinal);

    internal IReadOnlyList<ContextSelectedGraphSource> Sources
        => _values.Select(value => value.Freeze()).ToArray();

    internal bool Add(ContextGraphSource source, ContextInclusionReason reason)
    {
        if (_byPath.TryGetValue(source.CanonicalPath, out var existing))
        {
            if (!existing.Reasons.Contains(reason))
            {
                existing.Reasons.Add(reason);
            }

            return false;
        }

        var added = new MutableSelection
        {
            Source = source,
            Reasons = [reason],
        };
        _byPath.Add(source.CanonicalPath, added);
        _values.Add(added);
        return true;
    }

    internal ContextSelectionAccumulator Clone()
    {
        var clone = new ContextSelectionAccumulator();
        foreach (var value in _values)
        {
            foreach (var reason in value.Reasons)
            {
                clone.Add(value.Source, reason);
            }
        }

        return clone;
    }

    private sealed record MutableSelection
    {
        public required ContextGraphSource Source { get; init; }

        public required List<ContextInclusionReason> Reasons { get; init; }

        internal ContextSelectedGraphSource Freeze() => new()
        {
            Source = Source,
            InclusionReasons = Reasons.ToArray(),
        };
    }
}
