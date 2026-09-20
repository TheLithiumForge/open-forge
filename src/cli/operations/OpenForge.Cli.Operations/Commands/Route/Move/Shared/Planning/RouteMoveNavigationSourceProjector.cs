using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Move.Shared.References;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.Planning;

internal sealed class RouteMoveNavigationSourceProjector
{
    internal ImmutableArray<SourceLogicalSource> ProjectIntended(
        RouteMoveResolvedDestination destination)
    {
        ArgumentNullException.ThrowIfNull(destination);
        var movedPaths = RouteMoveReferenceChangeProjector.BuildMovedPathMap(destination);
        return ProjectCurrent(destination.Inventory.Subject)
            .Select(source => ProjectSource(destination, source, movedPaths))
            .ToImmutableArray();
    }

    internal ImmutableArray<SourceLogicalSource> ProjectCurrent(
        RouteMoveResolvedSubject subject)
    {
        ArgumentNullException.ThrowIfNull(subject);
        return subject.Catalogue.Sources
            .Where(source => IsEligible(subject, source))
            .ToImmutableArray();
    }

    private static bool IsEligible(
        RouteMoveResolvedSubject subject,
        SourceLogicalSource source)
    {
        if (source.Base.Form == SourceDocumentForm.Loader
            || SourceFormClassifier.IsEntrypoint(source.Base.Form)
            || subject.NavigationExposure.ExposedPaths.Contains(
                source.Identity.CanonicalBasePath,
                StringComparer.Ordinal))
        {
            return true;
        }

        var node = subject.RouteFacts?.Topology.FindByPath(
            source.Identity.CanonicalBasePath);
        return node?.ParentState == SourceRouteParentState.Resolved
            && subject.NavigationExposure.UnavailableParents.Contains(
                node.ParentPaths[0],
                StringComparer.Ordinal);
    }

    private static SourceLogicalSource ProjectSource(
        RouteMoveResolvedDestination destination,
        SourceLogicalSource source,
        IReadOnlyDictionary<string, string> movedPaths)
    {
        if (!movedPaths.TryGetValue(source.Identity.CanonicalBasePath, out var destinationPath))
        {
            return source;
        }

        var destinationId = SourceIdentity.DeriveId(destinationPath)
            ?? throw new InvalidOperationException(
                "Every resolved Route Move destination source requires one canonical identity.");
        return new SourceLogicalSource(
            new SourceLogicalIdentity(destinationId, destinationPath),
            ProjectLayer(destination, source.Base, destinationPath),
            source.Overwrite is { } overwrite
                ? ProjectLayer(
                    destination,
                    overwrite,
                    movedPaths[overwrite.CanonicalPath])
                : null);
    }

    private static SourceLayer ProjectLayer(
        RouteMoveResolvedDestination destination,
        SourceLayer layer,
        string destinationPath)
    {
        var workspace = destination.Inventory.Subject.Request.Workspace;
        return new SourceLayer(
            destinationPath,
            Path.Combine(
                workspace.PhysicalRoot,
                destinationPath.Replace('/', Path.DirectorySeparatorChar)),
            layer.Form,
            layer.Kind);
    }
}
