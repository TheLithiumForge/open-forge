using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Framework.Extensions.Operational.Models;
using OpenForge.Cli.Core.Framework.Distribution.Operational;
using OpenForge.Cli.Core.Framework.Distribution.Operational.Models;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;

namespace OpenForge.Cli.Core.Commands.Status.Shared.Aggregation;

internal static class StatusLifecycleAggregator
{
    internal static StatusLifecycle Build(
        FrameworkLifecycleStatusView framework,
        ExtensionLifecycleStatusView extensions,
        bool absenceProven,
        IReadOnlySet<string> currentNavigationPaths)
    {
        if (absenceProven)
        {
            return Absent();
        }

        var targets = new List<StatusFrameworkTarget>(framework.Targets.Count);
        foreach (var target in framework.Targets.OrderBy(target => target.Path, StringComparer.Ordinal))
        {
            targets.Add(Project(target, framework.SourceAvailability, currentNavigationPaths));
        }

        return new StatusLifecycle(
            new StatusFrameworkLifecycle
            {
                State = StatusStateMap.Lifecycle(framework.Lifecycle),
                SourceAvailability = StatusStateMap.Source(framework.SourceAvailability),
                Targets = targets,
            },
            new StatusExtensionLifecycle
            {
                State = StatusStateMap.Lifecycle(extensions.Lifecycle),
                SourceAvailability = StatusStateMap.Source(extensions.SourceAvailability),
                Installed = extensions.Installed
                    .OrderBy(extension => extension.Id, StringComparer.Ordinal)
                    .Select(Project)
                    .ToArray(),
                ManagedFiles = ManagedFiles(extensions),
            });
    }

    internal static StatusLifecycle Unavailable()
    {
        var unavailable = new StatusIntegerValue(StatusValueState.Unavailable, null);
        return new StatusLifecycle(
            new StatusFrameworkLifecycle
            {
                State = StatusLifecycleState.Incomplete,
                SourceAvailability = StatusSourceAvailability.Unavailable,
                Targets = [],
            },
            new StatusExtensionLifecycle
            {
                State = StatusLifecycleState.Incomplete,
                SourceAvailability = StatusSourceAvailability.Unavailable,
                Installed = [],
                ManagedFiles = new StatusManagedExtensionFiles
                {
                    Counts = new StatusManagedTargetCounts
                    {
                        Current = unavailable,
                        Changed = unavailable,
                        Missing = unavailable,
                        Unavailable = unavailable,
                        Blocked = unavailable,
                    },
                    Targets = [],
                },
            });
    }

    private static StatusLifecycle Absent()
    {
        var zero = new StatusIntegerValue(StatusValueState.Available, 0L);
        return new StatusLifecycle(
            new StatusFrameworkLifecycle
            {
                State = StatusLifecycleState.Absent,
                SourceAvailability = StatusSourceAvailability.NotApplicable,
                Targets = [],
            },
            new StatusExtensionLifecycle
            {
                State = StatusLifecycleState.Absent,
                SourceAvailability = StatusSourceAvailability.NotApplicable,
                Installed = [],
                ManagedFiles = new StatusManagedExtensionFiles
                {
                    Counts = new StatusManagedTargetCounts
                    {
                        Current = zero,
                        Changed = zero,
                        Missing = zero,
                        Unavailable = zero,
                        Blocked = zero,
                    },
                    Targets = [],
                },
            });
    }

    private static StatusFrameworkTarget Project(
        FrameworkManagedTargetObservation target,
        OperationalSourceAvailability sourceAvailability,
        IReadOnlySet<string> currentNavigationPaths)
    {
        return new StatusFrameworkTarget
        {
            Path = target.Path,
            Kind = StatusStateMap.ManagedTargetKind(target.Kind),
            SourceAssetPath = target.SourceAssetPath,
            Region = target.Region,
            State = StatusStateMap.Target(FrameworkGeneratedNavigationCurrentness.ReadState(target, sourceAvailability, currentNavigationPaths)),
            Cause = target.Cause ?? target.Source.Cause,
        };
    }

    private static StatusInstalledExtension Project(InstalledExtensionObservation extension)
    {
        return new StatusInstalledExtension
        {
            Id = extension.Id,
            Version = extension.Version,
            Source = extension.Source,
            SourceAvailability = StatusStateMap.Source(extension.SourceAvailability),
            Dependencies = extension.Dependencies.Order(StringComparer.Ordinal).ToArray(),
            Paths = extension.Paths.Order(StringComparer.Ordinal).ToArray(),
            SourceCause = extension.SourceCause,
        };
    }

    private static StatusManagedExtensionFiles ManagedFiles(ExtensionLifecycleStatusView extensions)
    {
        var targets = extensions.Targets
            .GroupBy(target => target.Path, StringComparer.Ordinal)
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .Select(Project)
            .ToArray();
        var state = extensions.State == OperationalViewState.Complete
            ? StatusValueState.Available
            : StatusValueState.Unavailable;
        return new StatusManagedExtensionFiles
        {
            Counts = new StatusManagedTargetCounts
            {
                Current = Count(targets, StatusTargetState.Current, state),
                Changed = Count(targets, StatusTargetState.Changed, state),
                Missing = Count(targets, StatusTargetState.Missing, state),
                Unavailable = Count(targets, StatusTargetState.Unavailable, state),
                Blocked = Count(targets, StatusTargetState.Blocked, state),
            },
            Targets = targets,
        };
    }

    private static StatusExtensionTarget Project(
        IGrouping<string, ExtensionManagedTargetObservation> observations)
    {
        var ordered = observations.OrderBy(target => target.Owners.FirstOrDefault(), StringComparer.Ordinal).ToArray();
        var first = ordered[0];
        return new StatusExtensionTarget
        {
            Path = observations.Key,
            Owners = ordered.SelectMany(target => target.Owners).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray(),
            State = StatusStateMap.Target(first.State),
            Cause = ordered.Select(target => target.Cause).FirstOrDefault(cause => cause is not null),
        };
    }

    private static StatusIntegerValue Count(
        IReadOnlyList<StatusExtensionTarget> targets,
        StatusTargetState targetState,
        StatusValueState valueState)
        => StatusMeasurementCalculator.Value(
            valueState,
            targets.LongCount(target => target.State == targetState));
}
