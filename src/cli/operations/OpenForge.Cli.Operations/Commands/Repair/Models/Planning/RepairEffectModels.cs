using System.Collections.ObjectModel;
using System.Text;
using OpenForge.Cli.Core.Commands.Repair;
using OpenForge.Cli.Core.Commands.Shared.Models;
using OpenForge.Cli.Core.Commands.Repair.Models.Selection;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Commands.Repair.Shared.Request;

namespace OpenForge.Cli.Core.Commands.Repair.Models.Planning;

internal sealed record RepairChange
{
    internal RepairChange(
        SourceLocation occurrence,
        string expectedDestination,
        string intendedDestination,
        RepairCatalogueMember catalogueMember,
        RepairTargetSelection target,
        IEnumerable<RepairSelectionOrigin> origins)
    {
        ArgumentNullException.ThrowIfNull(occurrence);
        if (occurrence.ByteLength < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(occurrence),
                occurrence.ByteLength,
                "A Repair change must identify a non-empty destination span.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(expectedDestination);
        ArgumentException.ThrowIfNullOrWhiteSpace(intendedDestination);
        if (string.Equals(expectedDestination, intendedDestination, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "A Repair change must alter the authored destination.",
                nameof(intendedDestination));
        }

        if (!Enum.IsDefined(catalogueMember))
        {
            throw new ArgumentOutOfRangeException(
                nameof(catalogueMember),
                catalogueMember,
                "The Repair catalogue member is not defined.");
        }

        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(origins);
        var originValues = origins
            .Select(origin =>
            {
                if (!Enum.IsDefined(origin))
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(origins),
                        origin,
                        "The Repair selection origin is not defined.");
                }

                return origin;
            })
            .Distinct()
            .ToArray();
        if (originValues.Length == 0)
        {
            throw new ArgumentException(
                "A Repair change requires at least one selection origin.",
                nameof(origins));
        }

        Occurrence = occurrence;
        ExpectedDestination = expectedDestination;
        IntendedDestination = intendedDestination;
        CatalogueMember = catalogueMember;
        Target = target;
        Origins = new ReadOnlyCollection<RepairSelectionOrigin>(originValues);
    }

    internal SourceLocation Occurrence { get; }

    internal CommandSourceLocation OccurrenceView => CommandSourceLocation.From(Occurrence);

    internal string ExpectedDestination { get; }

    internal string IntendedDestination { get; }

    internal RepairCatalogueMember CatalogueMember { get; }

    internal RepairTargetSelection Target { get; }

    internal IReadOnlyList<RepairSelectionOrigin> Origins { get; }
}

internal sealed record RepairEffect
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    internal RepairEffect(
        string sourceCanonicalPath,
        FileStateSnapshot expectedState,
        FileStateSnapshot intendedState,
        IEnumerable<RepairChange> changes,
        RecoveryBundleAttribution recoveryAttribution)
    {
        SourceCanonicalPath = RepairPathValidation.ValidateMarkdown(
            sourceCanonicalPath,
            nameof(sourceCanonicalPath));
        ArgumentNullException.ThrowIfNull(expectedState);
        ArgumentNullException.ThrowIfNull(intendedState);
        if (!expectedState.HasBytes || !intendedState.HasBytes
            || expectedState.Kind != FileExpectationKind.File
            || intendedState.Kind != FileExpectationKind.File)
        {
            throw new ArgumentException(
                "A Repair effect requires complete file snapshots.",
                nameof(expectedState));
        }

        if (expectedState.Bytes.AsSpan().SequenceEqual(intendedState.Bytes.AsSpan()))
        {
            throw new ArgumentException(
                "A Repair effect must change the complete file state.",
                nameof(intendedState));
        }

        ArgumentNullException.ThrowIfNull(changes);
        RepairDefinitions.ValidateRepairRecoveryAttribution(
            recoveryAttribution,
            nameof(recoveryAttribution));
        var values = changes
            .Select(change => change ?? throw new ArgumentException(
                "Repair effect changes cannot contain null members.",
                nameof(changes)))
            .OrderBy(change => change.Occurrence.ByteOffset)
            .ThenBy(change => change.Occurrence.ByteLength)
            .ToArray();
        ValidateAndProject(expectedState, intendedState, values);

        ExpectedState = expectedState;
        IntendedState = intendedState;
        Changes = new ReadOnlyCollection<RepairChange>(values);
        RecoveryAttribution = recoveryAttribution;
        FileChange = PlannedFileChange.Replace(
            expectedState.Expectation,
            intendedState.Bytes.AsSpan());
    }

    internal string SourceCanonicalPath { get; }

    internal FileStateSnapshot ExpectedState { get; }

    internal FileStateSnapshot IntendedState { get; }

    internal string? BeforeHash => ExpectedState.ContentHash;

    internal string? AfterHash => IntendedState.ContentHash;

    internal IReadOnlyList<RepairChange> Changes { get; }

    internal RecoveryBundleAttribution RecoveryAttribution { get; }

    internal PlannedFileChange FileChange { get; }

    private static void ValidateAndProject(
        FileStateSnapshot expectedState,
        FileStateSnapshot intendedState,
        RepairChange[] changes)
    {
        if (changes.Length == 0)
        {
            throw new ArgumentException(
                "A Repair effect requires at least one bounded change.",
                nameof(changes));
        }

        var projectedLength = expectedState.Bytes.Length;
        long previousEnd = -1;
        foreach (var change in changes)
        {
            var start = change.Occurrence.ByteOffset;
            var end = checked(start + change.Occurrence.ByteLength);
            if (start < 0 || end > expectedState.Bytes.Length || start < previousEnd)
            {
                throw new ArgumentException(
                    "Repair effect changes must be contained and non-overlapping in the expected file.",
                    nameof(changes));
            }

            var expectedBytes = StrictUtf8.GetBytes(change.ExpectedDestination);
            if (change.Occurrence.ByteLength != expectedBytes.Length
                || !expectedState.Bytes
                    .AsSpan()
                    .Slice(checked((int)start), expectedBytes.Length)
                    .SequenceEqual(expectedBytes))
            {
                throw new ArgumentException(
                    "A Repair change span must match its expected destination bytes.",
                    nameof(changes));
            }

            projectedLength = checked(
                projectedLength
                - expectedBytes.Length
                + StrictUtf8.GetByteCount(change.IntendedDestination));
            previousEnd = end;
        }

        var projected = new List<byte>(projectedLength);
        var sourcePosition = 0;
        foreach (var change in changes)
        {
            var start = checked((int)change.Occurrence.ByteOffset);
            var end = checked(start + checked((int)change.Occurrence.ByteLength));
            projected.AddRange(expectedState.Bytes.AsSpan(sourcePosition..start).ToArray());
            projected.AddRange(StrictUtf8.GetBytes(change.IntendedDestination));
            sourcePosition = end;
        }

        projected.AddRange(expectedState.Bytes.AsSpan()[sourcePosition..].ToArray());
        if (!projected.ToArray().AsSpan().SequenceEqual(intendedState.Bytes.AsSpan()))
        {
            throw new ArgumentException(
                "A Repair intended state must equal the complete projected file bytes.",
                nameof(intendedState));
        }
    }
}

internal sealed record RepairNoOp
{
    internal RepairNoOp(
        string sourceCanonicalPath,
        SourceLocation occurrence,
        string destination,
        RepairCatalogueMember catalogueMember,
        RepairTargetSelection target,
        FileStateSnapshot currentState,
        IEnumerable<RepairSelectionOrigin> origins)
    {
        SourceCanonicalPath = RepairPathValidation.ValidateMarkdown(
            sourceCanonicalPath,
            nameof(sourceCanonicalPath));
        ArgumentNullException.ThrowIfNull(occurrence);
        if (occurrence.ByteLength < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(occurrence),
                occurrence.ByteLength,
                "A Repair no-op must identify a non-empty destination span.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(destination);
        if (!Enum.IsDefined(catalogueMember))
        {
            throw new ArgumentOutOfRangeException(
                nameof(catalogueMember),
                catalogueMember,
                "The Repair catalogue member is not defined.");
        }

        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(currentState);
        ArgumentNullException.ThrowIfNull(origins);
        var originValues = origins
            .Select(origin =>
            {
                if (!Enum.IsDefined(origin))
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(origins),
                        origin,
                        "The Repair selection origin is not defined.");
                }

                return origin;
            })
            .Distinct()
            .ToArray();
        if (originValues.Length == 0)
        {
            throw new ArgumentException(
                "A Repair no-op requires at least one selection origin.",
                nameof(origins));
        }

        Occurrence = occurrence;
        Destination = destination;
        CatalogueMember = catalogueMember;
        Target = target;
        CurrentState = currentState;
        Origins = new ReadOnlyCollection<RepairSelectionOrigin>(originValues);
    }

    internal string SourceCanonicalPath { get; }

    internal SourceLocation Occurrence { get; }

    internal string Destination { get; }

    internal RepairCatalogueMember CatalogueMember { get; }

    internal RepairTargetSelection Target { get; }

    internal FileStateSnapshot CurrentState { get; }

    internal IReadOnlyList<RepairSelectionOrigin> Origins { get; }
}
