namespace OpenForge.Cli.Core.Framework.OperationalContributors.Models;

internal enum OperationalViewState
{
    Complete,
    Incomplete,
    Blocked,
    Interrupted,
}
internal enum OperationalValueState
{
    Available,
    Unavailable,
    NotApplicable,
}

internal enum OperationalInstallationState
{
    Installed,
    Uninstalled,
    Incomplete,
    Blocked,
}

internal enum OperationalLifecycleState
{
    Absent,
    Trusted,
    Untrusted,
    Incomplete,
    Blocked,
}

internal enum OperationalLifecyclePresenceState
{
    Present,
    Missing,
    Unavailable,
}

internal enum OperationalSourceAvailability
{
    Available,
    Unavailable,
    NotApplicable,
}

internal enum OperationalTargetState
{
    Current,
    Changed,
    Missing,
    Unavailable,
    Blocked,
}

internal enum OperationalGeneratedNavigationState
{
    Current,
    Changed,
    Missing,
    Unavailable,
    Blocked,
    NotApplicable,
}
