using OpenForge.Cli.Core.Framework.Sources.Models.References;

namespace OpenForge.Cli.Core.Framework.Sources.Operational.Models.References;

internal enum LocalReferenceGraphFindingKind
{
    Cycle,
    Repeat,
}

internal sealed class LocalReferenceGraphFinding
{
    private LocalReferenceGraphFinding(LocalReferenceGraphFindingKind kind, IReadOnlyList<LocalReferenceObservation> occurrences)
    {
        Kind = kind;
        Occurrences = occurrences;
    }

    internal LocalReferenceGraphFindingKind Kind { get; }

    internal IReadOnlyList<LocalReferenceObservation> Occurrences { get; }

    internal static LocalReferenceGraphFinding Repeat(IReadOnlyList<LocalReferenceObservation> occurrences)
    {
        if (occurrences.Count < 2
            || occurrences.Any(item => item is null)
            || occurrences.Select(item => (item.SourcePath, item.Destination)).Distinct().Count() != 1)
        {
            throw new ArgumentException("A repeated local reference requires at least two identical authored occurrences.", nameof(occurrences));
        }

        return new(LocalReferenceGraphFindingKind.Repeat, occurrences.ToArray());
    }

    internal static LocalReferenceGraphFinding Cycle(IReadOnlyList<LocalReferenceObservation> occurrences)
    {
        if (occurrences.Count == 0 || occurrences.Any(item => item is null))
        {
            throw new ArgumentException("A local-reference cycle requires non-null occurrences.", nameof(occurrences));
        }

        for (var index = 0; index < occurrences.Count; index++)
        {
            var occurrence = occurrences[index];
            var next = occurrences[(index + 1) % occurrences.Count];
            if (occurrence.Facts.Target is not
                { Kind: SourceLinkTargetKind.Local, Resolution: SourceLinkTargetResolution.Complete }
                || !string.Equals(occurrence.Facts.Target.Path, next.RoutePath, StringComparison.Ordinal))
            {
                throw new ArgumentException("Local-reference cycle occurrences must form one exact complete local edge chain.", nameof(occurrences));
            }
        }

        return new(LocalReferenceGraphFindingKind.Cycle, occurrences.ToArray());
    }
}
