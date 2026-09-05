using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Framework.Extensions.Operational.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Operational.Models;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;

namespace OpenForge.Cli.Core.Commands.Status.Shared.Aggregation;

internal static class StatusLifecycleAggregator
{
    internal static StatusLifecycle Build(
        FrameworkLifecycleStatusView framework,
        ExtensionLifecycleStatusView extensions,
        bool absenceProven)
    {
        if (absenceProven)
        {
            return Absent();
        }

        return new StatusLifecycle(
            new StatusFrameworkLifecycle
            {
                State = framework.Lifecycle,
                SourceAvailability = framework.SourceAvailability,
                Targets = framework.Targets
                    .OrderBy(target => target.Path, StringComparer.Ordinal)
                    .Select(Project)
                    .ToArray(),
            },
            new StatusExtensionLifecycle
            {
                State = extensions.Lifecycle,
                SourceAvailability = extensions.SourceAvailability,
                Installed = extensions.Installed
                    .OrderBy(extension => extension.Id, StringComparer.Ordinal)
                    .Select(Project)
                    .ToArray(),
                ManagedFiles = ManagedFiles(extensions),
            });
    }

    internal static StatusLifecycle Unavailable()
    {
        var unavailable = new StatusIntegerValue(OperationalValueState.Unavailable, null);
        return new StatusLifecycle(
            new StatusFrameworkLifecycle
            {
                State = OperationalLifecycleState.Incomplete,
                SourceAvailability = OperationalSourceAvailability.Unavailable,
                Targets = [],
            },
            new StatusExtensionLifecycle
            {
                State = OperationalLifecycleState.Incomplete,
                SourceAvailability = OperationalSourceAvailability.Unavailable,
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
        var zero = new StatusIntegerValue(OperationalValueState.Available, 0L);
        return new StatusLifecycle(
            new StatusFrameworkLifecycle
            {
                State = OperationalLifecycleState.Absent,
                SourceAvailability = OperationalSourceAvailability.NotApplicable,
                Targets = [],
            },
            new StatusExtensionLifecycle
            {
                State = OperationalLifecycleState.Absent,
                SourceAvailability = OperationalSourceAvailability.NotApplicable,
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

    private static StatusFrameworkTarget Project(FrameworkManagedTargetObservation target)
    {
        return new StatusFrameworkTarget
        {
            Path = target.Path,
            Kind = target.Kind,
            SourceAssetPath = target.SourceAssetPath,
            Region = target.Region,
            BaselineFingerprint = target.BaselineFingerprint,
            FingerprintKind = target.FingerprintKind,
            State = target.State,
        };
    }

    private static StatusInstalledExtension Project(InstalledExtensionObservation extension)
    {
        return new StatusInstalledExtension
        {
            Id = extension.Id,
            Version = extension.Version,
            Source = extension.Source,
            SourceAvailability = extension.SourceAvailability,
            Dependencies = extension.Dependencies.Order(StringComparer.Ordinal).ToArray(),
            Paths = extension.Paths.Order(StringComparer.Ordinal).ToArray(),
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
            ? OperationalValueState.Available
            : OperationalValueState.Unavailable;
        return new StatusManagedExtensionFiles
        {
            Counts = new StatusManagedTargetCounts
            {
                Current = Count(targets, OperationalTargetState.Current, state),
                Changed = Count(targets, OperationalTargetState.Changed, state),
                Missing = Count(targets, OperationalTargetState.Missing, state),
                Unavailable = Count(targets, OperationalTargetState.Unavailable, state),
                Blocked = Count(targets, OperationalTargetState.Blocked, state),
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
            BaselineFingerprint = first.BaselineFingerprint,
            FingerprintKind = first.FingerprintKind,
            State = first.State,
        };
    }

    private static StatusIntegerValue Count(
        IReadOnlyList<StatusExtensionTarget> targets,
        OperationalTargetState targetState,
        OperationalValueState valueState)
        => StatusMeasurementCalculator.Value(
            valueState,
            targets.LongCount(target => target.State == targetState));
}
