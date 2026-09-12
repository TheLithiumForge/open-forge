using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Effects;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Selection;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;

namespace OpenForge.Cli.Core.Commands.Extension.Remove.Models.Planning;

internal sealed record ExtensionRemovePlanningAuthority(
    ExtensionRemoveChangedContentPolicy ChangedContentPolicy);

internal enum ExtensionRemovePlanningDisposition
{
    NoOp,
    ReleaseOwnership,
    Delete,
    Retain,
    Blocked,
}

internal sealed record ExtensionRemovePlanningDecision
{
    internal required ExtensionRemovePathPlan Path { get; init; }

    internal required ExtensionRemovePlanningDisposition Disposition { get; init; }
}

internal sealed record ExtensionRemoveTopology
{
    internal required IReadOnlyDictionary<string, byte[]> IntendedTargetBytes { get; init; }

    internal required IReadOnlyDictionary<string, IReadOnlyList<GeneratedNavigationEntry>>
        GeneratedEntries
    { get; init; }

    internal required IReadOnlySet<string> ProtectedPaths { get; init; }
}

internal sealed record ExtensionRemovePlanningPlan
{
    internal required ExtensionRemovePlanningAuthority Authority { get; init; }

    internal required IReadOnlyList<ExtensionRemovePlanningDecision> Decisions { get; init; }

    internal bool IsNoOp => Decisions.All(
        decision => decision.Disposition is ExtensionRemovePlanningDisposition.NoOp
            or ExtensionRemovePlanningDisposition.Retain);

    internal bool IsBlocked => Decisions.Any(
        decision => decision.Disposition == ExtensionRemovePlanningDisposition.Blocked);
}

internal sealed record ExtensionRemovePlannedEffect
{
    internal required ExtensionRemoveEffect Result { get; init; }

    internal PlannedFileChange? FileChange { get; init; }

    internal RecoveryBundleTarget? RecoveryTarget { get; init; }
}

internal sealed record ExtensionRemovePlanInput
{
    internal required ExtensionRemoveRequest Request { get; init; }

    internal required ExtensionRemoveSelection Selection { get; init; }

    internal required ExtensionRemoveDependencyPlan Dependencies { get; init; }

    internal required ExtensionRemovePlanningPlan Planning { get; init; }

    internal required ExtensionRemoveTopology Topology { get; init; }

    internal required IReadOnlyList<ExtensionRemovePlannedEffect> Effects { get; init; }

    internal PlannedFileChange? LifecycleChange { get; init; }

    internal RecoveryBundleTarget? LifecycleRecoveryTarget { get; init; }
}

internal sealed class ExtensionRemovePlan
{
    private ExtensionRemovePlan(ExtensionRemovePlanInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(input.Request);
        ArgumentNullException.ThrowIfNull(input.Selection);
        ArgumentNullException.ThrowIfNull(input.Dependencies);
        ArgumentNullException.ThrowIfNull(input.Planning);
        ArgumentNullException.ThrowIfNull(input.Topology);
        ArgumentNullException.ThrowIfNull(input.Effects);
        if (input.Request.RequestedIds.Count > 0
            && !input.Request.RequestedIds
                .OrderBy(id => id, StringComparer.Ordinal)
                .SequenceEqual(
                    input.Selection.Ids.OrderBy(id => id, StringComparer.Ordinal),
                    StringComparer.Ordinal))
        {
            throw new ArgumentException(
                "Extension Remove plans must snapshot the request selection.",
                nameof(input.Selection));
        }

        var selectedIds = new HashSet<string>(input.Selection.Ids, StringComparer.Ordinal);
        var selectedPackageIds = input.Dependencies.Packages
            .Where(package => package.SelectedForRemoval)
            .Select(package => package.Id)
            .OrderBy(id => id, StringComparer.Ordinal)
            .ToArray();
        var selectionIds = input.Selection.Ids
            .OrderBy(id => id, StringComparer.Ordinal)
            .ToArray();
        if (!selectionIds.SequenceEqual(selectedPackageIds, StringComparer.Ordinal))
        {
            throw new ArgumentException(
                "Extension Remove plans must select exactly the dependency-plan packages marked for removal.",
                nameof(input.Dependencies));
        }

        var pathValues = input.Planning.Decisions
            .Select(decision => decision.Path)
            .ToArray();
        var seenPaths = new HashSet<string>(StringComparer.Ordinal);
        foreach (var path in pathValues)
        {
            if (!seenPaths.Add(path.Path))
            {
                throw new ArgumentException(
                    "Extension Remove planning decisions must identify unique paths.",
                    nameof(input.Planning));
            }

            if (path.SelectedOwnerIds.Any(ownerId => !selectedIds.Contains(ownerId)))
            {
                throw new ArgumentException(
                    "Extension Remove path owners must belong to the selected IDs.",
                    nameof(input.Planning));
            }

            if (path.Classification == ExtensionRemovePathClassification.ChangedFinalOwner)
            {
                var expectedAction = ReadChangedFinalOwnerAction(
                    input.Request,
                    input.Planning.Authority);
                if (path.Action != expectedAction)
                {
                    throw new ArgumentException(
                        "Extension Remove changed-owner actions must match same-request prune authority.",
                        nameof(input.Planning));
                }
            }
        }

        Request = input.Request;
        Selection = input.Selection;
        Dependencies = input.Dependencies;
        Planning = input.Planning with
        {
            Decisions = new ReadOnlyCollection<ExtensionRemovePlanningDecision>([.. input.Planning.Decisions]),
        };
        Topology = new ExtensionRemoveTopologySnapshot(input.Topology);
        Effects = new ReadOnlyCollection<ExtensionRemovePlannedEffect>(input.Effects
            .Select(effect => effect ?? throw new ArgumentException(
                "Extension Remove planned effects cannot contain null members.",
                nameof(input.Effects)))
            .ToArray());
        LifecycleChange = input.LifecycleChange;
        LifecycleRecoveryTarget = input.LifecycleRecoveryTarget;
    }

    internal static ExtensionRemovePlan Create(ExtensionRemovePlanInput input)
        => new(input);

    internal ExtensionRemoveRequest Request { get; }

    internal ExtensionRemoveSelection Selection { get; }

    internal ExtensionRemoveDependencyPlan Dependencies { get; }

    internal ExtensionRemovePlanningPlan Planning { get; }

    internal ExtensionRemoveTopologySnapshot Topology { get; }

    internal IReadOnlyList<ExtensionRemovePlannedEffect> Effects { get; }

    internal PlannedFileChange? LifecycleChange { get; }

    internal RecoveryBundleTarget? LifecycleRecoveryTarget { get; }

    internal bool IsNoOp => Effects.Count == 0 && LifecycleChange is null;

    private static ExtensionRemovePathAction ReadChangedFinalOwnerAction(
        ExtensionRemoveRequest request,
        ExtensionRemovePlanningAuthority authority)
    {
        if (!Enum.IsDefined(authority.ChangedContentPolicy))
        {
            throw new ArgumentOutOfRangeException(
                nameof(authority),
                authority.ChangedContentPolicy,
                "The Extension Remove changed-content policy is not defined.");
        }

        if (request.Prune)
        {
            return ExtensionRemovePathAction.Delete;
        }

        if (!request.AllowInteraction || request.Automatic)
        {
            if (authority.ChangedContentPolicy != ExtensionRemoveChangedContentPolicy.KeepAsUnmanaged)
            {
                throw new ArgumentException(
                    "Non-interactive Extension Remove cannot select Delete without --prune.",
                    nameof(authority));
            }

            return ExtensionRemovePathAction.KeepAsUnmanaged;
        }

        return authority.ChangedContentPolicy switch
        {
            ExtensionRemoveChangedContentPolicy.KeepAsUnmanaged => ExtensionRemovePathAction.KeepAsUnmanaged,
            ExtensionRemoveChangedContentPolicy.Delete => ExtensionRemovePathAction.Delete,
            _ => throw new ArgumentOutOfRangeException(
                nameof(authority),
                authority.ChangedContentPolicy,
                "The Extension Remove changed-content policy is not defined."),
        };
    }
}

internal sealed record ExtensionRemovePlanBuild
{
    internal required ExtensionRemovePlan? Plan { get; init; }

    internal required ExtensionRemoveResult Result { get; init; }
}
