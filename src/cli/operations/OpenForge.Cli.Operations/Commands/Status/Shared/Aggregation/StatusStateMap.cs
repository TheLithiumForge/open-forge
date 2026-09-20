using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Framework.Distribution.Operational.Models;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Libraries.Models.Inventory;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;

namespace OpenForge.Cli.Core.Commands.Status.Shared.Aggregation;

internal static class StatusStateMap
{
    internal static StatusValueState Value(OperationalValueState value) => value switch
    {
        OperationalValueState.Available => StatusValueState.Available,
        OperationalValueState.Unavailable => StatusValueState.Unavailable,
        OperationalValueState.NotApplicable => StatusValueState.NotApplicable,
        _ => throw Undefined(value),
    };

    internal static StatusInstallationState Installation(OperationalInstallationState value) => value switch
    {
        OperationalInstallationState.Installed => StatusInstallationState.Installed,
        OperationalInstallationState.Uninstalled => StatusInstallationState.Uninstalled,
        OperationalInstallationState.Incomplete => StatusInstallationState.Incomplete,
        OperationalInstallationState.Blocked => StatusInstallationState.Blocked,
        _ => throw Undefined(value),
    };

    internal static StatusLifecycleState Lifecycle(OperationalLifecycleState value) => value switch
    {
        OperationalLifecycleState.Absent => StatusLifecycleState.Absent,
        OperationalLifecycleState.Trusted => StatusLifecycleState.Trusted,
        OperationalLifecycleState.Untrusted => StatusLifecycleState.Untrusted,
        OperationalLifecycleState.Incomplete => StatusLifecycleState.Incomplete,
        OperationalLifecycleState.Blocked => StatusLifecycleState.Blocked,
        _ => throw Undefined(value),
    };

    internal static StatusSourceAvailability Source(OperationalSourceAvailability value) => value switch
    {
        OperationalSourceAvailability.Available => StatusSourceAvailability.Available,
        OperationalSourceAvailability.Unavailable => StatusSourceAvailability.Unavailable,
        OperationalSourceAvailability.NotApplicable => StatusSourceAvailability.NotApplicable,
        _ => throw Undefined(value),
    };

    internal static StatusTargetState Target(OperationalTargetState value) => value switch
    {
        OperationalTargetState.Current => StatusTargetState.Current,
        OperationalTargetState.Changed => StatusTargetState.Changed,
        OperationalTargetState.Missing => StatusTargetState.Missing,
        OperationalTargetState.Unavailable => StatusTargetState.Unavailable,
        OperationalTargetState.Blocked => StatusTargetState.Blocked,
        _ => throw Undefined(value),
    };

    internal static StatusTargetState LibraryTarget(LibraryMappingObservationState value) => value switch
    {
        LibraryMappingObservationState.Current => StatusTargetState.Current,
        LibraryMappingObservationState.Changed => StatusTargetState.Changed,
        LibraryMappingObservationState.Missing => StatusTargetState.Missing,
        LibraryMappingObservationState.Unavailable => StatusTargetState.Unavailable,
        LibraryMappingObservationState.Blocked => StatusTargetState.Blocked,
        _ => throw Undefined(value),
    };

    internal static StatusGeneratedNavigationState GeneratedNavigation(OperationalGeneratedNavigationState value) => value switch
    {
        OperationalGeneratedNavigationState.Current => StatusGeneratedNavigationState.Current,
        OperationalGeneratedNavigationState.Changed => StatusGeneratedNavigationState.Changed,
        OperationalGeneratedNavigationState.Missing => StatusGeneratedNavigationState.Missing,
        OperationalGeneratedNavigationState.Unavailable => StatusGeneratedNavigationState.Unavailable,
        OperationalGeneratedNavigationState.Blocked => StatusGeneratedNavigationState.Blocked,
        OperationalGeneratedNavigationState.NotApplicable => StatusGeneratedNavigationState.NotApplicable,
        _ => throw Undefined(value),
    };

    internal static StatusManagedTargetKind ManagedTargetKind(FrameworkManagedTargetKind value) => value switch
    {
        FrameworkManagedTargetKind.File => StatusManagedTargetKind.File,
        FrameworkManagedTargetKind.ManagedRegion => StatusManagedTargetKind.ManagedRegion,
        FrameworkManagedTargetKind.GeneratedRegion => StatusManagedTargetKind.GeneratedRegion,
        _ => throw Undefined(value),
    };

    internal static StatusLibraryRecordState LibraryRecord(LibraryRegistrationReadState value) => value switch
    {
        LibraryRegistrationReadState.Missing => StatusLibraryRecordState.Missing,
        LibraryRegistrationReadState.Complete => StatusLibraryRecordState.Complete,
        LibraryRegistrationReadState.Malformed => StatusLibraryRecordState.Malformed,
        LibraryRegistrationReadState.Unavailable => StatusLibraryRecordState.Unavailable,
        LibraryRegistrationReadState.Blocked => StatusLibraryRecordState.Blocked,
        _ => throw Undefined(value),
    };

    internal static StatusLibrarySourceRootState LibrarySourceRoot(LibrarySourceRootState value) => value switch
    {
        LibrarySourceRootState.Available => StatusLibrarySourceRootState.Available,
        LibrarySourceRootState.Missing => StatusLibrarySourceRootState.Missing,
        LibrarySourceRootState.Invalid => StatusLibrarySourceRootState.Invalid,
        LibrarySourceRootState.Inaccessible => StatusLibrarySourceRootState.Inaccessible,
        LibrarySourceRootState.Unavailable => StatusLibrarySourceRootState.Unavailable,
        LibrarySourceRootState.Blocked => StatusLibrarySourceRootState.Blocked,
        _ => throw Undefined(value),
    };

    internal static StatusRecoveryCandidateKind RecoveryCandidate(RecoveryBundleCandidateKind value) => value switch
    {
        RecoveryBundleCandidateKind.Final => StatusRecoveryCandidateKind.Final,
        RecoveryBundleCandidateKind.Draft => StatusRecoveryCandidateKind.Draft,
        _ => throw Undefined(value),
    };

    internal static StatusRecoveryIntegrity RecoveryIntegrity(RecoveryBundleIntegrity value) => value switch
    {
        RecoveryBundleIntegrity.Verified => StatusRecoveryIntegrity.Verified,
        RecoveryBundleIntegrity.Malformed => StatusRecoveryIntegrity.Malformed,
        RecoveryBundleIntegrity.Unsupported => StatusRecoveryIntegrity.Unsupported,
        RecoveryBundleIntegrity.Unavailable => StatusRecoveryIntegrity.Unavailable,
        RecoveryBundleIntegrity.Incomplete => StatusRecoveryIntegrity.Incomplete,
        _ => throw Undefined(value),
    };

    private static ArgumentOutOfRangeException Undefined<T>(T value) where T : struct, Enum
        => new(nameof(value), value, "The Status state is not defined.");
}
