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
        IEnumerable<ContextSourceIdentity> candidates)
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

    internal SourceLocation? DestinationLocation { get; }

    internal IReadOnlyList<ContextSourceIdentity> Candidates { get; }
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
        CliNextAction? next)
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
    }

    public string Command => ContextDefinitions.CommandIdentity;

    public CliSemanticStatus Status { get; }

    public CliWorkspace? Workspace { get; }

    internal ContextSelection Selection { get; }

    internal ContextPresentation Presentation { get; }

    internal ContextCoverage Coverage { get; }

    internal IReadOnlyList<ContextPathProjection> Paths { get; }

    internal IReadOnlyList<ContextLink> Links { get; }

    internal IReadOnlyList<ContextSource> Sources { get; }

    internal IReadOnlyList<ContextFinding> Findings { get; }

    public CliNextAction? Next { get; }
}
