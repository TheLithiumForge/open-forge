using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Models;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;

using OpenForge.Cli.Core.Commands.Route.Init.Models.Planning;

namespace OpenForge.Cli.Core.Commands.Route.Init.Shared.Planning;

internal sealed class RouteInitFrameworkOwnershipBuilder
{
    private readonly PhysicalPathResolver _physicalPathResolver = new();
    private readonly WorkspaceOwnershipStore _ownershipStore = new();

    internal ValueTask<RouteInitFrameworkTrust> ReadTrustAsync(
        WorkspaceLayoutRead layoutRead,
        FrameworkPayload payload,
        CancellationToken cancellationToken)
    {
        var layout = layoutRead.Layout ?? throw new InvalidOperationException("Framework trust requires a layout.");
        var workspace = layout.Workspace;
        ArgumentNullException.ThrowIfNull(payload);
        cancellationToken.ThrowIfCancellationRequested();
        var ownership = layoutRead.Ownership;
        var container = SourceLogicalPath.ToLexicalPath(workspace.LexicalRoot, layout.MainRoot);
        var physical = _physicalPathResolver.ResolveCandidate(workspace.LexicalRoot, workspace.PhysicalRoot, container);
        if (physical.State == PhysicalPathState.Missing)
        {
            return ValueTask.FromResult(new RouteInitFrameworkTrust(RouteInitFrameworkTrustState.InstallRequired, ownership, $"The {layout.MainRoot} Framework container is missing."));
        }
        if (physical.State != PhysicalPathState.Contained
            || !Directory.Exists(physical.GetContainedPhysicalPath())
            || (File.GetAttributes(container) & FileAttributes.ReparsePoint) != 0)
        {
            return ValueTask.FromResult(new RouteInitFrameworkTrust(RouteInitFrameworkTrustState.Blocked, ownership, $"The {layout.MainRoot} Framework container is not a contained ordinary directory."));
        }
        return ValueTask.FromResult(new RouteInitFrameworkTrust(RouteInitFrameworkTrustState.Current, ownership, null));
    }

    internal RouteInitFrameworkOwnershipPlan BuildPlan(
        RouteInitFrameworkTrust trust,
        FrameworkPayload payload,
        IEnumerable<RouteInitFrameworkManagedState> additions)
    {
        ArgumentNullException.ThrowIfNull(trust);
        ArgumentNullException.ThrowIfNull(payload);
        ArgumentNullException.ThrowIfNull(additions);
        if (!trust.IsCurrent)
        {
            return Blocked(trust.Cause ?? "Scoped Framework ownership requires a safe current workspace boundary.");
        }
        var materialized = additions.ToArray();
        if (materialized.Length == 0)
        {
            return new(RouteInitFrameworkOwnershipPlanState.Complete,
                trust.Ownership.IsTrustworthy ? OwnershipWritePlanResult.Unchanged()
                    : OwnershipWritePlanResult.Skipped("No new Framework effects establish ownership."), null);
        }
        var current = trust.Ownership.Document.Framework;
        var paths = (current?.Paths ?? []).ToHashSet(StringComparer.Ordinal);
        var regions = (current?.Regions ?? []).ToHashSet();
        var extensionPaths = trust.Ownership.Document.Extensions
            .SelectMany(extension => extension.Paths.Concat(extension.Regions.Select(region => region.Path)))
            .Select(PortableWorkspacePath.CreatePortableKey).ToHashSet(StringComparer.Ordinal);
        foreach (var addition in materialized.OrderBy(value => value.Path, StringComparer.Ordinal))
        {
            if (extensionPaths.Contains(PortableWorkspacePath.CreatePortableKey(addition.Path)))
            {
                return Blocked($"The scoped Framework target '{addition.Path}' is already owned by an Extension.");
            }
            if (addition.SourceAssetPath is { } sourceAssetPath)
            {
                if (payload.Find(sourceAssetPath) is null)
                {
                    return Blocked($"The scoped Framework source '{sourceAssetPath}' is not present in the running embedded inventory.");
                }
                paths.Add(addition.Path);
            }
            if (addition.OwnsGeneratedEntries)
            {
                regions.Add(new OwnedRegion(addition.Path, "entries"));
            }
        }
        var intended = new FrameworkOwnership(current?.Source ?? new OwnedSource("embedded-framework", null),
            [.. paths.Order(StringComparer.Ordinal)],
            [.. regions.OrderBy(region => region.Path, StringComparer.Ordinal).ThenBy(region => region.Region, StringComparer.Ordinal)])
        { GitIgnoredRoutes = current?.GitIgnoredRoutes ?? [] };
        return new(RouteInitFrameworkOwnershipPlanState.Complete,
            _ownershipStore.PlanFrameworkOwnership(trust.Ownership, intended), null);
    }

    private static RouteInitFrameworkOwnershipPlan Blocked(string cause)
        => new(RouteInitFrameworkOwnershipPlanState.Blocked, null, cause);
}
