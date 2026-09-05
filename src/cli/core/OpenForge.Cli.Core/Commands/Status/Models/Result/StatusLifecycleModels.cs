using OpenForge.Cli.Core.Framework.Lifecycle.Operational.Models;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;

namespace OpenForge.Cli.Core.Commands.Status.Models.Result;

internal sealed record StatusFrameworkTarget
{
    public required string Path { get; init; }

    public required FrameworkManagedTargetKind Kind { get; init; }

    public required string? SourceAssetPath { get; init; }

    public required string? Region { get; init; }

    public required string BaselineFingerprint { get; init; }

    public required string FingerprintKind { get; init; }

    public required OperationalTargetState State { get; init; }
}

internal sealed record StatusFrameworkLifecycle
{
    public required OperationalLifecycleState State { get; init; }

    public required OperationalSourceAvailability SourceAvailability { get; init; }

    public required IReadOnlyList<StatusFrameworkTarget> Targets { get; init; }
}

internal sealed record StatusInstalledExtension
{
    public required string Id { get; init; }

    public required string? Version { get; init; }

    public required string? Source { get; init; }

    public required OperationalSourceAvailability SourceAvailability { get; init; }

    public required IReadOnlyList<string> Dependencies { get; init; }

    public required IReadOnlyList<string> Paths { get; init; }
}

internal sealed record StatusExtensionTarget
{
    public required string Path { get; init; }

    public required IReadOnlyList<string> Owners { get; init; }

    public required string BaselineFingerprint { get; init; }

    public required string FingerprintKind { get; init; }

    public required OperationalTargetState State { get; init; }
}

internal sealed record StatusManagedTargetCounts
{
    public required StatusIntegerValue Current { get; init; }

    public required StatusIntegerValue Changed { get; init; }

    public required StatusIntegerValue Missing { get; init; }

    public required StatusIntegerValue Unavailable { get; init; }

    public required StatusIntegerValue Blocked { get; init; }
}

internal sealed record StatusManagedExtensionFiles
{
    public required StatusManagedTargetCounts Counts { get; init; }

    public required IReadOnlyList<StatusExtensionTarget> Targets { get; init; }
}

internal sealed record StatusExtensionLifecycle
{
    public required OperationalLifecycleState State { get; init; }

    public required OperationalSourceAvailability SourceAvailability { get; init; }

    public required IReadOnlyList<StatusInstalledExtension> Installed { get; init; }

    public required StatusManagedExtensionFiles ManagedFiles { get; init; }
}

internal sealed record StatusLifecycle(
    StatusFrameworkLifecycle Framework,
    StatusExtensionLifecycle Extensions);
