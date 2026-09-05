using OpenForge.Cli.Core.Framework.Distribution;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes.Models;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.UnitTests.Commands.Status;

internal static class StatusRouteTotalAvailableSupport
{
    internal static (OperationalViewState State, RouteContextObservation Context) Read(
        bool installed,
        FileReadState entryReadState,
        string? entryText,
        OperationalViewState entryViewState)
    {
        var workspace = Workspace();
        var source = installed ? Source(workspace) : null;
        var catalogue = new SourceCatalogue(
            workspace,
            [],
            source is null ? [] : [source],
            installed ? [] : [MissingRootIssue(workspace)],
            isCancelled: false);
        var topology = new SourceRouteTopology(
            source is null
                ? []
                : [new SourceRouteNode(
                    source.Identity,
                    SourceRouteParentState.None,
                    [],
                    [])],
            []);
        var routes = new SourceRouteFacts(
            topology,
            [],
            [],
            areLoaderRootFactsComplete: true,
            isCancelled: false);
        var sources = source is null
            ? []
            : new[] { Observation(source) };
        var entry = new RouteWorkspaceEntryObservation(
            new RouteSourceLayerObservation(
                "AGENTS.md",
                entryReadState,
                entryText),
            entryViewState);
        var inspection = new RouteSourceInspection
        {
            Catalogue = catalogue,
            Routes = routes,
            WorkspaceEntry = entry.Layer,
            Sources = sources,
            State = RouteSourceInspectionPolicy.ReadViewState(
                catalogue,
                routes,
                entry,
                sources),
        };

        var context = new RouteContextReader().Read(
            EmbeddedFrameworkPayloadReader.Read(),
            inspection);

        return (inspection.State, context);
    }

    internal static void AssertMeasurement(
        ContextMeasurementObservation observation,
        long expectedFiles,
        long expectedCharacters,
        long expectedBytes,
        long expectedTokens)
    {
        Assert.Equal(OperationalValueState.Available, observation.Files.State);
        Assert.Equal(OperationalValueState.Available, observation.Characters.State);
        Assert.Equal(OperationalValueState.Available, observation.Utf8Bytes.State);
        Assert.Equal(OperationalValueState.Available, observation.EstimatedTokens.State);
        Assert.Equal(expectedFiles, observation.Files.Value);
        Assert.Equal(expectedCharacters, observation.Characters.Value);
        Assert.Equal(expectedBytes, observation.Utf8Bytes.Value);
        Assert.Equal(expectedTokens, observation.EstimatedTokens.Value);
    }

    internal static void AssertUnavailable(RouteContextObservation context)
    {
        Assert.True(context.IsIncomplete);
        Assert.Equal(OperationalValueState.Unavailable, context.TotalAvailable.Files.State);
        Assert.Null(context.TotalAvailable.Files.Value);
        Assert.Null(context.TotalAvailable.Characters.Value);
        Assert.Null(context.TotalAvailable.Utf8Bytes.Value);
        Assert.Null(context.TotalAvailable.EstimatedTokens.Value);
    }

    private static CliWorkspace Workspace()
    {
        var path = Path.GetFullPath(Path.Combine(
            Path.GetTempPath(),
            "status-route-total-available-unit"));
        return new CliWorkspace(path, path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
    }

    private static SourceLogicalSource Source(CliWorkspace workspace)
    {
        const string path = ".agents/source.md";
        return new SourceLogicalSource(
            new SourceLogicalIdentity("source", path),
            new SourceLayer(
                path,
                Path.Combine(workspace.PhysicalRoot, ".agents", "source.md"),
                SourceDocumentForm.Markdown,
                SourceLayerKind.Base));
    }

    private static RouteSourceObservation Observation(SourceLogicalSource source)
        => new()
        {
            Source = source,
            Layers = [new RouteSourceLayerObservation(
                source.Base.CanonicalPath,
                FileReadState.Complete,
                "# Source\n")],
            Document = null,
            AuthoredMetadata = SourceAuthoredMetadataFacts.WithoutValues(
                SourceAuthoredMetadataState.Malformed),
            FrameworkMetadata = FrameworkDocumentMetadataFacts.WithoutValues(
                FrameworkDocumentMetadataState.Missing),
            GeneratedEntries = SourceGeneratedEntriesFacts.Absent,
            Structure = new RouteSourceStructureObservation(
                RouteTitleObservation.NotApplicable(),
                RouteAxiomsObservation.NotApplicable()),
            WorkspaceIssues = [],
        };

    private static SourceCatalogueIssue MissingRootIssue(CliWorkspace workspace)
        => new(
            SourceCatalogueIssueCode.RootMissing,
            Path.Combine(workspace.LexicalRoot, ".agents"),
            [],
            scopePhysicalPath: null,
            failure: null);
}
