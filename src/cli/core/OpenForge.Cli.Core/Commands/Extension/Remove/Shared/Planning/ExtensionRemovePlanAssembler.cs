using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Effects;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Serialization;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Recovery.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Remove.Shared.Planning;

internal static class ExtensionRemovePlanAssembler
{
    internal static ExtensionLifecycleState CreateIntendedLifecycle(
        ExtensionLifecycleState current,
        IReadOnlySet<string> selected)
        => new()
        {
            Coverage = current.Coverage,
            Packages = current.Packages
                .Where(package => !selected.Contains(package.Id))
                .Select(package => new LifecycleExtensionPackageV1
                {
                    Id = package.Id,
                    Version = package.Version,
                    Source = package.Source,
                    Dependencies = package.Dependencies.ToArray(),
                    Paths = package.Paths.ToArray(),
                }).ToArray(),
            Paths = current.Paths
                .Select(path => new LifecycleExtensionPathV1
                {
                    Path = path.Path,
                    Owners = path.Owners.Where(owner => !selected.Contains(owner)).ToArray(),
                    BaselineFingerprint = path.BaselineFingerprint,
                    FingerprintKind = path.FingerprintKind,
                })
                .Where(path => path.Owners.Length > 0)
                .ToArray(),
        };

    internal static IReadOnlyList<ExtensionRemovePlannedEffect> BuildEffects(
        IReadOnlyList<ExtensionRemovePathObservation> observations,
        IReadOnlyList<ExtensionRemovePathPlan> paths,
        ExtensionRemoveTopologyBuild topology)
    {
        var plans = paths.ToDictionary(path => path.Path, StringComparer.Ordinal);
        var effects = observations
            .Where(observation => plans[observation.Path].Action == ExtensionRemovePathAction.Delete)
            .OrderBy(observation => observation.Path, StringComparer.Ordinal)
            .Select(CreateDeleteEffect)
            .ToList();
        effects.AddRange(topology.Changes.Select(change => new ExtensionRemovePlannedEffect
        {
            Result = new ExtensionRemoveEffect(
                change.Path,
                packageId: null,
                ExtensionRemoveEffectKind.GeneratedRegion,
                ExtensionRemoveEffectAction.ReleaseOwnership,
                ExtensionRemoveEffectOutcome.Planned,
                ExtensionRemoveEffectResidual.None),
            FileChange = change.Change,
            RecoveryTarget = RecoveryBundleTarget.Create(change.Change, change.Before),
        }));
        return effects;
    }

    internal static ExtensionRemovePlanningDisposition ReadDisposition(
        ExtensionRemovePathAction action)
        => action switch
        {
            ExtensionRemovePathAction.RetainShared
                or ExtensionRemovePathAction.KeepAsUnmanaged => ExtensionRemovePlanningDisposition.Retain,
            ExtensionRemovePathAction.Delete => ExtensionRemovePlanningDisposition.Delete,
            ExtensionRemovePathAction.ReleaseOwnership => ExtensionRemovePlanningDisposition.ReleaseOwnership,
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, "The path action is not defined."),
        };

    private static ExtensionRemovePlannedEffect CreateDeleteEffect(
        ExtensionRemovePathObservation observation)
    {
        var snapshot = observation.Snapshot
            ?? throw new InvalidOperationException("A deletion requires an exact target snapshot.");
        var change = PlannedFileChange.Delete(snapshot.Expectation);
        return new ExtensionRemovePlannedEffect
        {
            Result = new ExtensionRemoveEffect(
                observation.Path,
                observation.SelectedOwnerIds[0],
                ExtensionRemoveEffectKind.PackageFile,
                ExtensionRemoveEffectAction.Delete,
                ExtensionRemoveEffectOutcome.Planned,
                ExtensionRemoveEffectResidual.None),
            FileChange = change,
            RecoveryTarget = RecoveryBundleTarget.Create(change, snapshot),
        };
    }
}
