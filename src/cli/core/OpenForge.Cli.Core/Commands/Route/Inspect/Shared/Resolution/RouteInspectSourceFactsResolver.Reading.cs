using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Resolution;

internal sealed partial class RouteInspectSourceFactsResolver
{
    private static void AddReadIssues(
        RouteInspectSourceResolutionInput input,
        ICollection<RouteInspectResolutionIssue> issues)
    {
        var seenPaths = new HashSet<string>(StringComparer.Ordinal);
        foreach (var identity in ReadRequiredRouteChain(
                     input.LogicalSource.Identity,
                     input.Resolution.RouteFacts.Topology))
        {
            var projection = input.Resolution.Projections.Projections.Single(candidate =>
                ReferenceEquals(candidate.LogicalSource.Identity, identity));
            AddDocumentReadIssue(projection.BaseRead, seenPaths, issues);
            if (projection.OverwriteRead is not null)
            {
                AddDocumentReadIssue(projection.OverwriteRead, seenPaths, issues);
            }
        }
    }

    private static IReadOnlyList<SourceLogicalIdentity> ReadRequiredRouteChain(
        SourceLogicalIdentity selected,
        SourceRouteTopology topology)
    {
        var current = topology.FindByPath(selected.CanonicalBasePath);
        if (current is null)
        {
            return [selected];
        }

        var chain = new List<SourceLogicalIdentity>();
        while (true)
        {
            chain.Add(current.Identity);
            if (current.ParentState != SourceRouteParentState.Resolved)
            {
                break;
            }

            current = topology.FindByPath(current.ParentPaths[0])
                ?? throw new InvalidOperationException("A resolved route parent is missing from the neutral topology.");
        }

        chain.Reverse();
        return chain;
    }

    private static void AddDocumentReadIssue(
        SourceDocumentReadResult document,
        ISet<string> seenPaths,
        ICollection<RouteInspectResolutionIssue> issues)
    {
        if (!seenPaths.Add(document.Layer.CanonicalPath)
            || document.Verification.State == SourceLayerVerificationState.Verified
                && document.Read?.State == FileReadState.Complete)
        {
            return;
        }

        var unsafeBoundary = document.Verification.State is
            SourceLayerVerificationState.Unsafe or SourceLayerVerificationState.Changed;
        issues.Add(RouteInspectResolutionSupport.CreateIssue(
            unsafeBoundary
                ? RouteInspectResolutionIssueCode.UnsafeSource
                : RouteInspectResolutionIssueCode.ReadUnavailable,
            document.Layer.CanonicalPath,
            unsafeBoundary
                ? "A required route-chain source no longer has its proved physical identity."
                : "A required route-chain source body could not be read completely."));
    }

    private static IReadOnlyList<RouteInspectPhysicalLayer> ReadPhysicalLayers(
        RouteSource source)
    {
        var layers = new List<RouteInspectPhysicalLayer>
        {
            new(
                source.Base.CanonicalLogicalPath,
                source.Base.PhysicalPath,
                RouteInspectLayerRole.Base),
        };
        if (source.Overwrite is not null)
        {
            layers.Add(new RouteInspectPhysicalLayer(
                source.Overwrite.CanonicalLogicalPath,
                source.Overwrite.PhysicalPath,
                RouteInspectLayerRole.Overwrite));
        }

        return layers;
    }
}
