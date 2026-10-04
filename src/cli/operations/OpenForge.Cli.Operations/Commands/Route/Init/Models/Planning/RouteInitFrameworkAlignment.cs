using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Framework.Distribution.Shared.Sources;
using System.Text;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Routing;
using OpenForge.Cli.Core.Framework.Workspace.Models;

using OpenForge.Cli.Core.Commands.Route.Init.Shared.Planning;

namespace OpenForge.Cli.Core.Commands.Route.Init.Models.Planning;

internal enum RouteInitFrameworkAlignmentState
{
    Complete,
    Blocked,
}

internal sealed record RouteInitFrameworkAlignedSegment(
    string ConcreteSegment,
    RouteInitFrameworkSegmentRole Role,
    SourceLogicalSource IntendedSource,
    string? SourceAssetPath);

internal sealed record RouteInitFrameworkAlignment
{
    internal RouteInitFrameworkAlignment(
        RouteInitTargetFacts target,
        IEnumerable<RouteInitFrameworkAlignedSegment> segments,
        IEnumerable<SourceLogicalSource> payloadSources,
        SourceRouteTopology payloadTopology)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(payloadTopology);
        Target = target;
        Segments = new ReadOnlyCollection<RouteInitFrameworkAlignedSegment>(segments.ToArray());
        PayloadSources = new ReadOnlyCollection<SourceLogicalSource>(payloadSources.ToArray());
        PayloadTopology = payloadTopology;
    }

    internal RouteInitTargetFacts Target { get; }

    internal IReadOnlyList<RouteInitFrameworkAlignedSegment> Segments { get; }

    internal IReadOnlyList<SourceLogicalSource> PayloadSources { get; }

    internal SourceRouteTopology PayloadTopology { get; }

    internal bool IsCanonicalRestoration { get; init; }
}

internal sealed record RouteInitFrameworkAlignmentBuild(
    RouteInitFrameworkAlignmentState State,
    RouteInitFrameworkAlignment? Alignment,
    string? Cause);
