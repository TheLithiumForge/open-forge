using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Ownership;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;

namespace OpenForge.Cli.Core.Framework.Extensions.Operational.Models;

internal sealed record InstalledExtensionObservation
{
    public required string Id { get; init; }

    public required string? Version { get; init; }

    public required string? Source { get; init; }

    public required OperationalSourceAvailability SourceAvailability { get; init; }

    public required IReadOnlyList<string> Dependencies { get; init; }

    public required IReadOnlyList<string> Paths { get; init; }
}
internal sealed record ExtensionManagedTargetObservation
{
    public required string Path { get; init; }

    public required IReadOnlyList<string> Owners { get; init; }

    public required string BaselineFingerprint { get; init; }

    public required string FingerprintKind { get; init; }

    public required OperationalTargetState State { get; init; }
}

internal sealed record ExtensionSourceObservation(
    string? RecordedSource,
    ExtensionSourceReadResult Read);

internal sealed record ExtensionLifecycleStatusView
{
    public required OperationalViewState State { get; init; }

    public required OperationalLifecyclePresenceState Presence { get; init; }

    public required OperationalLifecycleState Lifecycle { get; init; }

    public required OperationalSourceAvailability SourceAvailability { get; init; }

    public required IReadOnlyList<InstalledExtensionObservation> Installed { get; init; }

    public required IReadOnlyList<ExtensionManagedTargetObservation> Targets { get; init; }
}

internal sealed record ExtensionLifecycleDoctorView
{
    public required OperationalViewState State { get; init; }

    public required LifecycleReadResult Lifecycle { get; init; }

    public required IReadOnlyList<ExtensionSourceObservation> Sources { get; init; }

    public required LifecycleOwnershipReadResult Ownership { get; init; }

    public required IReadOnlyList<ExtensionManagedTargetObservation> Targets { get; init; }
}
