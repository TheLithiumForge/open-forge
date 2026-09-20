namespace OpenForge.Cli.Core.Commands.Status.Models.Result;

// Status owns the vocabulary consumed by native Presentation. Framework and
// contributor states are translated at the command aggregation boundary.
internal enum StatusValueState
{
    Available,
    Unavailable,
    NotApplicable,
}

internal enum StatusInstallationState
{
    Installed,
    Uninstalled,
    Incomplete,
    Blocked,
}

internal enum StatusLifecycleState
{
    Absent,
    Trusted,
    Untrusted,
    Incomplete,
    Blocked,
}

internal enum StatusSourceAvailability
{
    Available,
    Unavailable,
    NotApplicable,
}

internal enum StatusTargetState
{
    Current,
    Changed,
    Missing,
    Unavailable,
    Blocked,
}

internal enum StatusGeneratedNavigationState
{
    Current,
    Changed,
    Missing,
    Unavailable,
    Blocked,
    NotApplicable,
}

internal enum StatusManagedTargetKind
{
    File,
    ManagedRegion,
    GeneratedRegion,
}

internal enum StatusLibraryRecordState
{
    Missing,
    Complete,
    Malformed,
    Unavailable,
    Blocked,
}

internal enum StatusLibrarySourceRootState
{
    Available,
    Missing,
    Invalid,
    Inaccessible,
    Unavailable,
    Blocked,
}

internal enum StatusRecoveryCandidateKind
{
    Final,
    Draft,
}

internal enum StatusRecoveryIntegrity
{
    Verified,
    Malformed,
    Unsupported,
    Unavailable,
    Incomplete,
}
