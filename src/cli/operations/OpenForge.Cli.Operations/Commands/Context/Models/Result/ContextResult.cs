using OpenForge.Cli.Core.Commands.Shared.Models;
using OpenForge.Cli.Core.Commands.Context.Shared.Result;
using OpenForge.Cli.Core.Commands.Context.Models.Selection;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Context.Models.Result;

internal enum ContextFindingCode
{
    InvalidInput,
    InvalidSource,
    InvalidContent,
    InvalidLinkDepth,
    WorkspaceUnavailable,
    WorkspaceUnsafe,
    SourceAmbiguous,
    SourceUnsafe,
    OverwriteAmbiguous,
    TargetAmbiguous,
    TargetUnsafe,
    ClosureUnavailable,
    LayerUnavailable,
    InvalidEncoding,
    MarkdownUnavailable,
    TargetMissing,
    FragmentMissing,
    LinkEncodingInvalid,
    TargetUnreadable,
    SectionAmbiguous,
    ProjectionUnavailable,
    IdentityCollision,
    TargetCaseMismatch,
    FrontmatterMissing,
    SectionMissing,
    OperationFailed,
    Interrupted,
}

internal enum ContextInvalidSourceKind
{
    UnknownId,
    Other,
}

internal sealed record ContextFindingDefinition(string Code, CliSemanticStatus Status);

internal sealed record ContextFinding
{
    internal ContextFinding(
        ContextFindingCode code,
        string? subject,
        string cause,
        string? reference,
        ContextSourceIdentity? source,
        ContextSourceLayerKind? layer,
        string? path,
        ContextContentPart? part,
        SourceLocation? location,
        SourceLocation? destinationLocation,
        IEnumerable<ContextSourceIdentity> candidates,
        ContextInvalidSourceKind? invalidSourceKind = null)
    {
        var definition = ContextDefinitions.Read(code);
        ArgumentNullException.ThrowIfNull(cause);
        Code = code;
        Status = definition.Status;
        Subject = subject;
        Cause = cause;
        Reference = reference;
        Source = source;
        Layer = layer;
        Path = path;
        Part = part;
        Location = location;
        DestinationLocation = destinationLocation;
        Candidates = ContextResultCollections.Snapshot(candidates, nameof(candidates));
        InvalidSourceKind = invalidSourceKind;
    }

    internal ContextFindingCode Code { get; }

    internal CliSemanticStatus Status { get; }

    internal string? Subject { get; }

    internal string Cause { get; }

    internal string? Reference { get; }

    internal ContextSourceIdentity? Source { get; }

    internal ContextSourceLayerKind? Layer { get; }

    internal string? Path { get; }

    internal ContextContentPart? Part { get; }

    internal SourceLocation? Location { get; }

    internal CommandSourceLocation? LocationView
        => Location is { } location ? CommandSourceLocation.From(location) : null;

    internal SourceLocation? DestinationLocation { get; }

    internal IReadOnlyList<ContextSourceIdentity> Candidates { get; }

    internal ContextInvalidSourceKind? InvalidSourceKind { get; }
}

internal sealed record ContextResult : ICliCommandResult
{
    internal ContextResult(
        CliWorkspace? workspace,
        ContextSelection selection,
        ContextPresentation presentation,
        ContextCoverage coverage,
        IEnumerable<ContextPathProjection> paths,
        IEnumerable<ContextLink> links,
        IEnumerable<ContextSource> sources,
        IEnumerable<ContextFinding> findings,
        CliSemanticStatus status,
        CliNextAction? next,
        ContextCounts? counts = null)
    {
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(presentation);
        ArgumentNullException.ThrowIfNull(coverage);
        _ = CliStatusDefinitions.Read(status);
        Workspace = workspace;
        Selection = selection;
        Presentation = presentation;
        Coverage = coverage;
        Paths = ContextResultCollections.Snapshot(paths, nameof(paths));
        Links = ContextResultCollections.Snapshot(links, nameof(links));
        Sources = ContextResultCollections.Snapshot(sources, nameof(sources));
        Findings = ContextResultCollections.Snapshot(findings, nameof(findings));
        Status = status;
        Next = next;
        Counts = counts ?? ContextCounts.Unavailable("Context counts were not established.");
    }

    public string Command => ContextDefinitions.CommandIdentity;

    public CliSemanticStatus Status { get; }

    public CliWorkspace? Workspace { get; }

    internal string? WorkspacePath => Workspace?.LexicalRoot;

    internal bool WorkspaceExplicit => Workspace?.SelectedBy == CliWorkspaceSelectionMethod.ExplicitWorkspace;

    internal ContextSelection Selection { get; }

    internal ContextPresentation Presentation { get; }

    internal ContextCoverage Coverage { get; }

    internal IReadOnlyList<ContextPathProjection> Paths { get; }

    internal IReadOnlyList<ContextLink> Links { get; }

    internal IReadOnlyList<ContextSource> Sources { get; }

    internal IReadOnlyList<ContextFinding> Findings { get; }

    internal ContextCounts Counts { get; }

    public CliNextAction? Next { get; }
}
