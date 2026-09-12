using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Reading;
using OpenForge.Cli.Core.Framework.Lifecycle.Serialization;
using OpenForge.Cli.Core.Framework.Lifecycle.Shared.Validation;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Lifecycle;

internal sealed class LifecycleDocumentReader(PhysicalPathResolver physicalPathResolver)
{
    private static readonly UTF8Encoding StrictUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);
    private readonly LifecycleDocumentSnapshotReader _snapshotReader = new(physicalPathResolver);

    internal async ValueTask<LifecycleReadResult> ReadExtensionsAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken)
    {
        var snapshot = await _snapshotReader.ReadAsync(workspace, cancellationToken).ConfigureAwait(false);
        return ReadExtensions(snapshot);
    }

    internal LifecycleReadResult ReadExtensions(LifecycleDocumentSnapshot snapshot)
    {
        if (snapshot.State != LifecycleDocumentSnapshotState.Available)
        {
            return ReadUnavailableSnapshot(snapshot);
        }

        if (snapshot.File is not { } file)
        {
            throw new InvalidOperationException("An available lifecycle snapshot requires file bytes.");
        }

        try
        {
            _ = StrictUtf8.GetCharCount(file.Bytes.AsSpan());
            LifecycleJsonSyntaxValidator.ValidateNoDuplicateProperties(
                file.Bytes.AsSpan(),
                LifecycleSection.Extensions);
            var envelope = JsonSerializer.Deserialize(
                file.Bytes.AsSpan(),
                LifecycleJsonContext.Default.LifecycleEnvelopeV1)
                ?? throw new JsonException("The lifecycle document cannot be null.");
            var extensions = envelope.Extensions is { } extensionValue
                ? extensionValue.Deserialize(LifecycleJsonContext.Default.ExtensionLifecycleState)
                    ?? throw new JsonException("The lifecycle Extension section cannot be null.")
                : null;
            return LifecycleDocumentValidator.ValidateExtensions(snapshot.Workspace, envelope, extensions);
        }
        catch (Exception exception) when (exception is JsonException or DecoderFallbackException)
        {
            return InvalidJson(exception);
        }
    }

    private static LifecycleReadResult ReadUnavailableSnapshot(LifecycleDocumentSnapshot snapshot)
        => snapshot.State switch
        {
            LifecycleDocumentSnapshotState.Missing => Missing(),
            LifecycleDocumentSnapshotState.Blocked => ReadBlockedSnapshot(snapshot),
            LifecycleDocumentSnapshotState.Unavailable => ReadUnavailableFailure(snapshot),
            LifecycleDocumentSnapshotState.Interrupted => Cancelled(),
            LifecycleDocumentSnapshotState.Available => throw new InvalidOperationException(
                "An available lifecycle snapshot must be decoded."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(snapshot),
                snapshot.State,
                "The lifecycle document snapshot state is not defined."),
        };

    private static LifecycleReadResult ReadUnavailableFailure(LifecycleDocumentSnapshot snapshot)
    {
        var failure = snapshot.Failure
            ?? throw new InvalidOperationException("An unavailable lifecycle snapshot requires its failure.");
        return snapshot.FailureStage switch
        {
            LifecycleDocumentFailureStage.PhysicalResolution
                or LifecycleDocumentFailureStage.PhysicalReconfirmation => Blocked(failure.DirectCause),
            LifecycleDocumentFailureStage.ContainedFileAccess => Unavailable(failure.DirectCause),
            null => throw new InvalidOperationException(
                "An unavailable lifecycle snapshot requires its failure stage."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(snapshot),
                snapshot.FailureStage,
                "The lifecycle document failure stage is not defined."),
        };
    }

    private static LifecycleReadResult ReadBlockedSnapshot(LifecycleDocumentSnapshot snapshot)
    {
        var cause = snapshot.Cause
            ?? throw new InvalidOperationException("A blocked lifecycle snapshot requires its cause.");
        return snapshot.File is not null
            ? Unavailable(cause)
            : Blocked(cause);
    }

    private static LifecycleReadResult InvalidJson(Exception exception)
        => LifecycleReadResult.Create(
            state: LifecycleReadState.Invalid,
            trust: LifecycleExtensionTrust.Incomplete,
            packages: [],
            cause: $"The lifecycle document is invalid: {exception.Message}",
            coverageFacts: new LifecycleCoverageFacts
            {
                Paths = [],
                Coverage = LifecycleCoverageState.Incomplete,
                WorkspaceBinding = LifecycleWorkspaceBinding.NotChecked,
            });

    private static LifecycleReadResult Missing()
        => LifecycleReadResult.Create(
            state: LifecycleReadState.Missing,
            trust: LifecycleExtensionTrust.Incomplete,
            packages: [],
            cause: "The lifecycle document is missing; installed coverage cannot be proven empty.",
            coverageFacts: new LifecycleCoverageFacts
            {
                Paths = [],
                Coverage = LifecycleCoverageState.Incomplete,
                WorkspaceBinding = LifecycleWorkspaceBinding.NotChecked,
            });

    private static LifecycleReadResult Blocked(string cause)
        => LifecycleReadResult.Create(
            state: LifecycleReadState.Invalid,
            trust: LifecycleExtensionTrust.Blocked,
            packages: [],
            cause: cause,
            coverageFacts: new LifecycleCoverageFacts
            {
                Paths = [],
                Coverage = LifecycleCoverageState.Blocked,
                WorkspaceBinding = LifecycleWorkspaceBinding.Unavailable,
            });

    private static LifecycleReadResult Cancelled()
        => LifecycleReadResult.Create(
            state: LifecycleReadState.Cancelled,
            trust: LifecycleExtensionTrust.Incomplete,
            packages: [],
            cause: "Lifecycle inspection was interrupted.",
            coverageFacts: new LifecycleCoverageFacts
            {
                Paths = [],
                Coverage = LifecycleCoverageState.Interrupted,
                WorkspaceBinding = LifecycleWorkspaceBinding.NotChecked,
            });

    private static LifecycleReadResult Unavailable(string cause)
        => LifecycleReadResult.Create(
            state: LifecycleReadState.Unavailable,
            trust: LifecycleExtensionTrust.Incomplete,
            packages: [],
            cause: cause,
            coverageFacts: new LifecycleCoverageFacts
            {
                Paths = [],
                Coverage = LifecycleCoverageState.Incomplete,
                WorkspaceBinding = LifecycleWorkspaceBinding.NotChecked,
            });
}
