using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Metadata;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes.Models;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Sources.Routing;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes;

internal sealed class RouteSourceInspector
{
    private readonly FrameworkDocumentMetadataParser _frameworkMetadataParser = new();
    private readonly MarkdownDocumentParser _markdownParser = new();
    private readonly PhysicalPathResolver _physicalPathResolver;
    private readonly SourceAuthoredMetadataParser _sourceMetadataParser = new();
    private readonly SourceReadSessionReader _sessionReader;

    internal RouteSourceInspector(
        SourceReadSessionReader sessionReader,
        PhysicalPathResolver physicalPathResolver)
    {
        _sessionReader = sessionReader;
        _physicalPathResolver = physicalPathResolver;
    }

    internal async ValueTask<RouteSourceInspection> ReadAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken)
    {
        var session = await _sessionReader.ReadAsync(workspace, cancellationToken).ConfigureAwait(false);
        var routes = await new SourceRouteFactsResolver()
            .ResolveAsync(
                new SourceRouteFactsRequest(session.Catalogue, session.Catalogue.SelectAll()),
                session.DocumentReader,
                cancellationToken)
            .ConfigureAwait(false);
        var sources = new List<RouteSourceObservation>(session.Catalogue.Sources.Count);
        foreach (var source in session.Catalogue.Sources)
        {
            sources.Add(await ReadSourceAsync(source, session.DocumentReader, cancellationToken)
                .ConfigureAwait(false));
        }

        var entry = await ReadWorkspaceEntryAsync(workspace, cancellationToken).ConfigureAwait(false);
        return new RouteSourceInspection
        {
            Catalogue = session.Catalogue,
            Routes = routes,
            WorkspaceEntry = entry.Layer,
            Sources = sources.ToArray(),
            State = RouteSourceInspectionPolicy.ReadViewState(
                session.Catalogue,
                routes,
                entry,
                sources),
        };
    }

    private async ValueTask<RouteSourceObservation> ReadSourceAsync(
        SourceLogicalSource source,
        SourceDocumentReader reader,
        CancellationToken cancellationToken)
    {
        var baseRead = await reader.ReadAsync(source.Base, cancellationToken).ConfigureAwait(false);
        var layers = new List<RouteSourceLayerObservation> { Layer(baseRead) };
        if (source.Overwrite is { } overwrite)
        {
            layers.Add(Layer(await reader.ReadAsync(overwrite, cancellationToken).ConfigureAwait(false)));
        }

        var text = baseRead.Read?.State == FileReadState.Complete
            ? baseRead.Read.Value
            : null;
        var document = text is null ? null : _markdownParser.Parse(text);
        return new RouteSourceObservation
        {
            Source = source,
            Layers = layers.ToArray(),
            Document = document,
            AuthoredMetadata = document is null
                ? SourceAuthoredMetadataFacts.WithoutValues(SourceAuthoredMetadataState.Malformed)
                : _sourceMetadataParser.Parse(document, source.Base.Form),
            FrameworkMetadata = document is null
                ? FrameworkDocumentMetadataFacts.WithoutValues(FrameworkDocumentMetadataState.Malformed)
                : _frameworkMetadataParser.Parse(document),
            GeneratedEntries = document is null
                ? SourceGeneratedEntriesFacts.Unavailable("The source document is unavailable.")
                : SourceGeneratedEntriesParser.Parse(document),
        };
    }

    private async ValueTask<RouteWorkspaceEntryObservation>
        ReadWorkspaceEntryAsync(
            CliWorkspace workspace,
            CancellationToken cancellationToken)
    {
        const string path = SourceLogicalPath.WorkspaceEntryPath;
        var resolution = _physicalPathResolver.ResolveCandidate(
            workspace.LexicalRoot,
            workspace.PhysicalRoot,
            Path.Combine(workspace.LexicalRoot, path));
        if (resolution.State == PhysicalPathState.Missing)
        {
            return new RouteWorkspaceEntryObservation(
                new RouteSourceLayerObservation(path, FileReadState.Missing, null),
                OperationalViewState.Incomplete);
        }

        if (resolution.State is PhysicalPathState.External
            or PhysicalPathState.Dangling
            or PhysicalPathState.Cycle
            or PhysicalPathState.Invalid
            or PhysicalPathState.Unsupported)
        {
            return new RouteWorkspaceEntryObservation(
                new RouteSourceLayerObservation(path, FileReadState.InputOutputFailure, null),
                OperationalViewState.Blocked);
        }

        if (resolution.State != PhysicalPathState.Contained)
        {
            return new RouteWorkspaceEntryObservation(
                new RouteSourceLayerObservation(path, FileReadState.InputOutputFailure, null),
                OperationalViewState.Incomplete);
        }

        var read = await StrictUtf8FileReader
            .ReadAsync(resolution.GetContainedPhysicalPath(), path, cancellationToken)
            .ConfigureAwait(false);
        var state = read.State switch
        {
            FileReadState.Complete => OperationalViewState.Complete,
            FileReadState.Cancelled => OperationalViewState.Interrupted,
            FileReadState.Missing
                or FileReadState.InvalidEncoding
                or FileReadState.InvalidSyntax
                or FileReadState.AccessDenied
                or FileReadState.InputOutputFailure => OperationalViewState.Incomplete,
            _ => throw new ArgumentOutOfRangeException(
                nameof(read),
                read.State,
                "The workspace-entry read state is not defined."),
        };
        return new RouteWorkspaceEntryObservation(
            new RouteSourceLayerObservation(path, read.State, read.Value),
            state);
    }

    private static RouteSourceLayerObservation Layer(SourceDocumentReadResult read)
        => new(
            read.Layer.CanonicalPath,
            RouteSourceInspectionPolicy.ReadLayerState(read),
            read.Read?.Value);
}
