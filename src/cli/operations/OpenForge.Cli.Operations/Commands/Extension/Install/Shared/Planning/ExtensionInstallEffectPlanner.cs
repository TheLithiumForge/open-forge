using OpenForge.Cli.Core.Framework.Ownership.Models;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;

namespace OpenForge.Cli.Core.Commands.Extension.Install.Shared.Planning;

internal sealed class ExtensionInstallEffectPlanner(
    WorkspaceOwnershipStore ownershipStore,
    FileExpectationValidator validator)
{
    private readonly WorkspaceOwnershipStore _ownershipStore = ownershipStore;
    private readonly FileExpectationValidator _validator = validator;

    internal async ValueTask<ExtensionInstallEffectPlan> BuildAsync(
        ExtensionInstallEffectPlanningInput input,
        CancellationToken cancellationToken)
    {
        var effects = new List<ExtensionInstallPlannedEffect>();
        var createdDirectories = new HashSet<string>(StringComparer.Ordinal);
        var mutationPaths = input.Packages
            .SelectMany(package => package.Payload)
            .Select(file => file.TargetPath
                ?? throw new InvalidDataException(
                    "A planned Extension package file requires its normalized target path."))
            .ToHashSet(StringComparer.Ordinal);
        foreach (var path in mutationPaths.Order(StringComparer.Ordinal))
        {
            var parent = Path.GetDirectoryName(path.Replace('/', Path.DirectorySeparatorChar));
            while (!string.IsNullOrEmpty(parent)
                && parent.Replace(Path.DirectorySeparatorChar, '/') != ".agents")
            {
                var canonical = parent.Replace(Path.DirectorySeparatorChar, '/');
                var observation = await ObserveAsync(input.Request, canonical, cancellationToken)
                    .ConfigureAwait(false);
                if (observation.Finding is not null)
                {
                    return Stop(observation.Finding);
                }

                var snapshot = observation.Snapshot
                    ?? throw new InvalidOperationException(
                        "A complete Extension directory observation requires its snapshot.");
                if (snapshot.Kind == FileExpectationKind.Missing)
                {
                    _ = createdDirectories.Add(canonical);
                }
                else if (snapshot.Kind != FileExpectationKind.Directory)
                {
                    return Stop(
                        ExtensionInstallFindingCode.TargetUnsafe,
                        "A required Extension target parent is not an ordinary directory.",
                        canonical);
                }

                parent = Path.GetDirectoryName(parent);
            }
        }

        foreach (var directory in createdDirectories
                     .OrderBy(value => value.Count(character => character == '/'))
                     .ThenBy(value => value, StringComparer.Ordinal))
        {
            var logical = Logical(input.Request, directory);
            effects.Add(new ExtensionInstallPlannedEffect
            {
                Result = new ExtensionInstallEffect(
                    directory,
                    PackageId: null,
                    ExtensionInstallEffectKind.Directory,
                    ExtensionInstallEffectAction.Create,
                    ExtensionInstallEffectOutcome.Planned,
                    ExtensionInstallEffectResidual.None),
                DirectoryCreation = PlannedDirectoryCreation.Create(FileExpectation.Missing(logical)),
            });
        }

        var plannedPaths = new HashSet<string>(StringComparer.Ordinal);
        foreach (var package in input.Packages)
        {
            foreach (var file in package.Payload.OrderBy(value => value.TargetPath, StringComparer.Ordinal))
            {
                var path = file.TargetPath
                    ?? throw new InvalidDataException(
                        "A planned Extension package file requires its normalized target path.");
                if (!plannedPaths.Add(path))
                {
                    continue;
                }

                var before = input.TargetState.Observations[path];
                var intended = input.Topology.IntendedTargetBytes[path];
                if (before.Kind == FileExpectationKind.File
                    && string.Equals(
                        FileExpectation.Hash(before.Bytes.AsSpan()),
                        FileExpectation.Hash(intended.AsSpan()),
                        StringComparison.Ordinal))
                {
                    continue;
                }

                var change = before.Kind == FileExpectationKind.Missing
                    ? PlannedFileChange.Create(before.Expectation, intended.AsSpan())
                    : PlannedFileChange.Replace(before.Expectation, intended.AsSpan());
                effects.Add(new ExtensionInstallPlannedEffect
                {
                    Result = new ExtensionInstallEffect(
                        path,
                        package.Id,
                        ExtensionInstallEffectKind.PackageFile,
                        before.Kind == FileExpectationKind.Missing
                            ? ExtensionInstallEffectAction.Create
                            : ExtensionInstallEffectAction.Replace,
                        ExtensionInstallEffectOutcome.Planned,
                        ExtensionInstallEffectResidual.None),
                    FileChange = change,
                    RecoveryTarget = RecoveryBundleTarget.Create(change, before),
                });
            }
        }

        foreach (var generated in input.Topology.GeneratedTargetBytes.OrderBy(
                     pair => pair.Key,
                     StringComparer.Ordinal))
        {
            var observation = await ObserveAsync(input.Request, generated.Key, cancellationToken)
                .ConfigureAwait(false);
            if (observation.Finding is not null
                || observation.Snapshot?.Kind != FileExpectationKind.File)
            {
                return Stop(observation.Finding ?? new ExtensionInstallFinding(
                    ExtensionInstallFindingCode.GeneratedRegionUnsafe,
                    "A generated region host is not an ordinary file.",
                    generated.Key));
            }

            var before = observation.Snapshot;
            if (before.Bytes.AsSpan().SequenceEqual(generated.Value.AsSpan()))
            {
                continue;
            }

            var change = PlannedFileChange.ReplaceGeneratedRegion(
                before.Expectation,
                generated.Value.AsSpan());
            effects.Add(new ExtensionInstallPlannedEffect
            {
                Result = new ExtensionInstallEffect(
                    generated.Key,
                    PackageId: null,
                    ExtensionInstallEffectKind.GeneratedRegion,
                    ExtensionInstallEffectAction.Replace,
                    ExtensionInstallEffectOutcome.Planned,
                    ExtensionInstallEffectResidual.None),
                FileChange = change,
                RecoveryTarget = RecoveryBundleTarget.Create(change, before),
            });
        }

        var ownershipPlan = _ownershipStore.PlanExtensionOwnership(
            input.Ownership, input.TargetState.IntendedExtensions);
        RecoveryBundleTarget? ownershipRecovery = null;
        if (ownershipPlan.Change is { } ownershipChange)
        {
            ownershipRecovery = RecoveryBundleTarget.Create(
                ownershipChange,
                input.Ownership.Snapshot
                    ?? throw new InvalidOperationException(
                        "An Extension ownership write plan requires exact prior file facts."));
        }

        return new ExtensionInstallEffectPlan(
            new ReadOnlyCollection<ExtensionInstallPlannedEffect>([.. effects]),
            ownershipPlan.Change,
            ownershipRecovery,
            ownershipPlan.State switch
            {
                OwnershipWritePlanState.Planned => ExtensionInstallLifecycleAction.Publish,
                OwnershipWritePlanState.Unchanged => ExtensionInstallLifecycleAction.Preserve,
                OwnershipWritePlanState.Skipped => ExtensionInstallLifecycleAction.None,
                _ => throw new InvalidOperationException("The ownership write plan state is not defined."),
            },
            finding: null);
    }

    private async ValueTask<ExtensionInstallTargetObservation> ObserveAsync(
        ExtensionInstallRequest request,
        string relativePath,
        CancellationToken cancellationToken)
    {
        var check = await _validator.ValidateAsync(
            request.Workspace,
            FileExpectation.Missing(Logical(request, relativePath)),
            cancellationToken).ConfigureAwait(false);
        if (check.State == FileExpectationValidationState.Cancelled)
        {
            return ExtensionInstallTargetObservation.Stop(new ExtensionInstallFinding(
                ExtensionInstallFindingCode.Interrupted,
                "Extension effect planning was interrupted.",
                relativePath));
        }

        if (check.State is FileExpectationValidationState.Blocked
            or FileExpectationValidationState.Failed
            || check.Actual is null)
        {
            return ExtensionInstallTargetObservation.Stop(new ExtensionInstallFinding(
                ExtensionInstallFindingCode.TargetUnsafe,
                check.Cause ?? check.Failure?.DirectCause ?? "The Extension target is unsafe or unavailable.",
                relativePath));
        }

        return ExtensionInstallTargetObservation.Complete(check.Actual);
    }

    private static string Logical(ExtensionInstallRequest request, string relative)
        => Path.GetFullPath(Path.Combine(
            request.Workspace.LexicalRoot,
            relative.Replace('/', Path.DirectorySeparatorChar)));

    private static ExtensionInstallEffectPlan Stop(
        ExtensionInstallFindingCode code,
        string cause,
        string? target = null)
        => Stop(new ExtensionInstallFinding(code, cause, target));

    private static ExtensionInstallEffectPlan Stop(ExtensionInstallFinding finding)
        => new(
            effects: [],
            ownershipChange: null,
            ownershipRecoveryTarget: null,
            ExtensionInstallLifecycleAction.None,
            finding);
}
