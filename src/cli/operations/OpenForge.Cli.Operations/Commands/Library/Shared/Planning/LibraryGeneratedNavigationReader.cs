using OpenForge.Cli.Core.Commands.Library.Shared.Planning.Models;
using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Commands.Library.Models.Planning;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries;
using OpenForge.Cli.Core.Framework.Libraries.Models.Inventory;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Commands.Library.Shared.Planning;

internal static class LibraryGeneratedNavigationReader
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    internal static ValueTask<LibraryGeneratedNavigationRead> ReadAsync(
        LibraryGeneratedNavigationRequest request,
        CancellationToken cancellationToken)
        => ReadCoreAsync(request, cancellationToken);

    private static async ValueTask<LibraryGeneratedNavigationRead> ReadCoreAsync(
        LibraryGeneratedNavigationRequest request,
        CancellationToken cancellationToken)
    {
        var workspace = request.Workspace;
        var selectedId = request.SelectedLibrary.Id;
        var currentRecord = request.CurrentRecord;
        var intendedSelectedEntries = request.IntendedEntries;
        try
        {
            var catalogue = await new SourceCatalogueReader().ReadAsync(
                new SourceCatalogueRequest(workspace, [SourceLogicalPath.AgentsRoot]),
                cancellationToken).ConfigureAwait(false);
            if (catalogue.IsCancelled)
            {
                throw new OperationCanceledException(cancellationToken);
            }

            var selected = currentRecord?.Libraries.FirstOrDefault(library => library.Id == selectedId);
            var selectedPaths = selected is null ? [] : LibraryPathIdentity.Mappings(selected)
                .Select(mapping => mapping.DestinationPath.Value).ToHashSet(StringComparer.Ordinal);
            var intendedCatalogue = selectedPaths.Count == 0
                ? catalogue
                : new SourceCatalogue(
                    workspace,
                    catalogue.Candidates.Where(candidate => !selectedPaths.Contains(candidate.CanonicalPath)),
                    catalogue.Sources.Where(source => !selectedPaths.Contains(source.Identity.CanonicalBasePath)),
                    catalogue.Issues.Where(issue => !selectedPaths.Contains(issue.AttemptedCanonicalPath)
                        && !issue.RelatedPaths.Any(selectedPaths.Contains)),
                    isCancelled: false);
            var intendedByPath = intendedCatalogue.Sources
                .Where(source => !selectedPaths.Contains(source.Identity.CanonicalBasePath))
                .ToDictionary(source => source.Identity.CanonicalBasePath, StringComparer.Ordinal);
            var selectedPhysicalPaths = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var entry in intendedSelectedEntries)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var path = LibraryPathIdentity.Map(request.SelectedLibrary.SourceRoot,
                    request.SelectedLibrary.DestinationRoot, entry.SourcePath).DestinationPath.Value;
                if (TryCreateSelectedSource(path, entry.PhysicalPath) is not { } source)
                {
                    continue;
                }

                intendedByPath[path] = source;
                selectedPhysicalPaths[path] = entry.PhysicalPath;
            }

            var observedFormation = new GeneratedNavigationFormationBuilder().Build(catalogue);
            var intendedFormation = new GeneratedNavigationFormationBuilder().Build(
                intendedCatalogue,
                [.. intendedByPath.Values]);
            if (intendedFormation.Ambiguities.Count > 0
                || intendedFormation.IntendedTargetCollisions.Count > 0)
            {
                return Blocked(
                    intendedFormation.Ambiguities.Count > 0
                        ? intendedFormation.Ambiguities[0].Subject
                        : intendedFormation.IntendedTargetCollisions[0].TargetPath,
                    "The intended Library projection has an unsafe or ambiguous generated-navigation topology.");
            }

            var affectedPaths = ReadAffectedRegionPaths(observedFormation, intendedFormation);
            if (affectedPaths.Count == 0)
            {
                return Complete([]);
            }

            var parser = new MarkdownDocumentParser();
            var documents = new Dictionary<string, LibraryNavigationDocument>(StringComparer.Ordinal);
            foreach (var source in intendedFormation.Sources)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var physicalPath = selectedPhysicalPaths.GetValueOrDefault(
                    source.Identity.CanonicalBasePath,
                    source.Base.PhysicalPath);
                var bytes = await File.ReadAllBytesAsync(physicalPath, cancellationToken).ConfigureAwait(false);
                documents.Add(
                    source.Identity.CanonicalBasePath,
                    new LibraryNavigationDocument(bytes, parser.Parse(StrictUtf8.GetString(bytes))));
            }

            var regions = new List<GeneratedNavigationRegionInput>();
            foreach (var path in affectedPaths)
            {
                if (intendedFormation.FindSource(path) is not { } source
                    || !documents.TryGetValue(path, out var document))
                {
                    return Incomplete(path, "An affected generated-navigation source is unavailable.");
                }

                regions.Add(new GeneratedNavigationRegionInput(source, document.Document));
            }

            var metadataParser = new SourceAuthoredMetadataParser();
            var metadata = intendedFormation.Sources
                .Where(source => source.Base.Form != SourceDocumentForm.Loader)
                .Select(source => new GeneratedNavigationMetadata(
                    source,
                    metadataParser.Parse(
                        documents[source.Identity.CanonicalBasePath].Document,
                        source.Base.Form)))
                .ToArray();
            var projection = new GeneratedNavigationProjector().Project(
                new GeneratedNavigationProjectionRequest(intendedFormation, regions, metadata));
            var changes = ImmutableArray.CreateBuilder<PlannedFileChange>();
            foreach (var region in projection.Regions.OrderBy(value => value.CanonicalPath, StringComparer.Ordinal))
            {
                if (region.State != GeneratedNavigationRegionState.Available
                    || region.Change is not { } change)
                {
                    return Blocked(
                        region.CanonicalPath,
                        region.Cause ?? "An affected generated-navigation region is unsafe or unavailable.");
                }

                if (!change.RequiresUpdate)
                {
                    continue;
                }

                var document = documents[region.CanonicalPath];
                var logicalPath = SourceLogicalPath.ToLexicalPath(workspace.LexicalRoot, region.CanonicalPath);
                changes.Add(PlannedFileChange.ReplaceGeneratedRegion(
                    FileStateSnapshot.File(logicalPath, region.PhysicalPath, document.Bytes).Expectation,
                    change.ExpectedDocumentBytes.AsSpan()));
            }

            return Complete(changes.ToImmutable());
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception) when (exception is ArgumentException
            or DecoderFallbackException
            or InvalidDataException
            or IOException
            or UnauthorizedAccessException)
        {
            return Incomplete(path: null, exception.Message);
        }
    }

    private static SourceLogicalSource? TryCreateSelectedSource(string path, string physicalPath)
    {
        if (!SourceFormClassifier.TryClassify(path, out var form)
            || form == SourceDocumentForm.OverwriteCompanion)
        {
            return null;
        }

        var id = SourceIdentity.DeriveId(path);
        if (id is null)
        {
            return null;
        }

        return new SourceLogicalSource(
            new SourceLogicalIdentity(id, path),
            new SourceLayer(path, physicalPath, form, SourceLayerKind.Base));
    }

    private static IReadOnlyList<string> ReadAffectedRegionPaths(
        GeneratedNavigationFormation before,
        GeneratedNavigationFormation after)
    {
        var candidates = before.Sources.Concat(after.Sources)
            .Where(source => source.Base.Form == SourceDocumentForm.Loader
                || SourceFormClassifier.IsEntrypoint(source.Base.Form))
            .Select(source => source.Identity.CanonicalBasePath)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal);
        return [.. candidates.Where(path => !ReadChildren(before, path).SequenceEqual(
                ReadChildren(after, path),
                StringComparer.Ordinal))];
    }

    private static IReadOnlyList<string> ReadChildren(
        GeneratedNavigationFormation formation,
        string path)
    {
        if (string.Equals(path, SourceLogicalPath.LoaderPath, StringComparison.Ordinal))
        {
            return formation.Loader is null ? [] : formation.Topology.LoaderRootPaths;
        }

        return formation.Topology.FindByPath(path)?.ChildPaths ?? [];
    }

    private static LibraryGeneratedNavigationRead Complete(ImmutableArray<PlannedFileChange> changes)
        => new(changes, Issue: null);

    private static LibraryGeneratedNavigationRead Blocked(string? path, string cause)
        => new([], new LibraryGeneratedNavigationIssue(
            LibraryGeneratedNavigationIssueState.Blocked,
            path,
            cause));

    private static LibraryGeneratedNavigationRead Incomplete(string? path, string cause)
        => new([], new LibraryGeneratedNavigationIssue(
            LibraryGeneratedNavigationIssueState.Incomplete,
            path,
            cause));

}
