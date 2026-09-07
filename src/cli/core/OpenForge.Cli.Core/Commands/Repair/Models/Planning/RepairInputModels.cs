using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Commands.Repair.Models.Selection;

namespace OpenForge.Cli.Core.Commands.Repair.Models.Planning;

internal sealed record RepairOccurrenceState
{
    internal RepairOccurrenceState(
        string sourceCanonicalPath,
        SourceLocation occurrence,
        FileStateSnapshot observedFileState)
    {
        SourceCanonicalPath = SourceWorkspaceRelativePath.ValidateMarkdown(
            sourceCanonicalPath,
            nameof(sourceCanonicalPath));
        ArgumentNullException.ThrowIfNull(occurrence);
        if (occurrence.ByteLength < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(occurrence),
                occurrence.ByteLength,
                "A Repair occurrence must identify a non-empty destination span.");
        }

        ArgumentNullException.ThrowIfNull(observedFileState);
        Occurrence = occurrence;
        ObservedFileState = observedFileState;
    }

    internal string SourceCanonicalPath { get; }

    internal SourceLocation Occurrence { get; }

    internal FileStateSnapshot ObservedFileState { get; }
}

internal sealed record RepairDestinationTransition
{
    internal RepairDestinationTransition(
        string expectedDestination,
        string observedDestination,
        string intendedDestination)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(expectedDestination);
        ArgumentException.ThrowIfNullOrWhiteSpace(observedDestination);
        ArgumentException.ThrowIfNullOrWhiteSpace(intendedDestination);
        if (string.Equals(expectedDestination, intendedDestination, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "A Repair destination transition must describe a changed destination.",
                nameof(intendedDestination));
        }

        ExpectedDestination = expectedDestination;
        ObservedDestination = observedDestination;
        IntendedDestination = intendedDestination;
    }

    internal string ExpectedDestination { get; }

    internal string ObservedDestination { get; }

    internal string IntendedDestination { get; }
}

internal sealed record RepairableReferenceInput
{
    internal RepairableReferenceInput(
        RepairOccurrenceState occurrenceState,
        RepairDestinationTransition destinationTransition,
        RepairCatalogueMember catalogueMember,
        RepairSelectionOrigin selectionOrigin,
        RepairTargetSelection target)
    {
        ArgumentNullException.ThrowIfNull(occurrenceState);
        ArgumentNullException.ThrowIfNull(destinationTransition);
        if (!Enum.IsDefined(catalogueMember))
        {
            throw new ArgumentOutOfRangeException(
                nameof(catalogueMember),
                catalogueMember,
                "The Repair catalogue member is not defined.");
        }

        if (!Enum.IsDefined(selectionOrigin))
        {
            throw new ArgumentOutOfRangeException(
                nameof(selectionOrigin),
                selectionOrigin,
                "The Repair selection origin is not defined.");
        }

        ArgumentNullException.ThrowIfNull(target);
        if (catalogueMember == RepairCatalogueMember.MissingTargetRelink
            && target.CandidateProvenance.Count == 0)
        {
            throw new ArgumentException(
                "A missing-target Repair input requires selected candidate provenance.",
                nameof(target));
        }

        if (catalogueMember != RepairCatalogueMember.MissingTargetRelink
            && target.CandidateProvenance.Count != 0)
        {
            throw new ArgumentException(
                "A safe-exact Repair input cannot carry guided candidate provenance.",
                nameof(target));
        }

        if (catalogueMember == RepairCatalogueMember.MissingTargetRelink
            && selectionOrigin == RepairSelectionOrigin.Automatic)
        {
            throw new ArgumentException(
                "Automatic Repair selection cannot select a missing-target candidate.",
                nameof(selectionOrigin));
        }

        OccurrenceState = occurrenceState;
        DestinationTransition = destinationTransition;
        CatalogueMember = catalogueMember;
        SelectionOrigin = selectionOrigin;
        Target = target;
    }

    internal RepairOccurrenceState OccurrenceState { get; }

    internal RepairDestinationTransition DestinationTransition { get; }

    internal string SourceCanonicalPath => OccurrenceState.SourceCanonicalPath;

    internal SourceLocation Occurrence => OccurrenceState.Occurrence;

    internal string ExpectedDestination => DestinationTransition.ExpectedDestination;

    internal string ObservedDestination => DestinationTransition.ObservedDestination;

    internal string IntendedDestination => DestinationTransition.IntendedDestination;

    internal RepairCatalogueMember CatalogueMember { get; }

    internal RepairSelectionOrigin SelectionOrigin { get; }

    internal RepairTargetSelection Target { get; }

    internal FileStateSnapshot ObservedFileState => OccurrenceState.ObservedFileState;
}
