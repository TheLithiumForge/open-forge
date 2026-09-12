using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Framework.Sources.Models.Selection;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.References.Models.Request;

internal enum ReferencesDirection
{
    In,
    Out,
    Both,
}

internal sealed record ReferencesRequest
{
    internal ReferencesRequest(
        CliWorkspace workspace,
        string sourceReference,
        ReferencesDirection direction = ReferencesDirection.Both,
        IEnumerable<SourceUniverseSelectorOccurrence>? selectorOccurrences = null)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentNullException.ThrowIfNull(sourceReference);
        ValidateDirection(direction);
        var occurrences = Snapshot(selectorOccurrences ?? []);
        if (direction == ReferencesDirection.Out && occurrences.Count != 0)
        {
            throw new ArgumentException("References filters are valid only when incoming work is requested.", nameof(selectorOccurrences));
        }

        Workspace = workspace;
        SourceReference = sourceReference;
        Direction = direction;
        SelectorOccurrences = occurrences;
    }

    internal ReferencesRequest(
        CliWorkspace workspace,
        string sourceReference,
        IEnumerable<SourceUniverseSelectorOccurrence> selectorOccurrences,
        ReferencesDirection direction = ReferencesDirection.Both)
        : this(workspace, sourceReference, direction, selectorOccurrences)
    {
    }

    internal CliWorkspace Workspace { get; }

    internal string SourceReference { get; }

    internal string SuppliedSourceReference => SourceReference;

    internal ReferencesDirection Direction { get; }

    internal IReadOnlyList<SourceUniverseSelectorOccurrence> SelectorOccurrences { get; }

    internal IReadOnlyList<SourceUniverseSelectorOccurrence> SourceUniverseSelectorOccurrences
        => SelectorOccurrences;

    internal bool RequestsIncoming => Direction is ReferencesDirection.In or ReferencesDirection.Both;

    internal bool RequestsOutgoing => Direction is ReferencesDirection.Out or ReferencesDirection.Both;

    private static IReadOnlyList<SourceUniverseSelectorOccurrence> Snapshot(
        IEnumerable<SourceUniverseSelectorOccurrence> occurrences)
    {
        ArgumentNullException.ThrowIfNull(occurrences);
        var values = occurrences
            .Select(value => value ?? throw new ArgumentException(
                "References selector occurrences cannot contain null members.", nameof(occurrences)))
            .ToArray();
        for (var index = 0; index < values.Length; index++)
        {
            if (values[index].Position != index + 1)
            {
                throw new ArgumentException(
                    "References selector positions must preserve contiguous global command-line order.",
                    nameof(occurrences));
            }
        }

        return new ReadOnlyCollection<SourceUniverseSelectorOccurrence>(values);
    }

    private static void ValidateDirection(ReferencesDirection direction)
    {
        if (!Enum.IsDefined(direction))
        {
            throw new ArgumentOutOfRangeException(nameof(direction), direction, "The References direction is not defined.");
        }
    }
}

internal sealed record ReferencesRequestEcho
{
    internal ReferencesRequestEcho(
        CliWorkspace workspace,
        string sourceReference,
        ReferencesDirection? requestedDirection,
        IEnumerable<SourceUniverseSelectorOccurrence>? selectorOccurrences = null)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentNullException.ThrowIfNull(sourceReference);
        if (requestedDirection is { } direction && !Enum.IsDefined(direction))
        {
            throw new ArgumentOutOfRangeException(nameof(requestedDirection), requestedDirection, "The References direction is not defined.");
        }

        Workspace = workspace;
        SourceReference = sourceReference;
        RequestedDirection = requestedDirection;
        SelectorOccurrences = new ReadOnlyCollection<SourceUniverseSelectorOccurrence>(
            (selectorOccurrences ?? [])
            .Select(value => value ?? throw new ArgumentException(
                "References selector occurrences cannot contain null members.", nameof(selectorOccurrences)))
            .ToArray());
    }

    internal ReferencesRequestEcho(ReferencesRequest request)
        : this(
            (request ?? throw new ArgumentNullException(nameof(request))).Workspace,
            request.SourceReference,
            request.Direction,
            request.SelectorOccurrences)
    {
    }

    internal CliWorkspace Workspace { get; }

    internal string SourceReference { get; }

    internal ReferencesDirection? RequestedDirection { get; }

    internal IReadOnlyList<SourceUniverseSelectorOccurrence> SelectorOccurrences { get; }
}
