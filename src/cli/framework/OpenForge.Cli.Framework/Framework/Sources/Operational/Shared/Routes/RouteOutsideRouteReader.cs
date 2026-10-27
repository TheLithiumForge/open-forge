using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.Routes;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Applicability.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;

namespace OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes;

internal static class RouteOutsideRouteReader
{
    internal static WorkspaceLayout InventoryLayout(WorkspaceLayoutRead observation)
    {
        var layout = observation.Layout ?? throw new InvalidOperationException("Inventory requires its retained layout.");
        if (observation.OutsideRoutes.Length == layout.OutsideRoutes.Length
            && observation.OutsideRoutes.All(route => route.State == WorkspaceOutsideRouteReadState.Available)) return layout;
        return layout.WithOutsideRoutes(observation.OutsideRoutes
            .Where(route => route.State == WorkspaceOutsideRouteReadState.Available)
            .Select(route => route.Descriptor.DirectoryPath));
    }

    internal static string? MaintenanceRoot(WorkspaceLayout layout, string path)
        => layout.OutsideRoutes.SingleOrDefault(route => Contains(route.DirectoryPath, path))?.DirectoryPath;

    internal static bool Contains(string directory, string path)
        => path == directory || path.StartsWith(directory + "/", StringComparison.Ordinal);

    internal static async ValueTask<SourceLoadingClosureResolution> ReadLoadingAsync(
        RouteSourceInspection inspection, SourceCatalogue catalogue, CancellationToken cancellationToken)
    {
        var reader = inspection.Session?.DocumentReader
            ?? throw new InvalidOperationException("Loading provenance requires the retained read session.");
        var observed = inspection.Sources.ToDictionary(source => source.Source.Base.CanonicalPath, StringComparer.Ordinal);
        var routeFacts = inspection.Routes.RouteFacts.ToDictionary(fact => fact.Identity.CanonicalBasePath, StringComparer.Ordinal);
        var sources = new List<SourceLoadingClosureSource>();
        foreach (var source in catalogue.Sources)
        {
            var read = await reader.ReadAsync(source.Base, cancellationToken).ConfigureAwait(false);
            var known = observed.GetValueOrDefault(source.Base.CanonicalPath);
            var node = inspection.Routes.Topology.FindByPath(source.Base.CanonicalPath);
            sources.Add(new SourceLoadingClosureSource
            {
                Path = source.Base.CanonicalPath,
                Form = source.Base.Form,
                RouteState = routeFacts.GetValueOrDefault(source.Base.CanonicalPath)?.State
                    ?? SourceRouteState.Unrouted,
                ParentPath = source.IncludedParentPath ?? (node?.ParentState == SourceRouteParentState.Resolved ? node.ParentPaths[0] : null),
                Metadata = source.IncludedEntry is { } entry ? SourceAuthoredMetadataFacts.Complete(entry.Description, entry.Tags)
                    : known?.AuthoredMetadata ?? SourceAuthoredMetadataFacts.WithoutValues(SourceAuthoredMetadataState.Missing),
                GeneratedEntries = read.Read is { State: FileReadState.Complete, Value: { } body }
                    ? SourceGeneratedEntriesParser.Parse(reader.ParseDocument(body))
                    : SourceGeneratedEntriesFacts.Unavailable("The retained source body is unavailable."),
                IsLinked = source.IncludedEntry is not null,
                LinkedApplyTo = source.IncludedEntry is { } row ? ApplyToMetadataFacts.FromEntryLine(row.ApplyTo) : ApplyToMetadataFacts.Absent,
            });
        }
        return new SourceLoadingClosureResolver().Resolve(new SourceLoadingClosureRequest
        {
            Layout = catalogue.Layout,
            WorkspaceEntryPath = inspection.WorkspaceEntry.Path,
            Sources = sources,
            LoaderRootPaths = inspection.Routes.Topology.LoaderRootPaths,
            Aliases = catalogue.Aliases,
            IncludeLinkedSources = true,
            EvaluateApplicability = true,
        });
    }

    internal static RouteOutsideRoutesObservation Read(WorkspaceLayoutRead observation,
        RouteSourceInspection? inspection, RouteIncludedFilesObservation? included, SourceCatalogue? includedCatalogue = null,
        SourceLoadingClosureResolution? loading = null)
    {
        var registrationsComplete = observation.Layout is { } retainedLayout
            && observation.OutsideRoutes.Select(route => route.Descriptor.DirectoryPath)
                .SequenceEqual(retainedLayout.OutsideRoutes.Select(route => route.DirectoryPath), StringComparer.Ordinal);
        var rows = includedCatalogue?.IncludedRows ?? included?.Rows ?? [];
        var routes = observation.OutsideRoutes.Select(route => new RouteOutsideRouteObservation(route,
            inspection?.Sources.Where(source => Contains(route.Descriptor.DirectoryPath, source.Source.Base.CanonicalPath))
                .Select(source => source.Source.Base.CanonicalPath).ToArray() ?? [],
            inspection?.Sources.Where(source => SourceFormClassifier.IsEntrypoint(source.Source.Base.Form)
                    && SourceLogicalPath.ReadParent(source.Source.Base.CanonicalPath) == route.Descriptor.DirectoryPath)
                .Select(source => source.Source.Base.CanonicalPath).ToArray() ?? [],
            rows.Where(row => Contains(route.Descriptor.DirectoryPath, row.Path)
                || Contains(route.Descriptor.DirectoryPath, row.ParentPath)).ToArray())
        {
            Listed = ReadListing(observation, inspection, route, rows),
        }).ToArray();
        var inventory = new List<(string Physical, RouteContextSource Source, bool Startup, bool Continuity)>();
        var workspaceEntryIdentityComplete = true;
        if (inspection is not null)
        {
            var current = RouteCurrentContextReader.Read(inspection);
            var closure = new RouteContextClosureResolver().Resolve(inspection.Catalogue.Layout, current);
            var startup = closure.Startup.SelectMany(source => source.Layers).Select(layer => layer.Path).ToHashSet(StringComparer.Ordinal);
            var continuity = closure.Continuity.SelectMany(source => source.Layers).Select(layer => layer.Path).ToHashSet(StringComparer.Ordinal);
            foreach (var source in current.Sources)
            {
                var logical = inspection.Catalogue.FindByPath(source.Path)
                    ?? throw new InvalidOperationException("An inspected contribution requires its retained source.");
                foreach (var layer in source.Layers)
                {
                    var physical = layer.Path == logical.Base.CanonicalPath ? logical.Base.PhysicalPath
                        : logical.Overwrite?.PhysicalPath ?? throw new InvalidOperationException("An overwrite contribution requires its retained layer.");
                    inventory.Add((physical, source with { Layers = [layer] }, startup.Contains(layer.Path), continuity.Contains(layer.Path)));
                }
            }
            if (inspection.WorkspaceEntry.Text is not null)
            {
                var workspace = inspection.Catalogue.Workspace;
                var physical = inspection.Session?.DocumentReader.PhysicalPaths.ResolveCandidate(
                    workspace.LexicalRoot, workspace.PhysicalRoot,
                    SourceLogicalPath.ToLexicalPath(workspace.LexicalRoot, current.WorkspaceEntry.Path));
                workspaceEntryIdentityComplete = physical?.State == PhysicalPathState.Contained;
                if (physical is { State: PhysicalPathState.Contained })
                    inventory.Add((physical.GetContainedPhysicalPath(), current.WorkspaceEntry,
                        startup.Contains(current.WorkspaceEntry.Path), continuity.Contains(current.WorkspaceEntry.Path)));
            }
        }
        var linked = included?.Layers ?? [];
        var startupPaths = loading?.Startup.Select(item => item.Path).ToHashSet(StringComparer.Ordinal) ?? [];
        var continuityPaths = loading?.ContinuityPaths.ToHashSet(StringComparer.Ordinal) ?? [];
        var loadingComplete = loading is not null && loading.Issues.Count == 0 && loading.PendingConditions.Count == 0;
        var startupComplete = loadingComplete && workspaceEntryIdentityComplete;
        var continuityComplete = loadingComplete
            && (workspaceEntryIdentityComplete || !continuityPaths.Contains(SourceLogicalPath.WorkspaceEntryPath));
        var inventoryByPhysical = inventory.ToLookup(item => item.Physical, PhysicalIdentityTracker.PathComparer);
        var linkedByPhysical = linked.ToLookup(item => item.PhysicalPath, PhysicalIdentityTracker.PathComparer);
        var paths = inventory.Select(item => item.Physical).Concat(linked.Select(item => item.PhysicalPath))
            .Distinct(PhysicalIdentityTracker.PathComparer).Order(StringComparer.Ordinal);
        var layers = paths.Select(physical =>
        {
            var owned = inventoryByPhysical[physical].ToArray();
            var listed = linkedByPhysical[physical].ToArray();
            var source = owned.Length > 0 ? owned[0].Source : listed[0].Source;
            var contributions = owned.Select(item => item.Source).Concat(listed.Select(item => item.Source)).ToArray();
            return new RouteOutsideLayerObservation(physical,
                contributions.SelectMany(item => item.Layers)
                    .Select(layer => layer.Path).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray(),
                source, owned.Length > 0, listed.Length > 0, owned.Any(item => item.Startup), owned.Any(item => item.Continuity),
                listed.Any(item => item.Startup), listed.Any(item => item.Continuity), RouteContextMeasurementReader.Measure([source], true))
            {
                LoadingStartup = ReadMembership(contributions, startupPaths, startupComplete),
                LoadingContinuity = ReadMembership(contributions, continuityPaths, continuityComplete),
            };
        }).ToArray();
        var measuredPaths = inventory.Select(item => item.Source).Concat(linked.Select(item => item.Source))
            .SelectMany(source => source.Layers.Select(layer => layer.Path).Append(source.Path)).ToHashSet(StringComparer.Ordinal);
        return new RouteOutsideRoutesObservation
        {
            LayoutRead = observation,
            RegistrationInventoryComplete = registrationsComplete,
            Inspection = inspection,
            IncludedFiles = included,
            IncludedCatalogue = includedCatalogue,
            Loading = loading,
            Startup = RouteContextMeasurementReader.Measure(layers.Where(layer => layer.LoadingStartup is true).Select(layer => layer.Source),
                startupComplete && startupPaths.IsSubsetOf(measuredPaths)),
            Continuity = RouteContextMeasurementReader.Measure(layers.Where(layer => layer.LoadingContinuity is true).Select(layer => layer.Source),
                continuityComplete && continuityPaths.IsSubsetOf(measuredPaths)),
            Routes = routes,
            Layers = layers,
            Total = RouteContextMeasurementReader.Measure(layers.Select(layer => layer.Source),
                workspaceEntryIdentityComplete && registrationsComplete && inspection is not null && RouteSourceInspectionPolicy.IsCatalogueComplete(inspection.Catalogue)
                && inspection.WorkspaceEntry.State is FileReadState.Complete or FileReadState.Missing
                && observation.OutsideRoutes.All(route => route.State == WorkspaceOutsideRouteReadState.Available)
                && (included is null || !RouteContextMeasurementReader.IsUnavailable(included.Total))),
        };
    }

    private static bool? ReadListing(WorkspaceLayoutRead observation, RouteSourceInspection? inspection,
        WorkspaceOutsideRouteRead route, IReadOnlyList<SourceIncludedEntryRow> rows)
    {
        if (observation.Layout is not { } layout) return null;
        var loader = SourceLogicalPath.ToLexicalPath(layout.Workspace.LexicalRoot, layout.LoaderPath);
        if (observation.Expectations.Any(expectation => expectation.Kind == FileExpectationKind.Missing
            && string.Equals(expectation.LogicalPath, loader, StringComparison.Ordinal))) return false;
        if (inspection?.Sources.FirstOrDefault(source => source.Source.Base.Form == SourceDocumentForm.Loader)
            ?.GeneratedEntries.State != SourceGeneratedEntriesState.Complete) return null;
        var direct = rows.Where(row => row.ParentPath == layout.LoaderPath).ToArray();
        if (direct.Any(row => row.Path == route.Descriptor.CataloguePath)) return true;
        var compatibility = direct.Where(row => SourceLogicalPath.ReadParent(row.Path) == route.Descriptor.DirectoryPath
            && SourceIdentity.IsRecognizedEntrypointPath(layout, row.Path)).ToArray();
        if (compatibility.Length == 0) return false;
        if (compatibility.Any(row => inspection.Sources.Any(source => source.Source.Base.CanonicalPath == row.Path
            && SourceFormClassifier.IsEntrypoint(source.Source.Base.Form)
            && !source.Layers.Any(layer => layer.Path == row.Path && layer.State == FileReadState.Missing)))) return true;
        if (route.State == WorkspaceOutsideRouteReadState.Missing) return false;
        if (route.State is WorkspaceOutsideRouteReadState.Unsafe or WorkspaceOutsideRouteReadState.Unavailable
            || inspection.Catalogue.IsCancelled
            || compatibility.Any(row => inspection.Catalogue.Issues.Any(issue => row.Path == issue.AttemptedCanonicalPath
                || row.Path.StartsWith(issue.AttemptedCanonicalPath + "/", StringComparison.Ordinal)))) return null;
        return false;
    }

    private static bool? ReadMembership(IEnumerable<RouteContextSource> contributions, IReadOnlySet<string> paths, bool complete)
    {
        if (contributions.Any(source => paths.Contains(source.Path) || source.Layers.Any(layer => paths.Contains(layer.Path)))) return true;
        return complete ? false : null;
    }
}
