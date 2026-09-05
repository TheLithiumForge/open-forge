using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;

namespace OpenForge.Cli.Core.Framework.Lifecycle.Operational.Models;

internal enum FrameworkManagedTargetKind
{
    File,
    ManagedRegion,
    GeneratedRegion,
}

internal sealed record FrameworkManagedTargetObservation
{
    public required string Path { get; init; }

    public required FrameworkManagedTargetKind Kind { get; init; }

    public required string? SourceAssetPath { get; init; }

    public required string? Region { get; init; }

    public required string BaselineFingerprint { get; init; }

    public required string FingerprintKind { get; init; }

    public required FrameworkLifecycleTargetSourceValidation Source { get; init; }

    public required OperationalTargetState State { get; init; }
}

internal sealed record FrameworkLifecycleStatusView
{
    public required OperationalViewState State { get; init; }

    public required OperationalLifecyclePresenceState Presence { get; init; }

    public required OperationalLifecycleState Lifecycle { get; init; }

    public required OperationalSourceAvailability SourceAvailability { get; init; }

    public required IReadOnlyList<FrameworkManagedTargetObservation> Targets { get; init; }
}

internal sealed record FrameworkLifecycleDoctorView
{
    public required OperationalViewState State { get; init; }

    public required LifecycleStoreReadResult Lifecycle { get; init; }

    public required FrameworkPayloadReadResult Payload { get; init; }

    public required IReadOnlyList<FrameworkManagedTargetObservation> Targets { get; init; }
}
