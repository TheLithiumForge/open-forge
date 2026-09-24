using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Commands.Remove.Models.Planning;

namespace OpenForge.Cli.Core.Commands.Remove.Shared.Planning;

internal sealed class RemoveNavigationPlanner
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);
    private readonly SourceCatalogueReader _catalogueReader = new();
    private readonly GeneratedNavigationFormationBuilder _formationBuilder = new();
    private readonly GeneratedNavigationRegionPlanner _regionPlanner = new();
    private readonly MarkdownDocumentParser _markdownParser = new();
    private readonly SourceAuthoredMetadataParser _metadataParser = new();

    internal async ValueTask<RemoveNavigationPlanningOutcome> BuildAsync(
        CliWorkspace workspace,
        IReadOnlyCollection<string> selectedPaths,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentNullException.ThrowIfNull(selectedPaths);
        if (selectedPaths.Count == 0 || selectedPaths.All(path => !path.StartsWith(".agents/", StringComparison.Ordinal)))
        {
            return new RemoveNavigationPlanningOutcome.Available(RemoveNavigationPlan.Empty);
        }

        var catalogue = await _catalogueReader.ReadAsync(
            new SourceCatalogueRequest(workspace, [SourceLogicalPath.AgentsRoot]),
            cancellationToken).ConfigureAwait(false);
        if (catalogue.IsCancelled)
        {
            return new RemoveNavigationPlanningOutcome.Unavailable("The .agents source catalogue could not be completely read.");
        }

        if (catalogue.Issues.Any(issue => issue.Stage == SourceCatalogueIssueStage.Root
                && issue.Code != SourceCatalogueIssueCode.RootMissing))
        {
            return new RemoveNavigationPlanningOutcome.Unavailable("The .agents source catalogue is unsafe or unavailable.");
        }

        if (catalogue.Issues.Any(issue => issue.Stage == SourceCatalogueIssueStage.Root
                && issue.Code == SourceCatalogueIssueCode.RootMissing))
        {
            return new RemoveNavigationPlanningOutcome.Available(RemoveNavigationPlan.Empty);
        }

        if (catalogue.Issues.Count != 0)
        {
            return new RemoveNavigationPlanningOutcome.Unavailable(
                "The .agents source catalogue contains unresolved candidates, so generated navigation cannot be projected safely.");
        }

        var selected = selectedPaths.ToHashSet(StringComparer.Ordinal);
        var removedSources = catalogue.Sources
            .Where(source => selected.Contains(source.Identity.CanonicalBasePath))
            .ToArray();
        var removedOverwritePaths = catalogue.Sources
            .Select(source => source.Overwrite?.CanonicalPath)
            .OfType<string>()
            .Where(selected.Contains)
            .ToHashSet(StringComparer.Ordinal);
        var intendedSources = catalogue.Sources
            .Where(source => !selected.Contains(source.Identity.CanonicalBasePath))
            .Select(source => removedOverwritePaths.Contains(source.Overwrite?.CanonicalPath ?? string.Empty)
                ? new SourceLogicalSource(source.Identity, source.Base)
                : source)
            .ToArray();
        var observedFormation = _formationBuilder.Build(catalogue);
        var intendedFormation = _formationBuilder.Build(catalogue, intendedSources);
        if (intendedFormation.IntendedTargetCollisions.Count != 0 || intendedFormation.Ambiguities.Count != 0)
        {
            return new RemoveNavigationPlanningOutcome.Unavailable(
                "The intended remaining .agents catalogue has a route identity or physical target collision.");
        }

        var affectedHosts = SelectAffectedHosts(removedSources, observedFormation, intendedFormation, selectedPaths);
        if (affectedHosts.Count == 0)
        {
            return new RemoveNavigationPlanningOutcome.Available(RemoveNavigationPlan.Empty);
        }

        var metadata = new List<GeneratedNavigationMetadata>(intendedSources.Length);
        var snapshots = new List<FileStateSnapshot>(intendedSources.Length);
        foreach (var source in intendedSources)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var read = ReadSource(workspace, source);
            if (read is null)
            {
                return new RemoveNavigationPlanningOutcome.Unavailable(
                    $"Authored metadata for '{source.Identity.CanonicalBasePath}' could not be read exactly.");
            }

            snapshots.Add(read.Snapshot);
            metadata.Add(new GeneratedNavigationMetadata(
                source,
                _metadataParser.Parse(_markdownParser.Parse(read.Text), source.Base.Form)));
        }

        var hostDocuments = new List<(SourceLogicalSource Source, RemoveNavigationSourceText Text)>();
        foreach (var host in affectedHosts)
        {
            var read = ReadSource(workspace, host);
            if (read is null)
            {
                return new RemoveNavigationPlanningOutcome.Unavailable(
                    $"Generated navigation source '{host.Identity.CanonicalBasePath}' could not retain exact UTF-8 bytes.");
            }
            hostDocuments.Add((host, read));
        }

        var regions = hostDocuments.Select(document => new GeneratedNavigationRegionInput(
            document.Source,
            _markdownParser.Parse(document.Text.Text))).ToArray();
        var projectionRequest = new GeneratedNavigationProjectionRequest(intendedFormation, regions, metadata);
        var changes = new List<RemoveNavigationChange>();
        foreach (var document in hostDocuments)
        {
            var regionInput = regions.Single(region => ReferenceEquals(region.Source, document.Source));
            var projection = _regionPlanner.Plan(projectionRequest, regionInput);
            if (projection.State != GeneratedNavigationRegionState.Available || projection.Change is null)
            {
                return new RemoveNavigationPlanningOutcome.Unavailable(
                    projection.Cause ?? $"Generated navigation for '{projection.CanonicalPath}' is unavailable.");
            }

            if (!projection.Change.RequiresUpdate)
            {
                continue;
            }

            var logicalPath = Path.Combine(
                workspace.LexicalRoot,
                projection.CanonicalPath.Replace('/', Path.DirectorySeparatorChar));
            changes.Add(new RemoveNavigationChange(
                PlannedFileChange.ReplaceGeneratedRegion(
                    document.Text.Snapshot.Expectation,
                    projection.Change.ExpectedDocumentBytes.AsSpan()),
                document.Text.Snapshot,
                logicalPath));
        }

        return new RemoveNavigationPlanningOutcome.Available(new RemoveNavigationPlan(
            [.. changes.OrderBy(change => change.Snapshot.LogicalPath, StringComparer.Ordinal)],
            [.. snapshots.DistinctBy(snapshot => snapshot.LogicalPath, StringComparer.Ordinal)
                .OrderBy(snapshot => snapshot.LogicalPath, StringComparer.Ordinal)]));
    }

    private static IReadOnlyList<SourceLogicalSource> SelectAffectedHosts(
        IReadOnlyList<SourceLogicalSource> removedSources,
        GeneratedNavigationFormation observed,
        GeneratedNavigationFormation intended,
        IReadOnlyCollection<string> selectedPaths)
    {
        var paths = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var source in removedSources)
        {
            var node = observed.Topology.FindByPath(source.Identity.CanonicalBasePath);
            if (node is null)
            {
                continue;
            }

            var hostPath = node.ParentState switch
            {
                SourceRouteParentState.Resolved => node.ParentPaths[0],
                SourceRouteParentState.Ambiguous => observed.Loader?.Identity.CanonicalBasePath,
                _ => null,
            };
            if (hostPath is not null && intended.FindSource(hostPath) is not null)
            {
                paths.Add(hostPath);
            }
        }

        foreach (var selectedPath in selectedPaths)
        {
            if (!SourceLogicalPath.IsCanonicalSource(selectedPath))
            {
                continue;
            }

            var directory = SourceLogicalPath.ReadParent(selectedPath);
            while (true)
            {
                foreach (var host in observed.Sources.Where(source => IsNavigationHost(source)
                    && string.Equals(
                        SourceLogicalPath.ReadParent(source.Identity.CanonicalBasePath),
                        directory,
                        StringComparison.Ordinal)))
                {
                    if (intended.FindSource(host.Identity.CanonicalBasePath) is not null)
                    {
                        paths.Add(host.Identity.CanonicalBasePath);
                    }
                }

                if (directory == SourceLogicalPath.AgentsRoot)
                {
                    break;
                }
                directory = SourceLogicalPath.ReadParent(directory);
            }
        }

        return paths.Select(path => intended.FindSource(path)
                ?? throw new InvalidOperationException("An affected navigation host must be part of the intended formation."))
            .ToArray();
    }

    private static bool IsNavigationHost(SourceLogicalSource source)
        => source.Base.Form == SourceDocumentForm.Loader
            || SourceFormClassifier.IsEntrypoint(source.Base.Form);

    private static RemoveNavigationSourceText? ReadSource(CliWorkspace workspace, SourceLogicalSource source)
    {
        try
        {
            var bytes = File.ReadAllBytes(source.Base.PhysicalPath);
            var logicalPath = Path.Combine(
                workspace.LexicalRoot,
                source.Identity.CanonicalBasePath.Replace('/', Path.DirectorySeparatorChar));
            return new RemoveNavigationSourceText(
                StrictUtf8.GetString(bytes),
                FileStateSnapshot.File(logicalPath, source.Base.PhysicalPath, bytes));
        }
        catch (Exception exception) when (exception is DecoderFallbackException
            or IOException or UnauthorizedAccessException)
        {
            return null;
        }
    }
}
