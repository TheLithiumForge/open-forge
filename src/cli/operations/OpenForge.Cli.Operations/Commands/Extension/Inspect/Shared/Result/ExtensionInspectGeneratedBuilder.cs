using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Result;

internal static class ExtensionInspectGeneratedBuilder
{
    private const string GeneratedOwnership = "derived-navigation-only";

    internal static ExtensionInspectGenerated Build(
        IReadOnlyDictionary<string, MarkdownFingerprintFacts> current,
        IReadOnlyDictionary<string, MarkdownFingerprintFacts> intended,
        ExtensionInspectPathState pathState,
        ICollection<ExtensionInspectFinding> findings)
    {
        var regions = new List<ExtensionInspectGeneratedRegion>();
        var paths = current.Keys.Concat(intended.Keys).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal);
        foreach (var path in paths)
        {
            current.TryGetValue(path, out var currentFacts);
            intended.TryGetValue(path, out var intendedFacts);
            var facts = ReadGeneratedFacts(currentFacts, intendedFacts);
            var region = facts.Region;
            var state = region.State switch
            {
                MarkdownFingerprintRegionState.Valid => ExtensionInspectGeneratedRegionState.Valid,
                MarkdownFingerprintRegionState.Absent => ExtensionInspectGeneratedRegionState.Absent,
                MarkdownFingerprintRegionState.Invalid => ExtensionInspectGeneratedRegionState.Invalid,
                MarkdownFingerprintRegionState.Ambiguous => ExtensionInspectGeneratedRegionState.Ambiguous,
                MarkdownFingerprintRegionState.Unavailable => ExtensionInspectGeneratedRegionState.Unavailable,
                _ => throw new ArgumentOutOfRangeException(nameof(region), region.State, "The Markdown fingerprint region state is not defined."),
            };
            regions.Add(new ExtensionInspectGeneratedRegion
            {
                Path = path,
                State = state,
                StartByteOffset = region.StartByteOffset,
                EndByteOffset = region.EndByteOffset,
                ExcludedInteriorByteLength = region.ExcludedInteriorByteLength,
            });
            if (state is ExtensionInspectGeneratedRegionState.Invalid or ExtensionInspectGeneratedRegionState.Ambiguous)
            {
                ExtensionInspectFindingPolicy.Add(findings, new ExtensionInspectFindingInput
                {
                    Code = ExtensionInspectFindingCode.GeneratedBoundaryInvalid,
                    Path = path,
                    Cause = "The generated Markdown region is not a valid final boundary.",
                });
            }
        }

        var incomplete = regions.Any(region => region.State is ExtensionInspectGeneratedRegionState.Invalid
            or ExtensionInspectGeneratedRegionState.Ambiguous
            or ExtensionInspectGeneratedRegionState.Unavailable);
        return new ExtensionInspectGenerated
        {
            State = incomplete || pathState is ExtensionInspectPathState.Incomplete or ExtensionInspectPathState.Blocked
                ? ExtensionInspectGeneratedState.Incomplete
                : ExtensionInspectGeneratedState.Complete,
            Ownership = GeneratedOwnership,
            Regions = regions,
        };
    }

    private static MarkdownFingerprintFacts ReadGeneratedFacts(
        MarkdownFingerprintFacts? current,
        MarkdownFingerprintFacts? intended)
    {
        if (current is null)
        {
            if (intended is { } available)
            {
                return available;
            }

            throw new InvalidOperationException("A generated path requires current or intended Markdown facts.");
        }

        if (intended is null)
        {
            return current;
        }

        return ReadRegionPriority(intended.Region.State) > ReadRegionPriority(current.Region.State)
            ? intended
            : current;
    }

    private static int ReadRegionPriority(MarkdownFingerprintRegionState state)
        => state switch
        {
            MarkdownFingerprintRegionState.Absent => 0,
            MarkdownFingerprintRegionState.Valid => 1,
            MarkdownFingerprintRegionState.Unavailable => 2,
            MarkdownFingerprintRegionState.Ambiguous => 3,
            MarkdownFingerprintRegionState.Invalid => 4,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The Markdown fingerprint region state is not defined."),
        };
}
