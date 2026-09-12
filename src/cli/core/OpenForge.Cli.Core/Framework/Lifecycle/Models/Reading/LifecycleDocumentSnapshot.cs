using OpenForge.Cli.Core.Framework.Filesystem.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Shared.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Lifecycle.Models.Reading;

internal enum LifecycleDocumentSnapshotState
{
    Available,
    Missing,
    Blocked,
    Unavailable,
    Interrupted,
}

internal enum LifecycleDocumentFailureStage
{
    PhysicalResolution,
    PhysicalReconfirmation,
    ContainedFileAccess,
}

internal sealed record LifecycleDocumentSnapshot
{
    private const int MaximumCauseLength = 256;

    private LifecycleDocumentSnapshot(
        LifecycleDocumentSnapshotState state,
        CliWorkspace workspace,
        FileStateSnapshot? file,
        FilesystemFailure? failure,
        LifecycleDocumentFailureStage? failureStage,
        string? cause)
    {
        State = state;
        Workspace = workspace;
        File = file;
        Failure = failure;
        FailureStage = failureStage;
        Cause = cause;
    }

    internal LifecycleDocumentSnapshotState State { get; }

    internal CliWorkspace Workspace { get; }

    internal FileStateSnapshot? File { get; }

    internal FilesystemFailure? Failure { get; }

    internal LifecycleDocumentFailureStage? FailureStage { get; }

    internal string? Cause { get; }

    internal static LifecycleDocumentSnapshot Available(
        CliWorkspace workspace,
        FileStateSnapshot file)
    {
        if (file.Kind != FileExpectationKind.File || !file.HasBytes)
        {
            throw new ArgumentException(
                "An available lifecycle document requires exact file bytes.",
                nameof(file));
        }

        LifecycleDocumentValidator.ValidateSnapshotPath(workspace, file);

        return new LifecycleDocumentSnapshot(
            LifecycleDocumentSnapshotState.Available,
            workspace,
            file,
            failure: null,
            failureStage: null,
            cause: null);
    }

    internal static LifecycleDocumentSnapshot Missing(
        CliWorkspace workspace,
        FileStateSnapshot file)
    {
        if (file.Kind != FileExpectationKind.Missing || file.HasBytes)
        {
            throw new ArgumentException(
                "A missing lifecycle document requires a missing file snapshot.",
                nameof(file));
        }

        LifecycleDocumentValidator.ValidateSnapshotPath(workspace, file);

        return new LifecycleDocumentSnapshot(
            LifecycleDocumentSnapshotState.Missing,
            workspace,
            file,
            failure: null,
            failureStage: null,
            cause: null);
    }

    internal static LifecycleDocumentSnapshot Blocked(
        CliWorkspace workspace,
        FileStateSnapshot? file,
        string cause)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cause);
        if (file is { Kind: not FileExpectationKind.Directory } || file?.HasBytes == true)
        {
            throw new ArgumentException(
                "A blocked lifecycle document can carry only a directory snapshot.",
                nameof(file));
        }

        if (file is not null)
        {
            LifecycleDocumentValidator.ValidateSnapshotPath(workspace, file);
        }

        var boundedCause = cause.Length <= MaximumCauseLength
            ? cause
            : cause[..MaximumCauseLength];

        return new LifecycleDocumentSnapshot(
            LifecycleDocumentSnapshotState.Blocked,
            workspace,
            file,
            failure: null,
            failureStage: null,
            boundedCause);
    }

    internal static LifecycleDocumentSnapshot Unavailable(
        CliWorkspace workspace,
        FilesystemFailure failure,
        LifecycleDocumentFailureStage failureStage)
    {
        ValidateFailure(failure, failureStage);
        return new LifecycleDocumentSnapshot(
            LifecycleDocumentSnapshotState.Unavailable,
            workspace,
            file: null,
            failure,
            failureStage,
            cause: null);
    }

    internal static LifecycleDocumentSnapshot Interrupted(CliWorkspace workspace)
        => new(
            LifecycleDocumentSnapshotState.Interrupted,
            workspace,
            file: null,
            failure: null,
            failureStage: null,
            cause: null);

    private static void ValidateFailure(
        FilesystemFailure failure,
        LifecycleDocumentFailureStage failureStage)
    {
        var validFailure = failureStage switch
        {
            LifecycleDocumentFailureStage.PhysicalResolution
                or LifecycleDocumentFailureStage.PhysicalReconfirmation => failure.Kind is
                    FilesystemFailureKind.AccessDenied
                    or FilesystemFailureKind.InputOutput
                    or FilesystemFailureKind.InvalidPath
                    or FilesystemFailureKind.Unsupported,
            LifecycleDocumentFailureStage.ContainedFileAccess => failure.Kind is
                FilesystemFailureKind.AccessDenied or FilesystemFailureKind.InputOutput,
            _ => throw new ArgumentOutOfRangeException(
                nameof(failureStage),
                failureStage,
                "The lifecycle document failure stage is not defined."),
        };
        if (!validFailure)
        {
            throw new ArgumentException(
                "The filesystem failure kind does not match the lifecycle document failure stage.",
                nameof(failure));
        }
    }
}
