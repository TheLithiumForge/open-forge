using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Reading;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;

internal sealed class RouteListInventoryReader
{
    internal async ValueTask<RouteListInventoryFacts> ReadAsync(
        RouteListInventoryRequest request,
        SourceDocumentReader documentReader)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(documentReader);
        ValidateWorkspace(request, documentReader);

        var sourceCatalogue = await new SourceCatalogueReader()
            .ReadAsync(
                new SourceCatalogueRequest(request.Workspace, request.LogicalRoots),
                request.CancellationToken)
            .ConfigureAwait(false);
        var selection = sourceCatalogue.SelectAll();
        var projectionBuildResult = await new RouteListSourceProjectionBuilder()
            .ReadAsync(selection, documentReader, request.CancellationToken)
            .ConfigureAwait(false);
        var sources = projectionBuildResult.ProjectionSet.Sources
            .Select(source => new RouteListInventorySource(source))
            .ToArray();
        var findings = FormFindings(sourceCatalogue, selection, projectionBuildResult, sources);
        var aliases = FormAliases(sourceCatalogue, selection);

        if (sourceCatalogue.IsCancelled
            || projectionBuildResult.IsCancelled
            || request.CancellationToken.IsCancellationRequested)
        {
            return RouteListInventoryFacts.Interrupted(
                sourceCatalogue,
                projectionBuildResult,
                sources,
                findings,
                aliases,
                RouteListFilesystemFindingPolicy.Interrupted(
                    ReadInterruptionSubject(projectionBuildResult)));
        }

        return RouteListInventoryFacts.Create(
            sourceCatalogue,
            projectionBuildResult,
            sources,
            findings,
            aliases);
    }

    private static IReadOnlyList<RouteListFilesystemFinding> FormFindings(
        SourceCatalogue catalogue,
        SourceCatalogueSelection selection,
        RouteSourceProjectionBuildResult projectionBuildResult,
        IReadOnlyList<RouteListInventorySource> sources)
    {
        var findings = new List<RouteListFilesystemFinding>();
        foreach (var issue in selection.RootIssues.Concat(selection.Issues))
        {
            var finding = RouteListFilesystemFindingPolicy.FromCatalogueIssue(
                issue,
                catalogue.FindCandidateByPath(issue.AttemptedCanonicalPath));
            if (finding is not null)
            {
                findings.Add(finding);
            }
        }

        foreach (var projection in projectionBuildResult.Projections)
        {
            AddReadFinding(projection.BaseRead, findings);
            if (projection.OverwriteRead is not null)
            {
                AddReadFinding(projection.OverwriteRead, findings);
            }
        }

        foreach (var read in projectionBuildResult.ReadResults)
        {
            AddReadFinding(read, findings);
        }

        foreach (var source in sources)
        {
            RouteListInventoryFindingBuilder.AddMetadataFinding(
                source.Source.CanonicalPath,
                source.Source.Metadata,
                findings);
            if (source.Source.Metadata.IsCompatibilityEntrypoint
                && source.Source.Metadata.State == RouteSourceMetadataState.Complete)
            {
                findings.Add(RouteListFilesystemFindingPolicy.CompatibilityEntrypoint(
                    source.Source.CanonicalPath));
            }
        }

        RouteListInventoryFindingBuilder.AddIdentityCollisionFindings(sources, findings);
        return findings;
    }

    private static void AddReadFinding(
        SourceDocumentReadResult read,
        ICollection<RouteListFilesystemFinding> findings)
    {
        var finding = RouteListFilesystemFindingPolicy.FromDocumentRead(read);
        if (finding is not null)
        {
            findings.Add(finding);
        }
    }

    private static IReadOnlyList<RouteListPhysicalAlias> FormAliases(
        SourceCatalogue catalogue,
        SourceCatalogueSelection selection)
    {
        var aliases = new Dictionary<string, RouteListPhysicalAlias>(StringComparer.Ordinal);
        foreach (var issue in selection.Issues.Where(issue => issue.Code == SourceCatalogueIssueCode.PhysicalAlias))
        {
            var candidate = catalogue.FindCandidateByPath(issue.AttemptedCanonicalPath)
                ?? throw new InvalidOperationException("A physical-alias issue requires its source candidate.");
            var firstPath = issue.RelatedPaths.Single(path =>
                !string.Equals(path, issue.AttemptedCanonicalPath, StringComparison.Ordinal));
            AddAlias(
                aliases,
                new RouteListPhysicalAlias(
                    issue.AttemptedCanonicalPath,
                    candidate.PhysicalPath
                    ?? throw new InvalidOperationException("A physical-alias candidate requires its contained physical path."),
                    firstPath));
            AddContainingDirectoryAlias(catalogue, aliases, candidate, firstPath);
        }

        return aliases.Values.ToArray();
    }

    private static void AddContainingDirectoryAlias(
        SourceCatalogue catalogue,
        IDictionary<string, RouteListPhysicalAlias> aliases,
        SourceCandidate candidate,
        string firstPath)
    {
        var firstCandidate = catalogue.FindCandidateByPath(firstPath);
        if (firstCandidate?.PhysicalParentPath is null
            || candidate.PhysicalParentPath is null
            || !PhysicalIdentityTracker.PathComparer.Equals(
                firstCandidate.PhysicalParentPath,
                candidate.PhysicalParentPath))
        {
            return;
        }

        var canonicalParent = SourceLogicalPath.ReadParent(candidate.CanonicalPath);
        var firstCanonicalParent = SourceLogicalPath.ReadParent(firstCandidate.CanonicalPath);
        if (string.Equals(canonicalParent, firstCanonicalParent, StringComparison.Ordinal))
        {
            return;
        }

        AddAlias(
            aliases,
            new RouteListPhysicalAlias(
                canonicalParent,
                candidate.PhysicalParentPath,
                firstCanonicalParent));
    }

    private static void AddAlias(
        IDictionary<string, RouteListPhysicalAlias> aliases,
        RouteListPhysicalAlias alias)
    {
        if (aliases.TryGetValue(alias.CanonicalLogicalPath, out var existing))
        {
            if (!string.Equals(existing.PhysicalPath, alias.PhysicalPath, StringComparison.Ordinal)
                || !string.Equals(
                    existing.FirstCanonicalLogicalPath,
                    alias.FirstCanonicalLogicalPath,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException("A logical path has conflicting physical-alias facts.");
            }

            return;
        }

        aliases.Add(alias.CanonicalLogicalPath, alias);
    }

    private static string ReadInterruptionSubject(
        RouteSourceProjectionBuildResult projectionBuildResult)
    {
        foreach (var read in projectionBuildResult.Projections
                     .SelectMany(projection => new[] { projection.BaseRead, projection.OverwriteRead })
                     .Where(read => read is not null)
                     .Cast<SourceDocumentReadResult>()
                     .Concat(projectionBuildResult.ReadResults))
        {
            if (read.Verification.State == SourceLayerVerificationState.Cancelled
                || read.Read?.State == FileReadState.Cancelled)
            {
                return read.Layer.CanonicalPath;
            }
        }

        return SourceLogicalPath.AgentsRoot;
    }

    private static void ValidateWorkspace(
        RouteListInventoryRequest request,
        SourceDocumentReader documentReader)
    {
        if (!string.Equals(
                request.Workspace.LexicalRoot,
                documentReader.Workspace.LexicalRoot,
                StringComparison.Ordinal)
            || !PhysicalIdentityTracker.PathComparer.Equals(
                request.Workspace.PhysicalRoot,
                documentReader.Workspace.PhysicalRoot))
        {
            throw new ArgumentException(
                "The route-list inventory request and document reader must share one workspace.",
                nameof(documentReader));
        }
    }

}
