using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.Planning;

internal static class RouteMoveDestinationProjector
{
    internal static RouteMoveDestinationProjectionResult Project(
        RouteMoveCategoryInventory inventory)
    {
        var subject = inventory.Subject;
        var operation = subject.Request;
        var parsed = SourceReferenceParser.Parse(operation.DestinationTarget);
        if (parsed.State != SourceReferenceParseState.Valid)
        {
            return Stop(
                inventory,
                RouteMoveFindingCode.InvalidDestination,
                CliSemanticStatus.Invalid,
                operation.DestinationTarget,
                parsed.Cause ?? "The Route Move destination reference is invalid.");
        }

        string? destinationPath;
        if (parsed.Kind == SourceReferenceKind.SourcePath)
        {
            destinationPath = parsed.AttemptedPath;
        }
        else if (parsed.AttemptedId is { } destinationId)
        {
            destinationPath = LogicalDestinationPath(subject, destinationId);
        }
        else
        {
            destinationPath = null;
        }
        if (destinationPath is null)
        {
            return Stop(
                inventory,
                RouteMoveFindingCode.InvalidDestination,
                CliSemanticStatus.Invalid,
                operation.DestinationTarget,
                "The logical destination cannot preserve the selected source form.");
        }

        if (!RouteMoveMaintenanceBoundaryInspector.SameBoundary(subject.Catalogue.Layout,
                subject.SelectedSource.Base.CanonicalPath, destinationPath)
            || !HasCompatibleForm(subject, destinationPath))
        {
            return Stop(
                inventory,
                RouteMoveFindingCode.InvalidDestination,
                CliSemanticStatus.Invalid,
                operation.DestinationTarget,
                "The destination shape does not preserve the selected subject form.");
        }

        return ProjectResolvedPath(inventory, destinationPath);
    }

    private static string? LogicalDestinationPath(
        RouteMoveResolvedSubject subject,
        string destinationId)
    {
        if (subject.Kind == RouteMoveSubjectKind.Leaf)
        {
            return subject.Catalogue.Layout.SourcePath(destinationId + SourceFormClassifier.MarkdownExtension);
        }

        var fileName = subject.SelectedSource.Base.Form switch
        {
            SourceDocumentForm.CanonicalEntrypoint =>
                SourceFormClassifier.CanonicalEntrypointFileName(destinationId[(destinationId.LastIndexOf('/') + 1)..]),
            SourceDocumentForm.IndexEntrypoint => SourceFormClassifier.IndexFileName,
            SourceDocumentForm.UnderscoreIndexEntrypoint => SourceFormClassifier.UnderscoreIndexFileName,
            SourceDocumentForm.ReferencesEntrypoint => SourceFormClassifier.ReferencesFileName,
            SourceDocumentForm.UnderscoreReferencesEntrypoint => SourceFormClassifier.UnderscoreReferencesFileName,
            _ => null,
        };
        if (fileName is null)
        {
            return null;
        }

        var destinationPath = subject.Catalogue.Layout.SourcePath($"{destinationId}/{fileName}");
        return SourceLogicalPath.IsCanonicalSource(destinationPath)
            ? destinationPath
            : null;
    }

    private static RouteMoveDestinationProjectionResult ProjectResolvedPath(
        RouteMoveCategoryInventory inventory,
        string destinationPath)
    {
        var subject = inventory.Subject;
        var destinationId = SourceIdentity.DeriveId(subject.Catalogue.Layout, destinationPath);
        if (destinationId is null)
        {
            return Stop(
                inventory,
                RouteMoveFindingCode.InvalidDestination,
                CliSemanticStatus.Invalid,
                destinationPath,
                "The destination does not derive one canonical route identity.");
        }

        var parent = FindParent(subject, destinationPath);
        if (parent is null)
        {
            return Stop(
                inventory,
                RouteMoveFindingCode.DestinationParentMissing,
                CliSemanticStatus.Blocked,
                destinationPath,
                "The destination requires one existing routed parent.");
        }

        var policyBoundary = ReadPolicyBoundary(inventory, destinationPath);
        if (policyBoundary is not null)
        {
            return policyBoundary;
        }

        return new RouteMoveDestinationProjectionResult(
            new RouteMoveDestinationProjection
            {
                Inventory = inventory,
                DestinationPath = destinationPath,
                DestinationId = destinationId,
                Parent = parent,
                Items = ProjectItems(inventory, destinationPath),
            },
            boundary: null);
    }

    private static RouteMoveDestinationProjectionResult? ReadPolicyBoundary(
        RouteMoveCategoryInventory inventory,
        string destinationPath)
    {
        var subject = inventory.Subject;
        if (string.Equals(
            destinationPath,
            subject.SelectedSource.Identity.CanonicalBasePath,
            StringComparison.Ordinal))
        {
            return Stop(
                inventory,
                RouteMoveFindingCode.SelfMove,
                CliSemanticStatus.Invalid,
                destinationPath,
                "The destination is the selected source path.");
        }

        if (subject.Kind != RouteMoveSubjectKind.Category)
        {
            return null;
        }

        var sourceRoot = SourceLogicalPath.ReadParent(
            subject.SelectedSource.Identity.CanonicalBasePath);
        if (!destinationPath.StartsWith($"{sourceRoot}/", StringComparison.Ordinal))
        {
            return null;
        }

        return Stop(
            inventory,
            RouteMoveFindingCode.DestinationInsideSource,
            CliSemanticStatus.Invalid,
            destinationPath,
            "A category destination cannot be inside its selected source tree.");
    }

    private static bool HasCompatibleForm(
        RouteMoveResolvedSubject subject,
        string path)
    {
        if (!SourceFormClassifier.TryClassify(subject.Catalogue.Layout, path, out var form))
        {
            return false;
        }

        if (subject.Kind == RouteMoveSubjectKind.Leaf)
        {
            return form == SourceDocumentForm.Markdown;
        }

        return form == subject.SelectedSource.Base.Form;
    }

    private static SourceLogicalSource? FindParent(
        RouteMoveResolvedSubject subject,
        string destinationPath)
    {
        var destinationDirectory = SourceLogicalPath.ReadParent(destinationPath);
        var parentDirectory = subject.Kind == RouteMoveSubjectKind.Category
            ? SourceLogicalPath.ReadParent(destinationDirectory)
            : destinationDirectory;
        var candidates = subject.Catalogue.Sources.Where(source =>
                SourceFormClassifier.IsEntrypoint(source.Base.Form)
                && string.Equals(
                    SourceLogicalPath.ReadParent(source.Identity.CanonicalBasePath),
                    parentDirectory,
                    StringComparison.Ordinal))
            .ToArray();
        if (candidates.Length != 1)
        {
            return null;
        }

        var route = subject.RouteFacts?.RouteFacts.FirstOrDefault(fact => string.Equals(
            fact.Identity.CanonicalBasePath,
            candidates[0].Identity.CanonicalBasePath,
            StringComparison.Ordinal));
        if (RouteMoveMaintenanceBoundaryInspector.IsOutside(subject.Catalogue, destinationPath))
            return subject.RouteFacts is { } facts
                && RouteMoveMaintenanceBoundaryInspector.ReadChainProblem(subject.LayoutRead, subject.Catalogue,
                    facts.Topology, candidates[0].Base.CanonicalPath, allowAnchor: true) is null ? candidates[0] : null;
        return route?.State == SourceRouteState.Routed ? candidates[0] : null;
    }

    private static ImmutableArray<RouteMoveDestinationItem> ProjectItems(
        RouteMoveCategoryInventory inventory,
        string destinationPath)
    {
        var subject = inventory.Subject;
        if (subject.Kind == RouteMoveSubjectKind.Leaf)
        {
            return subject.Layers.Select(layer => new RouteMoveDestinationItem
            {
                Item = new RouteMoveInventoryItem
                {
                    Kind = RouteMoveItemKind.RoutedMarkdown,
                    Layer = ReadLayer(layer.Layer.Kind),
                    SourceId = subject.SelectedSource.Identity.AutomaticId,
                    SourcePath = layer.Snapshot.LogicalPath,
                    RelativePath = Path.GetFileName(layer.Snapshot.LogicalPath),
                    Snapshot = layer.Snapshot,
                },
                DestinationPath = DestinationLayerPath(destinationPath, layer.Layer.Kind),
            }).ToImmutableArray();
        }

        var destinationRoot = SourceLogicalPath.ReadParent(destinationPath);
        var rootLayers = subject.Layers.ToDictionary(layer => layer.Snapshot.LogicalPath,
            layer => DestinationLayerPath(destinationPath, layer.Layer.Kind), StringComparer.Ordinal);
        return inventory.Items.Select(item => new RouteMoveDestinationItem
        {
            Item = item,
            DestinationPath = CategoryDestinationPath(item, destinationRoot, rootLayers),
        }).ToImmutableArray();
    }

    private static string CategoryDestinationPath(
        RouteMoveInventoryItem item, string destinationRoot, IReadOnlyDictionary<string, string> rootLayers)
    {
        if (rootLayers.TryGetValue(item.SourcePath, out var rootLayer)) return rootLayer;
        if (item.RelativePath == ".") return destinationRoot;
        return $"{destinationRoot}/{item.RelativePath}";
    }

    internal static string DestinationLayerPath(
        string destinationPath,
        SourceLayerKind kind)
        => kind == SourceLayerKind.Base
            ? destinationPath
            : SourceOverwritePath.ReadAdjacentPath(destinationPath);

    private static RouteMoveLayerKind ReadLayer(SourceLayerKind kind)
        => kind switch
        {
            SourceLayerKind.Base => RouteMoveLayerKind.Base,
            SourceLayerKind.Overwrite => RouteMoveLayerKind.Overwrite,
            _ => throw new ArgumentOutOfRangeException(
                nameof(kind),
                kind,
                "The source layer kind is not defined."),
        };

    internal static RouteMoveDestinationProjectionResult Stop(
        RouteMoveCategoryInventory inventory,
        RouteMoveFindingCode code,
        CliSemanticStatus status,
        string? target,
        string cause)
    {
        var subject = inventory.Subject;
        var layout = subject.Catalogue.Layout;
        var formation = RouteMoveBoundary.Start(subject.Request) with
        {
            Source = subject.Source,
            Destination = new RouteMoveDestination
            {
                Requested = subject.Request.DestinationTarget,
                CustomMainRoot = layout.IsDefault ? null : layout.MainRoot,
                SuggestedParentReference = code == RouteMoveFindingCode.DestinationParentMissing ? ParentReference(subject) : null,
            },
            Subject = new RouteMoveSubject { Kind = subject.Kind },
            Ownership = RouteMoveOwnershipProjector.Project(
                inventory.Ownership,
                subject),
        };
        return new RouteMoveDestinationProjectionResult(
            projection: null,
            RouteMoveBoundary.Stop(formation, code, status, target, cause));
    }

    private static string ParentReference(RouteMoveResolvedSubject subject)
    {
        var requested = subject.Request.DestinationTarget.Replace('\\', '/');
        var slash = requested.LastIndexOf('/');
        var parent = slash > 0 ? requested[..slash] : requested;
        return subject.Catalogue.Layout.TryGetSourceRelativePath(parent, out var relative)
            ? relative
            : parent;
    }
}
