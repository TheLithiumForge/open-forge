using OpenForge.Cli.Core.Commands.Library.Models.Permissions;
using OpenForge.Cli.Core.Commands.Library.Shared.Permissions;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Library.Models.Application;
using OpenForge.Cli.Core.Framework.Filesystem.LogicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.RelativeFileLinks;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;

namespace OpenForge.Cli.Core.Commands.Library.Shared.Application;

internal static class LibraryMutationApplicationRunner
{
    internal static async ValueTask<LibraryExecutionEvidence> ApplyAsync(
        LibraryMutationApplicationRequest input,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (!input.Lease.IsHeldFor(input.Lease.Request.Workspace))
        {
            throw new ArgumentException("Library application requires a live same-workspace lease.", nameof(input));
        }

        if (input.SettingsParentDirectories.IsDefault
            || input.Directories.IsDefault
            || input.Links.IsDefault
            || input.GeneratedRegions.IsDefault
            || input.ProtectedSourceRoots.IsDefault)
        {
            throw new ArgumentException("Library application requires initialized effect collections.", nameof(input));
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Stop(input, new LibraryCancellationFact(LibraryExecutionStage.Preflight), unexpected: null);
        }

        var reversibleEffects = input.Links.Length
            + input.GeneratedRegions.Length
            + (input.OwnershipChange is null ? 0 : 1)
            + (input.Permissions?.Change is null ? 0 : 1);
        if (reversibleEffects > 0 && !MatchesRecovery(input))
        {
            return Stop(
                input,
                cancellation: null,
                new LibraryUnexpectedFailureFact(
                    LibraryExecutionStage.RecoveryPreparation,
                    "The complete Library plan does not have one exact verified recovery preparation."));
        }

        if (!ProtectsSources(input))
        {
            return Stop(input, cancellation: null, new LibraryUnexpectedFailureFact(
                LibraryExecutionStage.Preflight, "A Library mutation target overlaps a protected source tree."));
        }
        if (input.Permissions is { } permissions && (permissions.Failure is not null
            || !await LibraryPermissionOperation.RevalidateAsync(input.Lease, permissions, cancellationToken).ConfigureAwait(false)))
        {
            var stopped = Stop(input, cancellation: null, unexpected: null) with
            {
                Permission = new(permissions.Result, Receipt: null, permissions.Failure ?? LibraryPermissionFailure.Changed),
            };
            return stopped;
        }

        var resolver = new PhysicalPathResolver();
        var validator = new FileExpectationValidator(resolver);
        var revalidator = new MutationRevalidator(validator);
        var fileChanges = input.GeneratedRegions
            .Concat(input.OwnershipChange is null ? [] : [input.OwnershipChange])
            .ToImmutableArray();
        var plannedDirectories = input.SettingsParentDirectories.Concat(input.Directories).ToImmutableArray();
        MutationValidationResult fileValidation = plannedDirectories.Length == 0 && fileChanges.Length == 0
            ? MutationValidationResult.Valid()
            : await revalidator.ValidateAsync(
                input.Lease,
                plannedDirectories,
                fileChanges,
                cancellationToken).ConfigureAwait(false);
        if (fileValidation.State != MutationValidationState.Valid)
        {
            return fileValidation.State == MutationValidationState.Cancelled
                ? Stop(input, new LibraryCancellationFact(LibraryExecutionStage.Preflight), unexpected: null)
                : Stop(
                    input,
                    cancellation: null,
                    new LibraryUnexpectedFailureFact(
                        LibraryExecutionStage.Preflight,
                        fileValidation.Cause ?? "A planned Library file target changed or became unsafe."));
        }

        var linkChecks = ImmutableArray.CreateBuilder<RelativeFileLinkValidationResult>(input.Links.Length);
        var linkPreflight = new LibraryLinkPreflight(input.Lease, plannedDirectories);
        foreach (var link in input.Links)
        {
            var check = await linkPreflight.ValidateAsync(
                link,
                cancellationToken).ConfigureAwait(false);
            linkChecks.Add(check);
            if (check.State != RelativeFileLinkValidationState.Matched)
            {
                return check.State == RelativeFileLinkValidationState.Cancelled
                    ? Stop(input, new LibraryCancellationFact(LibraryExecutionStage.Preflight), unexpected: null)
                    : Stop(
                        input,
                        cancellation: null,
                        new LibraryUnexpectedFailureFact(
                            LibraryExecutionStage.Preflight,
                            check.Cause ?? "A planned Library link target changed or became unsafe."));
            }
        }

        var directoryApplier = new DirectoryCreationApplier(revalidator, validator);
        var fileApplier = new FileChangeApplier(revalidator, validator);
        var directoryReceipts = ImmutableArray.CreateBuilder<DirectoryCreationReceipt>();
        var linkReceipts = ImmutableArray.CreateBuilder<RelativeFileLinkReceipt>();
        var generatedReceipts = ImmutableArray.CreateBuilder<FileChangeReceipt>();
        FileChangeReceipt? recordReceipt = null;
        var attempted = ImmutableArray.CreateBuilder<CanonicalRelativePath>();
        LibraryCancellationFact? cancellation = null;
        LibraryUnexpectedFailureFact? unexpected = null;
        var checkIndex = 0;

        for (var index = 0; index < input.SettingsParentDirectories.Length && cancellation is null && unexpected is null; index++)
        {
            var creation = input.SettingsParentDirectories[index];
            attempted.Add(Relative(input.Lease, creation.LogicalPath));
            var receipt = await directoryApplier.ApplyAsync(
                input.Lease,
                creation,
                fileValidation.Checks[checkIndex++],
                cancellationToken).ConfigureAwait(false);
            directoryReceipts.Add(receipt);
            if (!Verified(receipt.EffectState, receipt.VerificationState))
            {
                ReadStop(receipt.NotStartedReason, receipt.Cause, out cancellation, out unexpected);
                break;
            }
        }

        LibraryPermissionApplication? permissionApplication = null;
        if (cancellation is null && unexpected is null && input.Permissions is { } permissionStage)
        {
            if (permissionStage.Change is { } permissionChange)
            {
                attempted.Add(Relative(input.Lease, permissionChange.LogicalPath));
            }
            permissionApplication = await LibraryPermissionOperation.ApplyAsync(
                input.Lease, permissionStage, input.RecoveryPreparation, cancellationToken).ConfigureAwait(false);
            if (permissionApplication.Failure is { } permissionFailure)
            {
                if (permissionFailure == LibraryPermissionFailure.Interrupted)
                {
                    cancellation = new LibraryCancellationFact(LibraryExecutionStage.Application);
                }
                else
                {
                    unexpected = new LibraryUnexpectedFailureFact(LibraryExecutionStage.Application,
                        "The Library permission write could not be applied and verified.");
                }
            }
        }

        for (var index = 0; index < input.Directories.Length && cancellation is null && unexpected is null; index++)
        {
            var creation = input.Directories[index];
            attempted.Add(Relative(input.Lease, creation.LogicalPath));
            var receipt = await directoryApplier.ApplyAsync(
                input.Lease,
                creation,
                fileValidation.Checks[checkIndex++],
                cancellationToken).ConfigureAwait(false);
            directoryReceipts.Add(receipt);
            if (!Verified(receipt.EffectState, receipt.VerificationState))
            {
                ReadStop(receipt.NotStartedReason, receipt.Cause, out cancellation, out unexpected);
                break;
            }
        }

        if (cancellation is null && unexpected is null)
        {
            for (var index = 0; index < input.Links.Length; index++)
            {
                var effect = input.Links[index];
                attempted.Add(effect.DestinationPath);
                var expected = linkChecks[index].Actual
                    ?? throw new InvalidOperationException("A matched Library link check requires an actual observation.");
                var receipt = await RelativeFileLinkApplier.ApplyAsync(
                    resolver,
                    input.Lease,
                    new RelativeFileLinkApplicationInput
                    {
                        Effect = effect,
                        Expected = expected,
                        RecoveryPreparation = input.RecoveryPreparation,
                    },
                    cancellationToken).ConfigureAwait(false);
                linkReceipts.Add(receipt);
                if (!Verified(receipt.EffectState, receipt.VerificationState))
                {
                    ReadStop(receipt.NotStartedReason, receipt.Cause, out cancellation, out unexpected);
                    break;
                }
            }
        }

        if (cancellation is null && unexpected is null)
        {
            foreach (var change in input.GeneratedRegions)
            {
                attempted.Add(Relative(input.Lease, change.LogicalPath));
                var receipt = await fileApplier.ApplyAsync(
                    input.Lease,
                    change,
                    fileValidation.Checks[checkIndex++],
                    input.RecoveryPreparation,
                    cancellationToken).ConfigureAwait(false);
                generatedReceipts.Add(receipt);
                if (!Verified(receipt.EffectState, receipt.VerificationState))
                {
                    ReadStop(receipt.NotStartedReason, receipt.Cause, out cancellation, out unexpected);
                    break;
                }
            }
        }

        var publicationOrder = LibraryRecordPublicationOrder.NotObserved;
        if (cancellation is null && unexpected is null && input.OwnershipChange is { } recordChange)
        {
            attempted.Add(Relative(input.Lease, recordChange.LogicalPath));
            recordReceipt = await fileApplier.ApplyAsync(
                input.Lease,
                recordChange,
                fileValidation.Checks[checkIndex++],
                input.RecoveryPreparation,
                cancellationToken).ConfigureAwait(false);
            if (Verified(recordReceipt.EffectState, recordReceipt.VerificationState))
            {
                publicationOrder = LibraryRecordPublicationOrder.Last;
            }
            else
            {
                ReadStop(recordReceipt.NotStartedReason, recordReceipt.Cause, out cancellation, out unexpected);
            }
        }

        var execution = new LibraryExecutionEvidence
        {
            Permission = permissionApplication,
            Directories = directoryReceipts.ToImmutable(),
            Links = linkReceipts.ToImmutable(),
            GeneratedRegions = generatedReceipts.ToImmutable(),
            Record = recordReceipt,
            RecoveryPreparation = input.RecoveryPreparation,
            RecoveryCleanup = null,
            Cancellation = cancellation,
            UnexpectedFailure = unexpected,
            SourceEffectScope = new LibrarySourceEffectScopeFacts
            {
                ProtectedSourceRoots = input.ProtectedSourceRoots,
                AttemptedMutationTargets = attempted.ToImmutable(),
                IsComplete = true,
            },
            RecordPublicationOrder = publicationOrder,
        };
        return execution;
    }

    private static bool MatchesRecovery(LibraryMutationApplicationRequest input)
    {
        if (input.RecoveryPreparation is not { } preparation)
        {
            return false;
        }

        return (input.Permissions?.Change is not { } permission || preparation.MatchesChange(input.Lease.Request, permission))
            && input.Links.All(effect => preparation.MatchesRelativeFileLink(input.Lease.Request, effect))
            && input.GeneratedRegions.All(change => preparation.MatchesChange(input.Lease.Request, change))
            && (input.OwnershipChange is null
                || preparation.MatchesChange(input.Lease.Request, input.OwnershipChange));
    }

    private static LibraryExecutionEvidence Stop(
        LibraryMutationApplicationRequest input,
        LibraryCancellationFact? cancellation,
        LibraryUnexpectedFailureFact? unexpected)
    {
        var execution = new LibraryExecutionEvidence
        {
            Permission = input.Permissions is { } stage ? new(stage.Result, Receipt: null, stage.Failure) : null,
            Directories = [],
            Links = [],
            GeneratedRegions = [],
            Record = null,
            RecoveryPreparation = input.RecoveryPreparation,
            RecoveryCleanup = null,
            Cancellation = cancellation,
            UnexpectedFailure = unexpected,
            SourceEffectScope = new LibrarySourceEffectScopeFacts
            {
                ProtectedSourceRoots = input.ProtectedSourceRoots,
                AttemptedMutationTargets = [],
                IsComplete = true,
            },
            RecordPublicationOrder = LibraryRecordPublicationOrder.NotObserved,
        };
        return execution;
    }

    private static bool ProtectsSources(LibraryMutationApplicationRequest input)
    {
        var targets = input.Directories.Select(value => Relative(input.Lease, value.LogicalPath).Value)
            .Concat(input.SettingsParentDirectories.Select(value => Relative(input.Lease, value.LogicalPath).Value))
            .Concat(input.Links.Select(value => value.DestinationPath.Value))
            .Concat(input.GeneratedRegions.Select(value => Relative(input.Lease, value.LogicalPath).Value))
            .Concat(input.OwnershipChange is { } ownership ? [Relative(input.Lease, ownership.LogicalPath).Value] : [])
            .Concat(input.Permissions?.Change is { } permission ? [Relative(input.Lease, permission.LogicalPath).Value] : []);
        var sources = input.ProtectedSourceRoots.Select(source => PortableWorkspacePath.CreatePortableKey(source.Value)).ToArray();
        return targets.All(target =>
        {
            var key = PortableWorkspacePath.CreatePortableKey(target);
            return sources.All(source => key != source && !key.StartsWith($"{source}/", StringComparison.Ordinal));
        });
    }

    private static void ReadStop(
        FilesystemNotStartedReason? reason,
        string? cause,
        out LibraryCancellationFact? cancellation,
        out LibraryUnexpectedFailureFact? unexpected)
    {
        if (reason == FilesystemNotStartedReason.Cancelled)
        {
            cancellation = new LibraryCancellationFact(LibraryExecutionStage.Application);
            unexpected = null;
            return;
        }

        cancellation = null;
        unexpected = new LibraryUnexpectedFailureFact(
            LibraryExecutionStage.Application,
            cause ?? "A Library filesystem effect could not be applied and verified.");
    }

    private static bool Verified(
        FilesystemEffectState effectState,
        FilesystemVerificationState verificationState)
        => effectState == FilesystemEffectState.Applied
            && verificationState == FilesystemVerificationState.Verified;

    private static CanonicalRelativePath Relative(WorkspaceLockLease lease, string logicalPath)
        => CanonicalRelativePath.Create(Path.GetRelativePath(
            lease.Request.Workspace.LexicalRoot,
            logicalPath).Replace(Path.DirectorySeparatorChar, '/'));
}
