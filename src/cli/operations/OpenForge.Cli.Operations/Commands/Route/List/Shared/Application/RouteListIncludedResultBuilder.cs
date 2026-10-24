using OpenForge.Cli.Core.Commands.Route.List.Models;
using OpenForge.Cli.Core.Commands.Route.List.Models.Result;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Topology;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Application;

internal static class RouteListIncludedResultBuilder
{
    internal static RouteListResult Build(RouteListRequest request, RouteListSourceResolution source, RouteListResult result, CancellationToken cancellationToken)
    {
        if (result.Status is CliSemanticStatus.Interrupted or CliSemanticStatus.Failed or CliSemanticStatus.Blocked)
        {
            return result;
        }

        var included = source.IncludedSources;
        if (included.Count == 0 && source.UnlistedRoots.Count == 0 && source.IncludedRows.Count == 0)
        {
            return result;
        }

        cancellationToken.ThrowIfCancellationRequested();
        var byPath = included.ToDictionary(item => item.Base.CanonicalPath, StringComparer.Ordinal);
        var byParent = included.GroupBy(item => item.IncludedParentPath
                ?? throw new InvalidOperationException("An included source requires its catalogue parent."), StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.Ordinal);
        var rows = result.Rows.ToList();
        var selection = result.Selection;
        var status = result.Status;
        var findings = result.Findings.ToList();
        var next = result.Next;
        var exactPath = request.SourceReference is { } reference ? SourceReferenceParser.Parse(reference).AttemptedPath : null;
        if (exactPath is not null)
        {
            exactPath = source.IncludedRows.FirstOrDefault(row => row.Path == exactPath)?.AliasOf ?? exactPath;
        }

        var explicitInclude = exactPath is null ? null : byPath.GetValueOrDefault(exactPath);
        if (explicitInclude is not null)
        {
            rows.Clear();
            selection = RouteListSelectionFactory.ResolvedIncluded(explicitInclude);
            status = CliSemanticStatus.Complete;
            findings.Clear();
            next = null;
            Add(explicitInclude, 0, ReadAbsoluteDepth(explicitInclude) is null
                ? RouteListSelectionProvenance.DetachedRoot : RouteListSelectionProvenance.ExplicitRoot);
        }
        else if (!selection.IsResolved || status is not (CliSemanticStatus.Complete or CliSemanticStatus.Attention or CliSemanticStatus.Incomplete))
        {
            return result;
        }

        var selectedPaths = rows.Select(row => row.Path).ToHashSet(StringComparer.Ordinal);
        if (request.SourceReference is null)
        {
            selectedPaths.Add(source.Inventory.SourceCatalogue.Layout.LoaderPath);
            foreach (var root in byParent.GetValueOrDefault(source.Inventory.SourceCatalogue.Layout.LoaderPath) ?? [])
            {
                Add(root, 0, RouteListSelectionProvenance.LoaderRoot);
            }

            rows.AddRange(source.UnlistedRoots.Select(RouteListRow.Unlisted));
        }

        foreach (var parent in result.Rows.Where(row => row.Kind == RouteListRowKind.Entrypoint))
        {
            var children = byParent.GetValueOrDefault(parent.Path) ?? [];
            if (children.Length == 0)
            {
                continue;
            }

            rows[rows.IndexOf(parent)] = parent.WithDirectChildCount(parent.DirectChildCount + children.Length);
            if (CanDescend(parent.RelativeDepth))
            {
                foreach (var child in children)
                {
                    Add(child, parent.RelativeDepth + 1, RouteListSelectionProvenance.Descendant);
                }
            }
        }

        var pending = new Queue<string>(selectedPaths);
        while (pending.TryDequeue(out var parentPath))
        {
            foreach (var child in byParent.GetValueOrDefault(parentPath) ?? [])
            {
                if (selectedPaths.Add(child.Base.CanonicalPath))
                {
                    pending.Enqueue(child.Base.CanonicalPath);
                }
            }
        }

        var missing = source.IncludedRows.Where(row => row.PhysicalState == PhysicalPathState.Missing && selectedPaths.Contains(row.ParentPath))
            .DistinctBy(row => row.Path, StringComparer.Ordinal).ToArray();
        foreach (var item in missing)
        {
            var parent = rows.FirstOrDefault(row => row.Path == item.ParentPath);
            if (parent is not null)
            {
                rows[rows.IndexOf(parent)] = parent.WithDirectChildCount(null);
            }

            if (parent is not null && CanDescend(parent.RelativeDepth) || item.ParentPath == source.Inventory.SourceCatalogue.Layout.LoaderPath && request.SourceReference is null)
            {
                rows.Add(RouteListRow.RoutedLeaf($"./{item.Path}", item.Path, parent?.Id, parent?.Path,
                    parent is null ? 0 : parent.AbsoluteDepth + 1, parent is null ? 0 : parent.RelativeDepth + 1,
                    item.Entry.Description, item.Entry.Tags,
                    new RouteListProvenance(parent is null ? RouteListSelectionProvenance.LoaderRoot : RouteListSelectionProvenance.Descendant,
                        RouteListSourceProvenance.AuthoredLeaf, false)).AsMissing());
            }

            findings.Add(new RouteListFinding(RouteListFindingCode.ReadUnavailable, CliSemanticStatus.Incomplete, item.Path,
                "The included file is missing. Restore it or remove its catalogue row."));
        }

        if (missing.Length != 0)
        {
            status = CliSemanticStatus.Incomplete;
            next = RouteListResultBuilder.ReadNextAction(status);
        }

        var childrenByParent = rows.Where(row => row.RelativeDepth > 0)
            .GroupBy(row => row.ParentPath ?? throw new InvalidOperationException("A descendant row requires its catalogue parent."), StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.OrderBy(row => row.Path, StringComparer.Ordinal).ToArray(), StringComparer.Ordinal);
        var ordered = new List<RouteListRow>();
        foreach (var root in rows.Where(row => row.RelativeDepth == 0).OrderBy(row => row.Path, StringComparer.Ordinal))
        {
            Walk(root);
        }

        var rootCount = selection.Kind == RouteListSelectionKind.LoaderRoots
            ? result.Coverage.SelectedRootCount + ordered.Count(row => row.RelativeDepth == 0 && !result.Rows.Any(original => original.Path == row.Path))
            : 1;
        var effectiveDepth = RouteListDepth.Finite(ordered.Count == 0 ? 0 : ordered.Max(row => row.RelativeDepth));
        var coverage = status == CliSemanticStatus.Incomplete
            ? RouteListCoverage.Incomplete(request.RequestedDepth, effectiveDepth, rootCount, ordered.Count,
                result.Coverage.Evidence, result.Coverage.UnresolvedBoundaries.Concat(findings
                    .Where(finding => finding.Status == CliSemanticStatus.Incomplete).Select(finding => finding.Subject ?? finding.Cause)))
            : RouteListCoverage.Complete(request.RequestedDepth, effectiveDepth, rootCount, ordered.Count, result.Coverage.Evidence);
        return RouteListResult.Create(status, request.Workspace, selection, coverage, ordered, findings, next, result.CustomLoaderPath);

        bool CanDescend(int depth) => request.RequestedDepth.Kind == RouteListDepthKind.All || depth < request.RequestedDepth.FiniteValue;

        int? ReadAbsoluteDepth(SourceLogicalSource item)
        {
            if (item.IncludedParentPath == source.Inventory.SourceCatalogue.Layout.LoaderPath)
            {
                return 0;
            }

            if (item.IncludedParentPath is not { } parentPath)
            {
                return null;
            }

            if (byPath.TryGetValue(parentPath, out var parent))
            {
                return ReadAbsoluteDepth(parent) + 1;
            }

            return source.RouteFacts.Topology.FindByPath(parentPath) is not null
                ? source.RouteFacts.Topology.ReadAbsoluteDepth(parentPath) + 1 : null;
        }

        void Add(SourceLogicalSource item, int depth, RouteListSelectionProvenance provenance)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = item.Base.CanonicalPath;
            var children = byParent.GetValueOrDefault(path) ?? [];
            var parentPath = item.IncludedParentPath == source.Inventory.SourceCatalogue.Layout.LoaderPath ? null : item.IncludedParentPath;
            var parent = parentPath is null ? null : byPath.GetValueOrDefault(parentPath) ?? source.Inventory.SourceCatalogue.FindByPath(parentPath);
            var entry = item.IncludedEntry ?? throw new InvalidOperationException("An included source requires its catalogue row.");
            var rowProvenance = new RouteListProvenance(provenance,
                item.IsIncludedCatalogue ? RouteListSourceProvenance.AuthoredEntrypoint : RouteListSourceProvenance.AuthoredLeaf, false);
            var row = item.IsIncludedCatalogue
                ? RouteListRow.Entrypoint(item.Identity.AutomaticId, path, parent?.Identity.AutomaticId, parentPath, ReadAbsoluteDepth(item), depth,
                    entry.Description, entry.Tags, children.Length, rowProvenance)
                : RouteListRow.RoutedLeaf(item.Identity.AutomaticId, path, parent?.Identity.AutomaticId, parentPath, ReadAbsoluteDepth(item), depth,
                    entry.Description, entry.Tags, rowProvenance);
            rows.Add(row);
            if (CanDescend(depth))
            {
                foreach (var child in children)
                {
                    Add(child, depth + 1, RouteListSelectionProvenance.Descendant);
                }
            }
        }

        void Walk(RouteListRow row)
        {
            ordered.Add(row);
            if (row.IsUnlisted)
            {
                return;
            }

            foreach (var child in childrenByParent.GetValueOrDefault(row.Path) ?? [])
            {
                Walk(child);
            }
        }
    }
}
