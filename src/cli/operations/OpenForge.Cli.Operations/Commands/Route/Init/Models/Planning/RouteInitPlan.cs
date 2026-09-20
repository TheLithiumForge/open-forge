using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;

namespace OpenForge.Cli.Core.Commands.Route.Init.Models.Planning;

internal sealed record RouteInitPlan
{
    internal RouteInitPlan(
        RouteInitRequest request,
        RouteInitResultFormation preview,
        IEnumerable<PlannedDirectoryCreation> directoryCreations,
        IEnumerable<PlannedFileChange> fileChanges,
        IEnumerable<RecoveryBundleTarget> recoveryTargets,
        WorkspaceOwnershipRead? ownership)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(preview);
        ArgumentNullException.ThrowIfNull(directoryCreations);
        ArgumentNullException.ThrowIfNull(fileChanges);
        ArgumentNullException.ThrowIfNull(recoveryTargets);
        Request = request;
        Preview = preview;
        DirectoryCreations = Copy(directoryCreations, nameof(directoryCreations));
        FileChanges = Copy(fileChanges, nameof(fileChanges));
        RecoveryTargets = Copy(recoveryTargets, nameof(recoveryTargets));
        Ownership = ownership;
    }

    internal RouteInitRequest Request { get; }

    internal RouteInitResultFormation Preview { get; }

    internal ImmutableArray<PlannedDirectoryCreation> DirectoryCreations { get; }

    internal ImmutableArray<PlannedFileChange> FileChanges { get; }

    internal ImmutableArray<RecoveryBundleTarget> RecoveryTargets { get; }

    internal WorkspaceOwnershipRead? Ownership { get; }

    internal bool IsNoOp => DirectoryCreations.Length == 0
        && FileChanges.Length == 0;

    private static ImmutableArray<T> Copy<T>(IEnumerable<T> values, string name)
        where T : class
        => values
            .Select(value => value ?? throw new ArgumentException(
                "Route Init plan collections cannot contain null members.",
                name))
            .ToImmutableArray();
}

internal sealed record RouteInitPlanBuild(
    RouteInitPlan? Plan,
    RouteInitResultFormation Formation);
