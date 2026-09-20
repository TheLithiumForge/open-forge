using OpenForge.Cli.Core.Commands.References.Models.Request;
using OpenForge.Cli.Core.Commands.References.Models.Result;
using OpenForge.Cli.Core.Commands.References.Models.Source;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Framework.Sources.Models.Selection;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.References.Shared.Result;

internal static class ReferencesFindingFactory
{
    internal static void AddEvent(
        ICollection<ReferencesFinding> findings,
        ReferencesFindingCode code,
        ReferencesDirection? direction)
        => AddFinding(
            findings,
            new ReferencesFindingInput(
                code,
                code == ReferencesFindingCode.Interrupted
                ? "The References operation was interrupted before all requested evidence was established."
                : "The References operation failed before normal completion.")
            {
                Direction = direction,
            });

    internal static void AddFinding(
        ICollection<ReferencesFinding> findings,
        ReferencesFindingInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        var finding = new ReferencesFinding(
            input.Code,
            input.Direction,
            input.Subject,
            input.Cause,
            input.SelectorRole,
            input.SelectorOccurrence,
            input.Source,
            input.Layer,
            input.Path,
            input.Location,
            input.DestinationLocation,
            input.Candidates ?? [],
            input.StatusOverride);

        // One observation is recorded once. The same file is read by the outgoing scan and again
        // by the incoming scan, and the extractor and the resolver can both report one unusable
        // destination, so the same sentence would otherwise reach the reader two or three times.
        // Direction is deliberately not part of the identity: which pass noticed a file that
        // cannot be decoded is not a second fact about that file, and the reader is never shown
        // the direction. The first record wins and later repeats are dropped here, once, rather
        // than in each renderer.
        if (findings.Any(existing => DescribesSameFact(existing, finding)))
        {
            return;
        }

        findings.Add(finding);
    }

    private static bool DescribesSameFact(ReferencesFinding left, ReferencesFinding right)
        => left.Code == right.Code
            && string.Equals(left.Subject, right.Subject, StringComparison.Ordinal)
            && string.Equals(left.Cause, right.Cause, StringComparison.Ordinal)
            && string.Equals(left.Path, right.Path, StringComparison.Ordinal)
            && SamePlace(left.Location, right.Location)
            && SamePlace(left.DestinationLocation, right.DestinationLocation);

    private static bool SamePlace(SourceLocation? left, SourceLocation? right)
    {
        if (left is null || right is null)
        {
            return left is null && right is null;
        }

        return left.Line == right.Line && left.Column == right.Column;
    }
}
