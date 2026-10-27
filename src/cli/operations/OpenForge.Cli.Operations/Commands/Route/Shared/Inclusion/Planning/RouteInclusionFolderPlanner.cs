using OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Models;
using OpenForge.Cli.Core.Commands.Shared.ReferenceMaintenance;
using OpenForge.Cli.Core.Commands.Shared.RouteScaffolding;
using OpenForge.Cli.Core.Commands.Shared.RouteScaffolding.Models.Composition;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata.Models;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Completion;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Planning;

internal static class RouteInclusionFolderPlanner
{
    internal static RouteScaffoldDocument Document(RouteInclusionTarget target, WorkspaceLayoutRead layoutRead)
        => RouteScaffoldComposer.FolderDocument(
            SourceWorkspaceRelativePath.LiteralReferencePrefix + target.Path,
            FrameworkMetadataDescriptionDeriver.SentenceCase(SourceLogicalPath.ReadFileName(target.Path)),
            RoutedMetadataDefaults.Tags(RoutedMetadataCategory.Workspace), layoutRead.Settings.Document.Frontmatter);

    internal static PlannedFileChange? PlanCatalogue(RouteInclusionTarget target, WorkspaceLayoutRead layoutRead, WorkspaceLayout intendedLayout)
    {
        var snapshot = target.DirectoryRead?.CatalogueSnapshot
            ?? throw new IOException("Folder inclusion requires its initial canonical catalogue observation.");
        RequireUnambiguousEntrypoint(target);
        if (snapshot.Kind != FileExpectationKind.Missing) return null;
        var protection = ReferenceProtectionReader.Read(layoutRead);
        if (protection.Policy is not { } policy)
            throw new IOException("Catalogue creation requires safely observed ownership and settings.");
        var path = target.LoaderPath;
        if (new RoutedMetadataCompletionPolicy().Protection(path, policy with { Layout = intendedLayout }) != RoutedMetadataEligibility.Eligible
            || policy.Ownership.Framework?.Regions.Any(region => region.Path == path) == true
            || policy.Ownership.Extensions.Any(owner => owner.Regions.Any(region => region.Path == path)))
            throw new IOException("The missing catalogue is protected. Restore it through its owning operation before including this folder.");
        var bytes = new RouteScaffoldComposer().Compose(Document(target, layoutRead));
        return PlannedFileChange.Create(snapshot.Expectation, bytes.AsSpan());
    }

    private static void RequireUnambiguousEntrypoint(RouteInclusionTarget target)
    {
        var directory = target.DirectoryRead?.PhysicalDirectory
            ?? throw new IOException("Folder inclusion requires its observed physical directory.");
        var competing = Directory.EnumerateFiles(directory)
            .Where(path => Path.GetFileName(path) != SourceLogicalPath.ReadFileName(target.LoaderPath))
            .Where(path => SourceFormClassifier.IsCompatibilityFileName(Path.GetFileName(path)))
            .Order(StringComparer.Ordinal).FirstOrDefault();
        if (competing is not null)
            throw new IOException($"The folder has a competing catalogue '{Path.GetFileName(competing)}'. Resolve its entrypoints before including it with the canonical catalogue '{target.LoaderPath}'.");
    }
}
