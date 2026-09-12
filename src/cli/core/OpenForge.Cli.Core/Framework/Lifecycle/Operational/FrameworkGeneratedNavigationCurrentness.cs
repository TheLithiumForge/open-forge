using OpenForge.Cli.Core.Framework.Lifecycle.Models.Identity;
using OpenForge.Cli.Core.Framework.Lifecycle.Operational.Models;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;

namespace OpenForge.Cli.Core.Framework.Lifecycle.Operational;

internal static class FrameworkGeneratedNavigationCurrentness
{
    internal static OperationalTargetState ReadState(
        FrameworkManagedTargetObservation target,
        OperationalSourceAvailability sourceAvailability,
        IReadOnlySet<string> currentNavigationPaths)
        => sourceAvailability == OperationalSourceAvailability.Available
            && target.Source.State == FrameworkLifecycleTargetSourceState.Valid
            && target.Kind == FrameworkManagedTargetKind.GeneratedRegion
            && target.State == OperationalTargetState.Changed
            && currentNavigationPaths.Contains(target.Path)
                ? OperationalTargetState.Current
                : target.State;
}
