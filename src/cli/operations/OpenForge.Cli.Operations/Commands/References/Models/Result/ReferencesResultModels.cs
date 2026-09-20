using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Shared.Models;
using OpenForge.Cli.Core.Commands.References.Models.Occurrence;
using OpenForge.Cli.Core.Commands.References.Models.Request;
using OpenForge.Cli.Core.Commands.References.Models.Selection;
using OpenForge.Cli.Core.Commands.References.Models.Source;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Framework.Sources.Models.Selection;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.References.Models.Result;

internal enum ReferencesCoverage
{
    Complete,
    Incomplete,
    Blocked,
}

internal sealed record ReferencesSection
{
    internal ReferencesSection(
        ReferencesCoverage coverage,
        CliSemanticStatus status,
        IEnumerable<ReferencesOccurrence> occurrences)
    {
        if (!Enum.IsDefined(coverage))
        {
            throw new ArgumentOutOfRangeException(nameof(coverage), coverage, "The References section coverage is not defined.");
        }

        _ = CliStatusDefinitions.Read(status);
        ArgumentNullException.ThrowIfNull(occurrences);
        var values = occurrences
            .Select(value => value ?? throw new ArgumentException("References occurrences cannot contain null members.", nameof(occurrences)))
            .ToArray();
        Coverage = coverage;
        Status = status;
        OccurrenceCount = values.Length;
        Occurrences = new ReadOnlyCollection<ReferencesOccurrence>(values);
    }

    internal ReferencesCoverage Coverage { get; }

    internal CliSemanticStatus Status { get; }

    internal int OccurrenceCount { get; }

    internal IReadOnlyList<ReferencesOccurrence> Occurrences { get; }
}

internal sealed record ReferencesFinding
{
    internal ReferencesFinding(
        ReferencesFindingCode code,
        ReferencesDirection? direction,
        string? subject,
        string cause,
        SourceUniverseSelectorRole? selectorRole,
        int? selectorOccurrence,
        ReferencesSourceIdentity? source,
        SourceLayerKind? layer,
        string? path,
        SourceLocation? location,
        SourceLocation? destinationLocation,
        IEnumerable<ReferencesSourceIdentity> candidates,
        CliSemanticStatus? statusOverride = null)
    {
        var definition = ReferencesDefinitions.Read(code);
        if (statusOverride is { } overriddenStatus
            && (code != ReferencesFindingCode.GeneratedRegionUnavailable
                || overriddenStatus is not (CliSemanticStatus.Incomplete or CliSemanticStatus.Blocked)))
        {
            throw new ArgumentException("Only generated-region findings may override their conditional status.", nameof(statusOverride));
        }

        if (statusOverride is { } statusValue)
        {
            _ = CliStatusDefinitions.Read(statusValue);
        }
        if (direction is { } establishedDirection
            && establishedDirection is not (ReferencesDirection.In or ReferencesDirection.Out))
        {
            throw new ArgumentOutOfRangeException(
                nameof(direction),
                direction,
                "A References finding direction must be incoming or outgoing.");
        }

        ArgumentNullException.ThrowIfNull(cause);
        if (selectorRole is { } role && !Enum.IsDefined(role))
        {
            throw new ArgumentOutOfRangeException(nameof(selectorRole), selectorRole, "The References selector role is not defined.");
        }

        if (selectorOccurrence is < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(selectorOccurrence), selectorOccurrence, "A selector occurrence must be positive.");
        }

        if ((selectorRole is null) != (selectorOccurrence is null))
        {
            throw new ArgumentException("A References selector finding requires both role and occurrence coordinates.");
        }

        ArgumentNullException.ThrowIfNull(candidates);
        var candidateValues = candidates
            .Select(value => value ?? throw new ArgumentException("Finding candidates cannot contain null members.", nameof(candidates)))
            .Distinct()
            .OrderBy(value => value.Id, StringComparer.Ordinal)
            .ThenBy(value => value.Path, StringComparer.Ordinal)
            .ToArray();
        Code = code;
        Status = statusOverride ?? definition.Status;
        Direction = direction;
        Subject = subject;
        Cause = cause;
        SelectorRole = selectorRole;
        SelectorOccurrence = selectorOccurrence;
        Source = source;
        Layer = layer;
        Path = path;
        Location = location;
        DestinationLocation = destinationLocation;
        Candidates = new ReadOnlyCollection<ReferencesSourceIdentity>(candidateValues);
    }

    internal ReferencesFindingCode Code { get; }

    internal CliSemanticStatus Status { get; }

    internal ReferencesDirection? Direction { get; }

    internal string? Subject { get; }

    internal string Cause { get; }

    internal SourceUniverseSelectorRole? SelectorRole { get; }

    internal int? SelectorOccurrence { get; }

    internal ReferencesSourceIdentity? Source { get; }

    internal SourceLayerKind? Layer { get; }

    internal string? Path { get; }

    internal SourceLocation? Location { get; }

    internal CommandSourceLocation? LocationView
        => Location is { } location ? CommandSourceLocation.From(location) : null;

    internal SourceLocation? DestinationLocation { get; }

    internal IReadOnlyList<ReferencesSourceIdentity> Candidates { get; }
}

internal sealed record ReferencesResult : ICliCommandResult
{
    internal ReferencesResult(
        CliWorkspace? workspace,
        ReferencesSource? source,
        ReferencesDirection? requestedDirection,
        ReferencesIncomingSelection? incomingSelection,
        ReferencesSection? incoming,
        ReferencesSection? outgoing,
        IEnumerable<ReferencesFinding> findings,
        CliSemanticStatus status,
        CliNextAction? next)
    {
        if (requestedDirection is { } direction && !Enum.IsDefined(direction))
        {
            throw new ArgumentOutOfRangeException(nameof(requestedDirection), requestedDirection, "The References direction is not defined.");
        }

        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(nameof(status), status, "The References status is not defined.");
        }

        ArgumentNullException.ThrowIfNull(findings);
        var findingValues = findings
            .Select(value => value ?? throw new ArgumentException("References findings cannot contain null members.", nameof(findings)))
            .ToArray();
        if (requestedDirection is null && (incoming is not null || outgoing is not null || incomingSelection is not null))
        {
            throw new ArgumentException("An invalid direction cannot establish requested sections.");
        }

        if (requestedDirection is not null)
        {
            var requestsIncoming = requestedDirection is ReferencesDirection.In or ReferencesDirection.Both;
            var requestsOutgoing = requestedDirection is ReferencesDirection.Out or ReferencesDirection.Both;
            if (requestsIncoming != (incoming is not null) || requestsOutgoing != (outgoing is not null))
            {
                throw new ArgumentException("References sections must match the requested direction.");
            }

            if (requestsIncoming != (incomingSelection is not null))
            {
                throw new ArgumentException(
                    "References incoming-selection facts must match whether incoming work was requested.",
                    nameof(incomingSelection));
            }
        }

        Status = status;
        Workspace = workspace;
        Source = source;
        RequestedDirection = requestedDirection;
        IncomingSelection = incomingSelection;
        Incoming = incoming;
        Outgoing = outgoing;
        Findings = new ReadOnlyCollection<ReferencesFinding>(findingValues);
        Next = next;
    }

    public string Command => ReferencesDefinitions.CommandIdentity;

    public CliSemanticStatus Status { get; }

    public CliWorkspace? Workspace { get; }

    internal string? WorkspacePath => Workspace?.LexicalRoot;

    internal bool WorkspaceExplicit
        => Workspace?.SelectedBy == CliWorkspaceSelectionMethod.ExplicitWorkspace;

    internal ReferencesSource? Source { get; }

    internal ReferencesDirection? RequestedDirection { get; }

    internal ReferencesIncomingSelection? IncomingSelection { get; }

    internal ReferencesSection? Incoming { get; }

    internal ReferencesSection? Outgoing { get; }

    internal IReadOnlyList<ReferencesFinding> Findings { get; }

    public CliNextAction? Next { get; }
}
