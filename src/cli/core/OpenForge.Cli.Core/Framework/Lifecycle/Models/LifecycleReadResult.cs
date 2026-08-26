using System.Collections.ObjectModel;

namespace OpenForge.Cli.Core.Framework.Lifecycle.Models;

internal enum LifecycleReadState
{
    Complete,
    Missing,
    Invalid,
    Unavailable,
    Cancelled,
}

internal enum LifecycleExtensionTrust
{
    Trusted,
    Untrusted,
    Incomplete,
    Blocked,
    Absent,
}

internal enum LifecycleCoverageState
{
    NotStarted,
    Complete,
    Incomplete,
    Blocked,
    Failed,
    Interrupted,
}

internal enum LifecycleWorkspaceBinding
{
    NotChecked,
    Matched,
    Mismatched,
    Unavailable,
}

internal sealed record LifecycleInstalledPackage
{
    public required string Id { get; init; }

    public required string? Version { get; init; }

    public required string? Source { get; init; }

    public required IReadOnlyList<string> Dependencies { get; init; }

    public required IReadOnlyList<string> Paths { get; init; }
}

internal sealed record LifecycleInstalledPath
{
    public required string Path { get; init; }

    public required IReadOnlyList<string> Owners { get; init; }

    public required string BaselineFingerprint { get; init; }

    public required string FingerprintKind { get; init; }
}

internal sealed record LifecycleCoverageFacts
{
    public required IReadOnlyList<LifecycleInstalledPath> Paths { get; init; }

    public required LifecycleCoverageState Coverage { get; init; }

    public required LifecycleWorkspaceBinding WorkspaceBinding { get; init; }

    public string? FingerprintPolicy { get; init; }
}

internal sealed record LifecycleReadResult
{
    internal LifecycleReadResult(
        LifecycleReadState state,
        LifecycleExtensionTrust trust,
        IEnumerable<LifecycleInstalledPackage> packages,
        string? cause)
        : this(
            state,
            trust,
            packages,
            cause,
            new LifecycleCoverageFacts
            {
                Paths = [],
                Coverage = LifecycleCoverageState.NotStarted,
                WorkspaceBinding = LifecycleWorkspaceBinding.NotChecked,
            })
    {
    }

    internal static LifecycleReadResult Create(
        LifecycleReadState state,
        LifecycleExtensionTrust trust,
        IEnumerable<LifecycleInstalledPackage> packages,
        string? cause,
        LifecycleCoverageFacts coverageFacts)
        => new(state, trust, packages, cause, coverageFacts);

    private LifecycleReadResult(
        LifecycleReadState state,
        LifecycleExtensionTrust trust,
        IEnumerable<LifecycleInstalledPackage> packages,
        string? cause,
        LifecycleCoverageFacts coverageFacts)
    {
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, "The lifecycle read state is not defined.");
        }

        if (!Enum.IsDefined(trust))
        {
            throw new ArgumentOutOfRangeException(nameof(trust), trust, "The lifecycle Extension trust is not defined.");
        }

        ArgumentNullException.ThrowIfNull(packages);
        var values = packages
            .Select(value => value ?? throw new ArgumentException("Lifecycle packages cannot contain null members.", nameof(packages)))
            .ToArray();
        ArgumentNullException.ThrowIfNull(coverageFacts);
        var pathValues = coverageFacts.Paths
            .Select(value => value ?? throw new ArgumentException("Lifecycle paths cannot contain null members.", nameof(coverageFacts.Paths)))
            .ToArray();
        if (!Enum.IsDefined(coverageFacts.Coverage))
        {
            throw new ArgumentOutOfRangeException(nameof(coverageFacts.Coverage), coverageFacts.Coverage, "The lifecycle coverage is not defined.");
        }

        if (!Enum.IsDefined(coverageFacts.WorkspaceBinding))
        {
            throw new ArgumentOutOfRangeException(nameof(coverageFacts.WorkspaceBinding), coverageFacts.WorkspaceBinding, "The lifecycle workspace binding is not defined.");
        }

        State = state;
        Trust = trust;
        Packages = new ReadOnlyCollection<LifecycleInstalledPackage>(values);
        Paths = new ReadOnlyCollection<LifecycleInstalledPath>(pathValues);
        Cause = cause;
        Coverage = coverageFacts.Coverage;
        WorkspaceBinding = coverageFacts.WorkspaceBinding;
        FingerprintPolicy = coverageFacts.FingerprintPolicy;
    }

    internal LifecycleReadState State { get; }

    internal LifecycleExtensionTrust Trust { get; }

    internal IReadOnlyList<LifecycleInstalledPackage> Packages { get; }

    internal string? Cause { get; }

    internal IReadOnlyList<LifecycleInstalledPath> Paths { get; }

    internal LifecycleCoverageState Coverage { get; }

    internal LifecycleWorkspaceBinding WorkspaceBinding { get; }

    internal string? FingerprintPolicy { get; }
}
