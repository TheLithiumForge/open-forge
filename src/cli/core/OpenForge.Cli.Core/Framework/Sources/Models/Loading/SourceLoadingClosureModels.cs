using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.Core.Framework.Sources.Models.Loading;

internal enum SourceLoadingClosureReasonKind
{
    WorkspaceEntry,
    Loader,
    LoadNow,
    KeepInMind,
    AncestorRequired,
}

internal sealed record SourceLoadingClosureReason
{
    internal SourceLoadingClosureReason(
        SourceLoadingClosureReasonKind kind,
        string? sourcePath)
    {
        var coherent = kind switch
        {
            SourceLoadingClosureReasonKind.WorkspaceEntry
                or SourceLoadingClosureReasonKind.Loader => sourcePath is null,
            SourceLoadingClosureReasonKind.LoadNow
                or SourceLoadingClosureReasonKind.AncestorRequired => !string.IsNullOrWhiteSpace(sourcePath),
            SourceLoadingClosureReasonKind.KeepInMind => sourcePath is null
                || !string.IsNullOrWhiteSpace(sourcePath),
            _ => throw new ArgumentOutOfRangeException(
                nameof(kind),
                kind,
                "The source loading closure reason kind is not defined."),
        };
        if (!coherent)
        {
            throw new ArgumentException(
                "The source loading closure reason source does not match its kind.",
                nameof(sourcePath));
        }

        Kind = kind;
        SourcePath = sourcePath;
    }

    internal SourceLoadingClosureReasonKind Kind { get; }

    internal string? SourcePath { get; }
}

internal enum SourceLoadingClosureIssueKind
{
    LoaderUnavailable,
    LoadingMetadataUnavailable,
    ContinuityRouteUnavailable,
    ContinuityChainUnavailable,
    GeneratedEntriesUnavailable,
    VisibleMetadataUnavailable,
}

internal sealed record SourceLoadingClosureIssue
{
    internal SourceLoadingClosureIssue(
        SourceLoadingClosureIssueKind kind,
        string path,
        string cause)
    {
        _ = kind switch
        {
            SourceLoadingClosureIssueKind.LoaderUnavailable
                or SourceLoadingClosureIssueKind.LoadingMetadataUnavailable
                or SourceLoadingClosureIssueKind.ContinuityRouteUnavailable
                or SourceLoadingClosureIssueKind.ContinuityChainUnavailable
                or SourceLoadingClosureIssueKind.GeneratedEntriesUnavailable
                or SourceLoadingClosureIssueKind.VisibleMetadataUnavailable => true,
            _ => throw new ArgumentOutOfRangeException(
                nameof(kind),
                kind,
                "The source loading closure issue kind is not defined."),
        };

        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(cause);
        Kind = kind;
        Path = path;
        Cause = cause;
    }

    internal SourceLoadingClosureIssueKind Kind { get; }

    internal string Path { get; }

    internal string Cause { get; }
}

internal sealed record SourceLoadingClosureSource
{
    public required string Path { get; init; }

    public required SourceDocumentForm Form { get; init; }

    public required SourceRouteState RouteState { get; init; }

    public required string? ParentPath { get; init; }

    public required SourceAuthoredMetadataFacts Metadata { get; init; }

    public required SourceGeneratedEntriesFacts GeneratedEntries { get; init; }

    internal bool IsEntrypoint => SourceFormClassifier.IsEntrypoint(Form);

    internal bool IsLoader => Form == SourceDocumentForm.Loader;
}

internal sealed record SourceLoadingClosureRequest
{
    public required string WorkspaceEntryPath { get; init; }

    public required IReadOnlyList<SourceLoadingClosureSource> Sources { get; init; }

    public required IReadOnlyList<string> LoaderRootPaths { get; init; }
}

internal sealed record SourceLoadingClosureSelection
{
    public required string Path { get; init; }

    public required IReadOnlyList<SourceLoadingClosureReason> Reasons { get; init; }
}

internal sealed record SourceLoadingClosureResolution
{
    public required IReadOnlyList<SourceLoadingClosureSelection> Startup { get; init; }

    public required IReadOnlyList<string> ContinuityPaths { get; init; }

    public required IReadOnlyList<SourceLoadingClosureIssue> Issues { get; init; }
}
