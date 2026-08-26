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

internal sealed record LifecycleInstalledPackage
{
    public required string Id { get; init; }

    public required string? Version { get; init; }

    public required string Source { get; init; }

    public required IReadOnlyList<string> Dependencies { get; init; }

    public required IReadOnlyList<string> Paths { get; init; }
}

internal sealed record LifecycleReadResult
{
    internal LifecycleReadResult(
        LifecycleReadState state,
        LifecycleExtensionTrust trust,
        IEnumerable<LifecycleInstalledPackage> packages,
        string? cause)
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
        State = state;
        Trust = trust;
        Packages = new ReadOnlyCollection<LifecycleInstalledPackage>(values);
        Cause = cause;
    }

    internal LifecycleReadState State { get; }

    internal LifecycleExtensionTrust Trust { get; }

    internal IReadOnlyList<LifecycleInstalledPackage> Packages { get; }

    internal string? Cause { get; }
}
