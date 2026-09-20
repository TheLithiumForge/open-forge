using OpenForge.Cli.Core.Framework.Distribution.Models.Content;
using OpenForge.Cli.Core.Framework.Distribution.Operational.Models;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;

namespace OpenForge.Cli.Core.Framework.Distribution.Operational;

internal static class FrameworkGeneratedNavigationCurrentness
{
    internal static OperationalTargetState ReadState(
        FrameworkManagedTargetObservation target,
        OperationalSourceAvailability sourceAvailability,
        IReadOnlySet<string> currentNavigationPaths)
        => sourceAvailability == OperationalSourceAvailability.Available
            && target.Source.State == FrameworkTargetSourceState.Valid
            && target.Kind == FrameworkManagedTargetKind.GeneratedRegion
            && target.State == OperationalTargetState.Changed
            && currentNavigationPaths.Contains(target.Path)
                ? OperationalTargetState.Current
                : target.State;
}
