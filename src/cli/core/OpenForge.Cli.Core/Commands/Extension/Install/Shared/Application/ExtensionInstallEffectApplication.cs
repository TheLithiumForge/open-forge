using OpenForge.Cli.Core.Commands.Extension.Install.Models.Application;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Framework.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Recovery.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Install.Shared.Application;

internal sealed class ExtensionInstallEffectApplication(
    DirectoryCreationApplier directoryApplier,
    FileChangeApplier fileApplier,
    ExtensionInstallAppliedVerifier verifier)
{
    private readonly DirectoryCreationApplier _directoryApplier = directoryApplier;
    private readonly FileChangeApplier _fileApplier = fileApplier;
    private readonly ExtensionInstallAppliedVerifier _verifier = verifier;

    internal async ValueTask<ExtensionInstallApplicationStageResult> ApplyAsync(
        ExtensionInstallEffectApplicationInput input,
        CancellationToken cancellationToken)
    {
        var progress = input.Progress;
        var checkIndex = 0;
        for (var effectIndex = 0; effectIndex < input.Plan.Effects.Count; effectIndex++)
        {
            var effect = input.Plan.Effects[effectIndex];
            ExtensionInstallEffectOutcome? recordedOutcome;
            ExtensionInstallFinding? finding;
            try
            {
                if (effect.DirectoryCreation is { } creation)
                {
                    var receipt = await _directoryApplier.ApplyAsync(
                        input.Lease,
                        creation,
                        input.Validation.Checks[checkIndex++],
                        cancellationToken).ConfigureAwait(false);
                    var mapping = MapDirectoryReceipt(receipt, effect.Result.Path);
                    recordedOutcome = mapping.RecordedOutcome;
                    finding = mapping.Finding;
                }
                else if (effect.FileChange is { } change)
                {
                    var receipt = await _fileApplier.ApplyAsync(
                        input.Lease,
                        change,
                        input.Validation.Checks[checkIndex++],
                        PreparationFor(change, progress.RecoveryPreparation),
                        cancellationToken).ConfigureAwait(false);
                    var mapping = MapFileReceipt(receipt, effect.Result.Path);
                    recordedOutcome = mapping.RecordedOutcome;
                    finding = mapping.Finding;
                }
                else
                {
                    throw new InvalidOperationException(
                        "Every Extension Install effect requires one typed filesystem change.");
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                progress = progress.RecordEffect(
                    effectIndex,
                    ExtensionInstallEffectOutcome.CompletionUnknown);
                return Stop(
                    progress,
                    ExtensionInstallFindingCode.Interrupted,
                    "Extension Install effect application was interrupted.",
                    effect.Result.Path);
            }
            catch (Exception)
            {
                progress = progress.RecordEffect(
                    effectIndex,
                    ExtensionInstallEffectOutcome.CompletionUnknown);
                return Stop(
                    progress,
                    ExtensionInstallFindingCode.OperationFailed,
                    "Extension Install effect application failed unexpectedly.",
                    effect.Result.Path);
            }

            if (recordedOutcome is { } outcome)
            {
                progress = progress.RecordEffect(effectIndex, outcome);
            }

            if (finding is not null)
            {
                return new ExtensionInstallApplicationStageResult(
                    progress.RetainWorkspaceEffects(),
                    finding);
            }
        }

        ExtensionInstallVerificationResult topology;
        try
        {
            topology = await _verifier.VerifyTopologyAsync(
                input.Plan,
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Stop(
                progress,
                ExtensionInstallFindingCode.Interrupted,
                "Extension Install target-topology verification was interrupted.");
        }
        catch (Exception)
        {
            return Stop(
                progress,
                ExtensionInstallFindingCode.TopologyVerificationFailed,
                "Extension Install target-topology verification failed unexpectedly.");
        }

        progress = progress.WithVerification(topology.Verification);
        if (topology.Finding is not null)
        {
            return new ExtensionInstallApplicationStageResult(
                progress.RetainWorkspaceEffects(),
                topology.Finding);
        }

        if (input.Plan.LifecycleChange is { } lifecycleChange)
        {
            FileChangeReceipt receipt;
            try
            {
                receipt = await _fileApplier.ApplyAsync(
                    input.Lease,
                    lifecycleChange,
                    input.Validation.Checks[checkIndex],
                    PreparationFor(lifecycleChange, progress.RecoveryPreparation),
                    cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return Stop(
                    progress.WithLifecycle(ExtensionInstallLifecycleOutcome.CompletionUnknown),
                    ExtensionInstallFindingCode.Interrupted,
                    "Extension lifecycle publication was interrupted.",
                    ".agents/open-forge.lifecycle.json");
            }
            catch (Exception)
            {
                return Stop(
                    progress.WithLifecycle(ExtensionInstallLifecycleOutcome.CompletionUnknown),
                    ExtensionInstallFindingCode.OperationFailed,
                    "Extension lifecycle publication failed unexpectedly.",
                    ".agents/open-forge.lifecycle.json");
            }

            var lifecycleOutcome = ReadLifecycleOutcome(receipt);
            progress = progress.WithLifecycle(lifecycleOutcome);
            if (lifecycleOutcome != ExtensionInstallLifecycleOutcome.Verified)
            {
                return new ExtensionInstallApplicationStageResult(
                    progress
                        .WithVerification(progress.Verification with
                        {
                            ExtensionsLifecycle = ReadLifecycleVerificationState(receipt.NotStartedReason),
                        })
                        .RetainWorkspaceEffects(),
                    new ExtensionInstallFinding(
                        ReadLifecycleFindingCode(receipt.NotStartedReason),
                        receipt.Cause
                            ?? "The Extension lifecycle could not be published and verified.",
                        ".agents/open-forge.lifecycle.json"));
            }
        }

        ExtensionInstallVerificationResult final;
        try
        {
            final = await _verifier.VerifyFinalAsync(input.Plan, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Stop(
                progress,
                ExtensionInstallFindingCode.Interrupted,
                "Final Extension Install verification was interrupted.");
        }
        catch (Exception)
        {
            return Stop(
                progress,
                ExtensionInstallFindingCode.VerificationFailed,
                "Final Extension Install verification failed unexpectedly.");
        }

        progress = progress.WithVerification(final.Verification);
        return final.IsVerified
            ? new ExtensionInstallApplicationStageResult(progress, Finding: null)
            : new ExtensionInstallApplicationStageResult(
                progress.RetainWorkspaceEffects(),
                final.Finding
                    ?? new ExtensionInstallFinding(
                        ExtensionInstallFindingCode.VerificationFailed,
                        "Final Extension Install verification did not reach a verified state."));
    }

    private static RecoveryBundlePreparation? PreparationFor(
        PlannedFileChange change,
        RecoveryBundlePreparation? preparation)
        => change.Kind == PlannedFileChangeKind.Create ? null : preparation;

    private static ExtensionInstallEffectOutcome ReadOutcome(
        DirectoryCreationReceipt receipt)
        => ReadOutcome(receipt.EffectState, receipt.VerificationState);

    private static ExtensionInstallEffectOutcome ReadOutcome(FileChangeReceipt receipt)
        => ReadOutcome(receipt.EffectState, receipt.VerificationState);

    private static ExtensionInstallEffectOutcome ReadOutcome(
        FilesystemEffectState effect,
        FilesystemVerificationState verification)
    {
        EnsureDefined(effect);
        EnsureDefined(verification);
        return effect switch
        {
            FilesystemEffectState.NotStarted => verification switch
            {
                FilesystemVerificationState.NotStarted => ExtensionInstallEffectOutcome.NotStarted,
                FilesystemVerificationState.Verified
                    or FilesystemVerificationState.Failed => throw InconsistentReceipt(
                        effect,
                        verification),
                _ => throw Undefined(nameof(verification), verification),
            },
            FilesystemEffectState.Applied => verification switch
            {
                FilesystemVerificationState.NotStarted => throw InconsistentReceipt(effect, verification),
                FilesystemVerificationState.Verified => ExtensionInstallEffectOutcome.Verified,
                FilesystemVerificationState.Failed => ExtensionInstallEffectOutcome.VerificationFailed,
                _ => throw Undefined(nameof(verification), verification),
            },
            FilesystemEffectState.Unknown => verification switch
            {
                FilesystemVerificationState.NotStarted => ExtensionInstallEffectOutcome.CompletionUnknown,
                FilesystemVerificationState.Verified
                    or FilesystemVerificationState.Failed => throw InconsistentReceipt(
                        effect,
                        verification),
                _ => throw Undefined(nameof(verification), verification),
            },
            _ => throw Undefined(nameof(effect), effect),
        };
    }

    private static ExtensionInstallLifecycleOutcome ReadLifecycleOutcome(
        FileChangeReceipt receipt)
        => ReadOutcome(receipt) switch
        {
            ExtensionInstallEffectOutcome.Planned => ExtensionInstallLifecycleOutcome.Planned,
            ExtensionInstallEffectOutcome.NotStarted => ExtensionInstallLifecycleOutcome.NotStarted,
            ExtensionInstallEffectOutcome.Verified => ExtensionInstallLifecycleOutcome.Verified,
            ExtensionInstallEffectOutcome.VerificationFailed => ExtensionInstallLifecycleOutcome.VerificationFailed,
            ExtensionInstallEffectOutcome.CompletionUnknown => ExtensionInstallLifecycleOutcome.CompletionUnknown,
            _ => throw new ArgumentOutOfRangeException(
                nameof(receipt),
                ReadOutcome(receipt),
                "The Extension effect outcome is not defined."),
        };

    internal static ExtensionInstallEffectReceiptMapping MapDirectoryReceipt(
        DirectoryCreationReceipt receipt,
        string path)
    {
        var outcome = ReadOutcome(receipt);
        ExtensionInstallFinding? finding = null;
        if (receipt.EffectState != FilesystemEffectState.Applied
            || receipt.VerificationState != FilesystemVerificationState.Verified)
        {
            finding = new ExtensionInstallFinding(
                ReadFilesystemFindingCode(
                    receipt.NotStartedReason,
                    receipt.VerificationState),
                receipt.Cause ?? "An Extension Install directory could not be created and verified.",
                path);
        }

        return new ExtensionInstallEffectReceiptMapping(
            outcome == ExtensionInstallEffectOutcome.NotStarted ? null : outcome,
            finding);
    }

    private static ExtensionInstallEffectReceiptMapping MapFileReceipt(
        FileChangeReceipt receipt,
        string path)
    {
        if (receipt.EffectState == FilesystemEffectState.Applied
            && receipt.VerificationState == FilesystemVerificationState.Verified)
        {
            return new ExtensionInstallEffectReceiptMapping(
                ExtensionInstallEffectOutcome.Verified,
                Finding: null);
        }

        var code = ReadFilesystemFindingCode(
            receipt.NotStartedReason,
            receipt.VerificationState);
        var outcome = ReadOutcome(receipt);
        return new ExtensionInstallEffectReceiptMapping(
            outcome == ExtensionInstallEffectOutcome.NotStarted ? null : outcome,
            new ExtensionInstallFinding(
                code,
                receipt.Cause ?? "An Extension Install target could not be applied and verified.",
                path));
    }

    private static ExtensionInstallFindingCode ReadFilesystemFindingCode(
        FilesystemNotStartedReason? reason,
        FilesystemVerificationState verification)
    {
        EnsureDefined(verification);
        if (reason is { } notStartedReason)
        {
            return notStartedReason switch
            {
                FilesystemNotStartedReason.Cancelled => ExtensionInstallFindingCode.Interrupted,
                FilesystemNotStartedReason.TargetChanged => ExtensionInstallFindingCode.TargetChanged,
                FilesystemNotStartedReason.ApplicationFailed => ExtensionInstallFindingCode.WriteFailed,
                FilesystemNotStartedReason.ContractRejected => ExtensionInstallFindingCode.OperationFailed,
                _ => throw Undefined(nameof(reason), notStartedReason),
            };
        }

        return verification switch
        {
            FilesystemVerificationState.Failed => ExtensionInstallFindingCode.VerificationFailed,
            FilesystemVerificationState.NotStarted
                or FilesystemVerificationState.Verified => ExtensionInstallFindingCode.WriteFailed,
            _ => throw Undefined(nameof(verification), verification),
        };
    }

    private static ExtensionInstallFindingCode ReadLifecycleFindingCode(
        FilesystemNotStartedReason? reason)
    {
        if (reason is null)
        {
            return ExtensionInstallFindingCode.LifecyclePublicationFailed;
        }

        return reason.Value switch
        {
            FilesystemNotStartedReason.Cancelled => ExtensionInstallFindingCode.Interrupted,
            FilesystemNotStartedReason.TargetChanged
                or FilesystemNotStartedReason.ApplicationFailed
                or FilesystemNotStartedReason.ContractRejected => ExtensionInstallFindingCode.LifecyclePublicationFailed,
            _ => throw Undefined(nameof(reason), reason.Value),
        };
    }

    private static ExtensionInstallVerificationState ReadLifecycleVerificationState(
        FilesystemNotStartedReason? reason)
        => ReadLifecycleFindingCode(reason) == ExtensionInstallFindingCode.Interrupted
            ? ExtensionInstallVerificationState.Unknown
            : ExtensionInstallVerificationState.Failed;

    private static void EnsureDefined(FilesystemEffectState value)
    {
        _ = value switch
        {
            FilesystemEffectState.NotStarted
                or FilesystemEffectState.Applied
                or FilesystemEffectState.Unknown => true,
            _ => throw Undefined(nameof(value), value),
        };
    }

    private static void EnsureDefined(FilesystemVerificationState value)
    {
        _ = value switch
        {
            FilesystemVerificationState.NotStarted
                or FilesystemVerificationState.Verified
                or FilesystemVerificationState.Failed => true,
            _ => throw Undefined(nameof(value), value),
        };
    }

    private static ArgumentOutOfRangeException Undefined<T>(string name, T value)
        where T : struct, Enum
        => new(name, value, $"The Extension Install {name} value is not defined.");

    private static InvalidOperationException InconsistentReceipt(
        FilesystemEffectState effect,
        FilesystemVerificationState verification)
        => new(
            $"The Extension Install filesystem receipt pair '{effect}/{verification}' is internally inconsistent.");

    private static ExtensionInstallApplicationStageResult Stop(
        ExtensionInstallApplicationProgress progress,
        ExtensionInstallFindingCode code,
        string cause,
        string? target = null)
        => new(
            progress.RetainWorkspaceEffects(),
            new ExtensionInstallFinding(code, cause, target));
}

internal sealed record ExtensionInstallEffectReceiptMapping(
    ExtensionInstallEffectOutcome? RecordedOutcome,
    ExtensionInstallFinding? Finding);
