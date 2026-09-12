using System.Text;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Remove.Shared.Planning;

internal sealed class ExtensionRemoveTopologyBuilder
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);
    private readonly SourceCatalogueReader _catalogueReader = new();
    private readonly GeneratedNavigationFormationBuilder _formationBuilder = new();
    private readonly MarkdownDocumentParser _markdownParser = new();
    private readonly SourceAuthoredMetadataParser _metadataParser = new();
    private readonly GeneratedNavigationProjector _projector = new();

    internal async ValueTask<ExtensionRemoveTopologyBuild> BuildAsync(
        CliWorkspace workspace,
        IReadOnlySet<string> removedPaths,
        CancellationToken cancellationToken)
    {
        var catalogue = await _catalogueReader.ReadAsync(
            new SourceCatalogueRequest(workspace, [SourceLogicalPath.AgentsRoot]),
            cancellationToken).ConfigureAwait(false);
        if (catalogue.IsCancelled)
        {
            throw new OperationCanceledException(cancellationToken);
        }

        if (catalogue.Issues.Count > 0)
        {
            throw new InvalidDataException(
                catalogue.Issues[0].Failure?.DirectCause
                    ?? "Current authored source catalogue facts are unsafe or unavailable.");
        }

        if (removedPaths.Count == 0)
        {
            return Unchanged(catalogue);
        }

        var observedFormation = _formationBuilder.Build(catalogue);
        var retainedChild = removedPaths
            .Select(observedFormation.Topology.FindByPath)
            .Where(node => node is not null)
            .Cast<SourceRouteNode>()
            .SelectMany(node => node.ChildPaths)
            .Where(path => !removedPaths.Contains(path))
            .Order(StringComparer.Ordinal)
            .FirstOrDefault();
        if (retainedChild is not null)
        {
            throw new ExtensionRemoveTopologyBlockedException(
                $"A retained routed descendant depends on the selected route host: {retainedChild}");
        }

        var intendedSources = catalogue.Sources
            .Where(source => !removedPaths.Contains(source.Identity.CanonicalBasePath))
            .OrderBy(source => source.Identity.CanonicalBasePath, StringComparer.Ordinal)
            .ToArray();
        var formation = _formationBuilder.Build(catalogue, intendedSources);
        if (formation.Ambiguities.Count > 0 || formation.IntendedTargetCollisions.Count > 0)
        {
            throw new InvalidDataException(
                "The intended Extension removal topology contains an ambiguous route, alias, or target collision.");
        }

        var documents = new Dictionary<string, string>(StringComparer.Ordinal);
        var snapshots = new Dictionary<string, FileStateSnapshot>(StringComparer.Ordinal);
        foreach (var source in intendedSources)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var bytes = await File.ReadAllBytesAsync(source.Base.PhysicalPath, cancellationToken)
                .ConfigureAwait(false);
            documents.Add(source.Identity.CanonicalBasePath, StrictUtf8.GetString(bytes));
            snapshots.Add(
                source.Identity.CanonicalBasePath,
                FileStateSnapshot.File(
                    Path.GetFullPath(Path.Combine(
                        workspace.LexicalRoot,
                        source.Identity.CanonicalBasePath.Replace('/', Path.DirectorySeparatorChar))),
                    source.Base.PhysicalPath,
                    bytes));
        }

        var metadata = intendedSources
            .Where(source => source.Base.Form != SourceDocumentForm.Loader)
            .Select(source => new GeneratedNavigationMetadata(
                source,
                _metadataParser.Parse(
                    _markdownParser.Parse(documents[source.Identity.CanonicalBasePath]),
                    source.Base.Form)))
            .ToArray();
        var regions = intendedSources
            .Where(source => source.Base.Form == SourceDocumentForm.Loader
                || SourceFormClassifier.IsEntrypoint(source.Base.Form))
            .Select(source => new
            {
                Source = source,
                Document = _markdownParser.Parse(documents[source.Identity.CanonicalBasePath]),
            })
            .Select(value => new GeneratedNavigationRegionInput(value.Source, value.Document))
            .ToArray();
        var projection = _projector.Project(new GeneratedNavigationProjectionRequest(
            formation,
            regions,
            metadata));
        var unavailable = projection.Regions.FirstOrDefault(region =>
            region.State != GeneratedNavigationRegionState.Available);
        if (unavailable is not null)
        {
            throw new ExtensionRemoveTopologyBlockedException(
                unavailable.Cause ?? "The intended generated navigation projection is unavailable.");
        }

        var changes = new List<ExtensionRemoveGeneratedChange>();
        var resultRegions = new List<ExtensionRemoveGeneratedRegion>();
        var intendedBytes = new Dictionary<string, byte[]>(StringComparer.Ordinal);
        var generatedEntries = new Dictionary<string, IReadOnlyList<GeneratedNavigationEntry>>(
            StringComparer.Ordinal);
        foreach (var region in projection.Regions.OrderBy(
                     value => value.CanonicalPath,
                     StringComparer.Ordinal))
        {
            var change = region.Change
                ?? throw new InvalidDataException(
                    "An available generated region requires its bounded change.");
            var changed = !change.IsUnchanged;
            resultRegions.Add(new ExtensionRemoveGeneratedRegion(
                region.CanonicalPath,
                changed
                    ? ExtensionRemoveGeneratedRegionState.Changed
                    : ExtensionRemoveGeneratedRegionState.Unchanged));
            intendedBytes.Add(region.CanonicalPath, [.. change.ExpectedDocumentBytes]);
            generatedEntries.Add(region.CanonicalPath, region.Entries);
            if (!changed)
            {
                continue;
            }

            var before = snapshots[region.CanonicalPath];
            changes.Add(new ExtensionRemoveGeneratedChange(
                region.CanonicalPath,
                PlannedFileChange.ReplaceGeneratedRegion(
                    before.Expectation,
                    change.ExpectedDocumentBytes.AsSpan()),
                before));
        }

        return new ExtensionRemoveTopologyBuild(
            new ExtensionRemoveTopology
            {
                IntendedTargetBytes = intendedBytes,
                GeneratedEntries = generatedEntries,
                ProtectedPaths = catalogue.Candidates
                    .Select(candidate => candidate.CanonicalPath)
                    .ToHashSet(StringComparer.Ordinal),
            },
            resultRegions,
            changes);
    }

    private static ExtensionRemoveTopologyBuild Unchanged(SourceCatalogue catalogue)
        => new(
            new ExtensionRemoveTopology
            {
                IntendedTargetBytes = new Dictionary<string, byte[]>(StringComparer.Ordinal),
                GeneratedEntries = new Dictionary<string, IReadOnlyList<GeneratedNavigationEntry>>(StringComparer.Ordinal),
                ProtectedPaths = catalogue.Candidates
                    .Select(candidate => candidate.CanonicalPath)
                    .ToHashSet(StringComparer.Ordinal),
            },
            [],
            []);
}

internal sealed class ExtensionRemoveTopologyBlockedException(string message)
    : Exception(message);
