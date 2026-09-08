using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Recovery.Comparison;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models.Comparison;
using OpenForge.Cli.Core.Framework.Recovery.Observation;
using OpenForge.Cli.Core.Framework.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;

namespace OpenForge.Cli.Core.Framework.Recovery.Application;

internal static class OrdinaryFileRecoveryApplier
{
    internal static async ValueTask<OrdinaryFileRecoveryResult> ApplyAsync(
        PhysicalPathResolver resolver,
        WorkspaceLockLease lease,
        RecoveryBundlePreparation preparation,
        RecoveryEntry entry,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(resolver);
        Validate(lease, preparation, entry);
        cancellationToken.ThrowIfCancellationRequested();
        var input = await RecoveryEntryObservationReader.ReadAsync(
            resolver, new RecoveryEntryComparisonContext(lease.Request.Workspace, entry), cancellationToken).ConfigureAwait(false);
        return await ApplyComparedAsync(
            resolver,
            lease,
            preparation,
            RecoveryEntryComparer.Compare(input),
            cancellationToken).ConfigureAwait(false);
    }

    private static async ValueTask<OrdinaryFileRecoveryResult> ApplyComparedAsync(
        PhysicalPathResolver resolver,
        WorkspaceLockLease lease,
        RecoveryBundlePreparation preparation,
        RecoveryEntryComparison comparison,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(comparison);
        Validate(lease, preparation, comparison.Input.Context.Entry);
        if (!lease.IsHeldFor(comparison.Input.Context.Workspace))
        {
            throw new ArgumentException("Ordinary recovery comparison must retain the held workspace context.", nameof(comparison));
        }

        cancellationToken.ThrowIfCancellationRequested();
        if (comparison.State != RecoveryBundleTargetComparisonState.Intended)
        {
            return NotStarted(
                comparison,
                comparison.Cause
                    ?? "The recovery target is not in the exact intended state.");
        }

        var evidence = await RecoveryApplicationEvidence.ReadExactFinalAsync(
            lease.Request.Workspace,
            preparation,
            cancellationToken).ConfigureAwait(false);
        if (evidence.State != RecoveryBundleReadState.Valid)
        {
            return NotStarted(
                comparison,
                evidence.Cause ?? "The recovery final is unavailable.",
                evidence.Failure);
        }

        byte[]? priorBytes = null;
        var entry = comparison.Input.Context.Entry;
        if (entry.Prior.Kind == RecoveryEntryStateKind.OrdinaryFile)
        {
            try
            {
                priorBytes = await RecoveryApplicationEvidence.ReadPriorPayloadAsync(
                    preparation,
                    entry,
                    cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception) when (exception is InvalidDataException
                or UnauthorizedAccessException
                or IOException
                or ArgumentException
                or NotSupportedException
                or PlatformNotSupportedException)
            {
                return NotStarted(
                    comparison,
                    "The verified prior payload could not be reread.");
            }

            var confirmedEvidence = await RecoveryApplicationEvidence.ReadExactFinalAsync(
                lease.Request.Workspace,
                preparation,
                cancellationToken).ConfigureAwait(false);
            if (confirmedEvidence.State != RecoveryBundleReadState.Valid)
            {
                return NotStarted(
                    comparison,
                    confirmedEvidence.Cause
                        ?? "The recovery final changed during payload readback.",
                    confirmedEvidence.Failure);
            }
        }

        var immediateInput = await RecoveryEntryObservationReader.ReadAsync(
            resolver,
            comparison.Input.Context,
            cancellationToken).ConfigureAwait(false);
        var immediate = RecoveryEntryComparer.Compare(immediateInput);
        if (immediate.State != RecoveryBundleTargetComparisonState.Intended)
        {
            return NotStarted(
                immediate,
                immediate.Cause
                    ?? "The recovery target changed before the inverse effect.");
        }

        var targetPath = ResolveNoFollowTarget(
            resolver,
            lease,
            comparison.Input.Context.LogicalPath);
        if (targetPath is null)
        {
            return NotStarted(
                immediate,
                "The intended target has no safe contained no-follow physical destination.");
        }

        string? stagePath = null;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (entry.Prior.Kind == RecoveryEntryStateKind.Missing)
            {
                File.Delete(targetPath);
            }
            else
            {
                stagePath = await WriteStageAsync(
                    targetPath,
                    priorBytes
                        ?? throw new InvalidOperationException(
                            "An ordinary prior state requires verified payload bytes."),
                    cancellationToken).ConfigureAwait(false);

                var finalInput = await RecoveryEntryObservationReader.ReadAsync(
                    resolver,
                    immediate.Input.Context,
                    cancellationToken).ConfigureAwait(false);
                var finalComparison = RecoveryEntryComparer.Compare(finalInput);
                if (finalComparison.State != RecoveryBundleTargetComparisonState.Intended)
                {
                    return NotStarted(
                        finalComparison,
                        finalComparison.Cause
                            ?? "The recovery target changed while the prior payload was staged.");
                }

                var finalTargetPath = ResolveNoFollowTarget(
                    resolver,
                    lease,
                    finalComparison.Input.Context.LogicalPath);
                if (finalTargetPath is null
                    || !PhysicalIdentityTracker.PathComparer.Equals(
                        targetPath,
                        finalTargetPath))
                {
                    return NotStarted(
                        finalComparison,
                        "The recovery target physical identity changed while the prior payload was staged.");
                }

                immediate = finalComparison;
                cancellationToken.ThrowIfCancellationRequested();
                File.Move(
                    stagePath,
                    targetPath,
                    overwrite: entry.Intended.Kind == RecoveryEntryStateKind.OrdinaryFile);
                stagePath = null;
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception) when (exception is UnauthorizedAccessException
            or IOException
            or NotSupportedException
            or PlatformNotSupportedException)
        {
            var afterFailure = await ObserveAfterAsync(
                resolver,
                immediate.Input.Context,
                cancellationToken).ConfigureAwait(false);
            if (afterFailure?.State == RecoveryBundleTargetComparisonState.Prior)
            {
                return new OrdinaryFileRecoveryResult
                {
                    Before = immediate,
                    After = afterFailure,
                    Effect = FilesystemEffectState.Applied,
                    Verification = FilesystemVerificationState.Verified,
                    Failure = null,
                    Cause = null,
                };
            }

            var failure = FilesystemFailure.FromException(
                exception switch
                {
                    UnauthorizedAccessException => FilesystemFailureKind.AccessDenied,
                    NotSupportedException or PlatformNotSupportedException =>
                        FilesystemFailureKind.Unsupported,
                    _ => FilesystemFailureKind.InputOutput,
                },
                exception);
            return new OrdinaryFileRecoveryResult
            {
                Before = immediate,
                After = afterFailure,
                Effect = afterFailure?.State == RecoveryBundleTargetComparisonState.Intended
                    ? FilesystemEffectState.NotStarted
                    : FilesystemEffectState.Unknown,
                Verification = afterFailure?.State == RecoveryBundleTargetComparisonState.Intended
                    ? FilesystemVerificationState.NotStarted
                    : FilesystemVerificationState.Failed,
                Failure = failure,
                Cause = failure.DirectCause,
            };
        }
        finally
        {
            DeleteStage(stagePath);
        }

        RecoveryEntryComparison after;
        try
        {
            after = RecoveryEntryComparer.Compare(
                await RecoveryEntryObservationReader.ReadAsync(
                    resolver,
                    immediate.Input.Context,
                    cancellationToken).ConfigureAwait(false));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return new OrdinaryFileRecoveryResult
            {
                Before = immediate,
                After = null,
                Effect = FilesystemEffectState.Applied,
                Verification = FilesystemVerificationState.Failed,
                Failure = null,
                Cause = "Ordinary recovery verification was cancelled after the inverse effect returned.",
            };
        }

        return new OrdinaryFileRecoveryResult
        {
            Before = immediate,
            After = after,
            Effect = FilesystemEffectState.Applied,
            Verification = after.State == RecoveryBundleTargetComparisonState.Prior
                ? FilesystemVerificationState.Verified
                : FilesystemVerificationState.Failed,
            Failure = null,
            Cause = after.State == RecoveryBundleTargetComparisonState.Prior
                ? null
                : "The ordinary recovery inverse did not restore the exact prior identity.",
        };
    }

    private static void Validate(WorkspaceLockLease lease, RecoveryBundlePreparation preparation, RecoveryEntry entry)
    {
        ArgumentNullException.ThrowIfNull(lease);
        ArgumentNullException.ThrowIfNull(preparation);
        ArgumentNullException.ThrowIfNull(entry);
        if (!lease.IsHeldFor(lease.Request.Workspace) || !preparation.MatchesWorkspace(lease.Request.Workspace)
            || !preparation.Entries.Contains(entry))
        {
            throw new ArgumentException("Ordinary recovery requires an exact preparation member and its live workspace lease.", nameof(entry));
        }

        switch (entry.Kind)
        {
            case RecoveryEntryKind.OrdinaryCreate:
            case RecoveryEntryKind.OrdinaryReplace:
            case RecoveryEntryKind.OrdinaryReplaceGeneratedRegion:
            case RecoveryEntryKind.OrdinaryDelete:
                break;
            case RecoveryEntryKind.RelativeFileLinkCreate:
            case RecoveryEntryKind.RelativeFileLinkDelete:
                throw new ArgumentException("Ordinary recovery does not apply relative file-link entries.", nameof(entry));
            default:
                throw new ArgumentOutOfRangeException(nameof(entry), entry.Kind, "The recovery entry kind is not defined.");
        }
    }

    private static OrdinaryFileRecoveryResult NotStarted(
        RecoveryEntryComparison before,
        string cause,
        FilesystemFailure? failure = null)
        => new()
        {
            Before = before,
            After = null,
            Effect = FilesystemEffectState.NotStarted,
            Verification = FilesystemVerificationState.NotStarted,
            Failure = failure,
            Cause = cause,
        };

    private static string? ResolveNoFollowTarget(
        PhysicalPathResolver resolver,
        WorkspaceLockLease lease,
        string logicalPath)
    {
        var parent = Path.GetDirectoryName(logicalPath);
        if (parent is null)
        {
            return null;
        }

        var resolution = resolver.ResolveCandidate(
            lease.Request.Workspace.LexicalRoot,
            lease.Request.Workspace.PhysicalRoot,
            parent);
        if (resolution.State != PhysicalPathState.Contained)
        {
            return null;
        }

        var target = Path.Combine(
            resolution.GetContainedPhysicalPath(),
            Path.GetFileName(logicalPath));
        return PhysicalContainment.Contains(
            lease.Request.Workspace.PhysicalRoot,
            target)
            ? target
            : null;
    }

    private static async ValueTask<string> WriteStageAsync(
        string targetPath,
        byte[] bytes,
        CancellationToken cancellationToken)
    {
        var parent = Path.GetDirectoryName(targetPath)
            ?? throw new IOException("The ordinary recovery target has no parent directory.");
        var stage = Path.Combine(
            parent,
            $".open-forge-recovery-{Path.GetRandomFileName()}");
        var stageCreated = false;
        try
        {
            await using (var stream = new FileStream(
                             stage,
                             new FileStreamOptions
                             {
                                 Mode = FileMode.CreateNew,
                                 Access = FileAccess.Write,
                                 Share = FileShare.None,
                                 BufferSize = RecoveryBundleFormatV1.StreamBufferSize,
                                 Options = FileOptions.SequentialScan,
                             }))
            {
                stageCreated = true;
                await stream.WriteAsync(bytes, cancellationToken).ConfigureAwait(false);
            }

            var stagedBytes = await File.ReadAllBytesAsync(
                stage,
                cancellationToken).ConfigureAwait(false);
            if (!stagedBytes.AsSpan().SequenceEqual(bytes.AsSpan()))
            {
                throw new IOException("The staged recovery bytes did not match the verified prior payload.");
            }

            return stage;
        }
        catch
        {
            if (stageCreated)
            {
                DeleteStage(stage);
            }

            throw;
        }
    }

    private static async ValueTask<RecoveryEntryComparison?> ObserveAfterAsync(
        PhysicalPathResolver resolver,
        RecoveryEntryComparisonContext context,
        CancellationToken cancellationToken)
    {
        try
        {
            return RecoveryEntryComparer.Compare(
                await RecoveryEntryObservationReader.ReadAsync(
                    resolver,
                    context,
                    cancellationToken).ConfigureAwait(false));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return null;
        }
    }

    private static void DeleteStage(string? stagePath)
    {
        if (stagePath is null)
        {
            return;
        }

        try
        {
            var component = LinkTargetReader.Read(stagePath);
            if (component.State == PathComponentState.Missing)
            {
                return;
            }

            if (component.State != PathComponentState.Ordinary
                || component.Attributes is not { } attributes
                || (attributes & (FileAttributes.Directory
                    | FileAttributes.Device
                    | FileAttributes.ReparsePoint)) != 0)
            {
                return;
            }

            File.Delete(stagePath);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
        catch (NotSupportedException)
        {
        }
    }
}
