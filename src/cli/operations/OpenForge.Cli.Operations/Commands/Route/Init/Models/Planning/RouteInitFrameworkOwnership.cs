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

using OpenForge.Cli.Core.Commands.Route.Init.Shared.Planning;

namespace OpenForge.Cli.Core.Commands.Route.Init.Models.Planning;

internal enum RouteInitFrameworkTrustState
{
    Current,
    InstallRequired,
    UpdateRequired,
    Incomplete,
    Blocked,
    Cancelled,
}

internal sealed record RouteInitFrameworkTrust(
    RouteInitFrameworkTrustState State,
    WorkspaceOwnershipRead Ownership,
    string? Cause)
{
    internal bool IsCurrent => State == RouteInitFrameworkTrustState.Current;
}

internal sealed record RouteInitFrameworkManagedState
{
    internal RouteInitFrameworkManagedState(
        string path,
        string? sourceAssetPath,
        bool ownsGeneratedEntries)
    {
        if (!SourceLogicalPath.IsCanonical(path))
        {
            throw new ArgumentException("A scoped Framework ownership path must be canonical.", nameof(path));
        }

        if (sourceAssetPath is not null && !SourceLogicalPath.IsCanonical(sourceAssetPath))
        {
            throw new ArgumentException("A scoped Framework ownership source asset path must be canonical.", nameof(sourceAssetPath));
        }

        if (sourceAssetPath is null && !ownsGeneratedEntries)
        {
            throw new ArgumentException(
                "A scoped Framework ownership addition must own a whole source or its generated Entries region.",
                nameof(ownsGeneratedEntries));
        }

        Path = path;
        SourceAssetPath = sourceAssetPath;
        OwnsGeneratedEntries = ownsGeneratedEntries;
    }

    internal string Path { get; }

    internal string? SourceAssetPath { get; }

    internal bool OwnsGeneratedEntries { get; }
}

internal enum RouteInitFrameworkOwnershipPlanState
{
    Complete,
    Blocked,
}

internal sealed record RouteInitFrameworkOwnershipPlan(
    RouteInitFrameworkOwnershipPlanState State,
    OwnershipWritePlanResult? WritePlan,
    string? Cause);
