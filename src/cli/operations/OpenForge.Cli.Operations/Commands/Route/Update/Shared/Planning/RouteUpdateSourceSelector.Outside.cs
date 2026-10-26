using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Shared.ReferenceMaintenance;
using OpenForge.Cli.Core.Commands.Shared.RouteScaffolding;
using OpenForge.Cli.Core.Commands.Shared.RouteScaffolding.Models.Discovery;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Sources.Routing;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Planning;

internal sealed partial class RouteUpdateSourceSelector
{
    internal static async ValueTask<(ImmutableArray<FileStateSnapshot> Snapshots, RouteUpdateFinding? Finding)> ReadOutsideChainAsync(
        WorkspaceLayoutRead layoutRead, SourceCatalogue catalogue, SourceLogicalSource selected, CancellationToken token)
    {
        if (RouteUpdateOutsideBoundary.Find(layoutRead, selected.Base.CanonicalPath) is not { } boundary)
            return ([], null);
        var policy = ReferenceProtectionReader.Read(layoutRead).Policy;
        if (policy is null)
            return ([], new(RouteUpdateFindingCode.TargetUnsafe, "Outside source protection could not be established.", selected.Base.CanonicalPath));
        var discovery = new RouteScaffoldChainDiscovery().Discover(new()
        {
            Catalogue = catalogue,
            RecognizedRootPaths = [boundary.AnchorPath],
            EligibleSources = [selected],
            Settings = policy.Settings,
        });
        if (discovery.IsBlocked)
            return ([], new(RouteUpdateFindingCode.RouteAmbiguous, "The outside route chain contains competing entrypoints.", selected.Base.CanonicalPath));
        if (!discovery.Gaps.IsEmpty)
            return ([], new(RouteUpdateFindingCode.InvalidTarget, "The outside route chain requires existing entrypoints.", selected.Base.CanonicalPath));

        var topology = new SourceRouteTopologyBuilder().Build(catalogue.Layout, catalogue.Sources, []);
        var current = selected;
        var inputs = new List<SourceLayer>();
        while (true)
        {
            if (SourceFormClassifier.IsEntrypoint(current.Base.Form)
                && SourceLogicalPath.ReadParent(current.Base.CanonicalPath) == boundary.DirectoryPath) break;
            var node = topology.FindByPath(current.Base.CanonicalPath);
            if (node?.ParentState != SourceRouteParentState.Resolved)
                return ([], new(node?.ParentState == SourceRouteParentState.Ambiguous
                    ? RouteUpdateFindingCode.RouteAmbiguous : RouteUpdateFindingCode.InvalidTarget,
                    "The outside source requires one existing parent chain inside its registered folder.", current.Base.CanonicalPath));
            var parentPath = node.ParentPaths[0];
            if (!parentPath.StartsWith(boundary.DirectoryPath + "/", StringComparison.Ordinal)
                || catalogue.FindByPath(parentPath) is not { } parent || parent.Base.CanonicalPath != parentPath)
                return ([], new(RouteUpdateFindingCode.TargetUnsafe, "The outside parent cannot be redirected to another folder.", parentPath));
            current = parent;
            inputs.Add(current.Base);
            if (current.Overwrite is { } overwrite) inputs.Add(overwrite);
        }

        var snapshots = ImmutableArray.CreateBuilder<FileStateSnapshot>();
        var observer = new RouteUpdateLayerObserver(new SourceDocumentSnapshotReader());
        foreach (var layer in inputs.OrderBy(item => item.CanonicalPath, StringComparer.Ordinal))
        {
            var read = await observer.ObserveAsync(new()
            {
                Workspace = catalogue.Workspace,
                Layer = layer,
                LayoutRead = layoutRead,
            }, token).ConfigureAwait(false);
            if (read.Layer is not { } observed)
                return ([], new(read.FailureCode ?? RouteUpdateFindingCode.InspectionIncomplete,
                    read.FailureCause ?? "An outside route input could not be read.", layer.CanonicalPath));
            snapshots.Add(observed.Snapshot);
        }
        return (snapshots.ToImmutable(), null);
    }

    private static bool IsIncompleteOutside(RouteUpdateFindingCode code)
        => code is RouteUpdateFindingCode.InspectionIncomplete or RouteUpdateFindingCode.Interrupted;
}
