using OpenForge.Cli.Core.Framework.Filesystem.LogicalPaths.Models;
using System.Collections.Immutable;

namespace OpenForge.Cli.Core.Framework.Recovery.Models;

internal enum RecoveryBundleTargetComparisonState
{
    Prior,
    Intended,
    Third,
    Unavailable,
    Blocked,
}

internal sealed record RecoveryBundleTargetComparison
{
    private RecoveryBundleTargetComparison(
        string targetPath,
        RecoveryBundleTargetComparisonState state,
        RecoveryContentIdentity? observed,
        string? cause)
    {
        TargetPath = CanonicalRelativePath.Create(targetPath).Value;
        State = state;
        Observed = observed;
        Cause = cause;
    }

    internal string TargetPath { get; }

    internal RecoveryBundleTargetComparisonState State { get; }

    internal RecoveryContentIdentity? Observed { get; }

    internal string? Cause { get; }

    internal static RecoveryBundleTargetComparison Prior(
        string targetPath,
        RecoveryContentIdentity observed)
        => ObservedState(
            targetPath,
            RecoveryBundleTargetComparisonState.Prior,
            observed);

    internal static RecoveryBundleTargetComparison Intended(
        string targetPath,
        RecoveryContentIdentity? observed)
        => new(
            targetPath,
            RecoveryBundleTargetComparisonState.Intended,
            observed,
            cause: null);

    internal static RecoveryBundleTargetComparison Third(
        string targetPath,
        RecoveryContentIdentity? observed)
        => new(
            targetPath,
            RecoveryBundleTargetComparisonState.Third,
            observed,
            cause: null);

    internal static RecoveryBundleTargetComparison Classified(
        string targetPath,
        RecoveryBundleTargetComparisonState state,
        string cause)
    {
        if (state is not (RecoveryBundleTargetComparisonState.Unavailable
            or RecoveryBundleTargetComparisonState.Blocked))
        {
            throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "Only unavailable or blocked recovery comparisons carry a cause.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(cause);
        return new RecoveryBundleTargetComparison(targetPath, state, observed: null, cause);
    }

    private static RecoveryBundleTargetComparison ObservedState(
        string targetPath,
        RecoveryBundleTargetComparisonState state,
        RecoveryContentIdentity observed)
    {
        ArgumentNullException.ThrowIfNull(observed);
        return new RecoveryBundleTargetComparison(targetPath, state, observed, cause: null);
    }
}

internal sealed record RecoveryBundleComparison
{
    private RecoveryBundleComparison(
        string bundlePath,
        RecoveryBundleAttribution attribution,
        ImmutableArray<RecoveryBundleTargetComparison> targets)
    {
        BundlePath = Path.GetFullPath(bundlePath);
        Attribution = attribution;
        Targets = targets;
    }

    internal string BundlePath { get; }

    internal RecoveryBundleAttribution Attribution { get; }

    internal ImmutableArray<RecoveryBundleTargetComparison> Targets { get; }

    internal bool IsPartial
        => Targets.Any(target => target.State == RecoveryBundleTargetComparisonState.Prior)
            && Targets.Any(target => target.State == RecoveryBundleTargetComparisonState.Intended)
            && Targets.All(target => target.State is RecoveryBundleTargetComparisonState.Prior
                or RecoveryBundleTargetComparisonState.Intended);

    internal static RecoveryBundleComparison Create(
        string bundlePath,
        RecoveryBundleAttribution attribution,
        ImmutableArray<RecoveryBundleTargetComparison> targets)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(bundlePath);
        ArgumentNullException.ThrowIfNull(attribution);
        if (attribution.Producer != RecoveryBundleProducer.Framework)
        {
            throw new ArgumentException(
                "Recovery target comparison is defined only for Framework-attributed finals.",
                nameof(attribution));
        }

        if (targets.IsDefaultOrEmpty || targets.Any(target => target is null))
        {
            throw new ArgumentException(
                "A recovery comparison requires one or more compared targets.",
                nameof(targets));
        }

        return new RecoveryBundleComparison(bundlePath, attribution, targets);
    }
}
