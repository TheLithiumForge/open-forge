using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Serialization;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Framework.Lifecycle;

internal sealed partial class LifecycleStore
{
    private static readonly UTF8Encoding StrictUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);
    private readonly LifecycleDocumentSnapshotReader _snapshotReader;

    internal LifecycleStore(PhysicalPathResolver physicalPathResolver)
    {
        _snapshotReader = new LifecycleDocumentSnapshotReader(physicalPathResolver);
    }

    internal async ValueTask<LifecycleStoreReadResult> ReadAsync(
        CliWorkspace workspace,
        LifecycleSection selectedSection,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        if (!Enum.IsDefined(selectedSection))
        {
            throw new ArgumentOutOfRangeException(
                nameof(selectedSection),
                selectedSection,
                "The lifecycle section is not defined.");
        }

        var snapshot = await _snapshotReader.ReadAsync(workspace, cancellationToken).ConfigureAwait(false);
        return Read(snapshot, selectedSection);
    }

    internal LifecycleStoreReadResult Read(
        LifecycleDocumentSnapshot snapshot,
        LifecycleSection selectedSection)
    {
        if (!Enum.IsDefined(selectedSection))
        {
            throw new ArgumentOutOfRangeException(
                nameof(selectedSection),
                selectedSection,
                "The lifecycle section is not defined.");
        }

        return snapshot.State switch
        {
            LifecycleDocumentSnapshotState.Available => DecodeAvailable(snapshot, selectedSection),
            LifecycleDocumentSnapshotState.Missing => Missing(snapshot, selectedSection),
            LifecycleDocumentSnapshotState.Blocked => Blocked(snapshot, selectedSection),
            LifecycleDocumentSnapshotState.Unavailable => Unavailable(snapshot, selectedSection),
            LifecycleDocumentSnapshotState.Interrupted => LifecycleStoreReadResult.Cancelled(
                snapshot.Workspace,
                selectedSection),
            _ => throw new ArgumentOutOfRangeException(
                nameof(snapshot),
                snapshot.State,
                "The lifecycle document snapshot state is not defined."),
        };
    }

    private static LifecycleStoreReadResult Decode(
        CliWorkspace workspace,
        LifecycleSection selectedSection,
        FileStateSnapshot file)
    {
        try
        {
            _ = StrictUtf8.GetString(file.Bytes.AsSpan());
            LifecycleJsonSyntaxValidator.ValidateNoDuplicateProperties(
                file.Bytes.AsSpan(),
                selectedSection);
            var envelope = JsonSerializer.Deserialize(
                file.Bytes.AsSpan(),
                LifecycleJsonContext.Default.LifecycleEnvelopeV1)
                ?? throw new JsonException("The lifecycle document cannot be null.");
            var common = LifecycleDocumentValidator.ValidateCommon(workspace, envelope);
            if (common.State == LifecycleCommonValidationState.Invalid)
            {
                return LifecycleStoreReadResult.Invalid(
                    workspace,
                    selectedSection,
                    file,
                    common.Cause ?? "The lifecycle common envelope is invalid.");
            }

            if (common.State == LifecycleCommonValidationState.Blocked)
            {
                return LifecycleStoreReadResult.Blocked(
                    workspace,
                    selectedSection,
                    file,
                    common.Cause ?? "The lifecycle common envelope is blocked.");
            }

            return DecodeSelectedSection(workspace, selectedSection, file, envelope);
        }
        catch (Exception exception) when (exception is JsonException or DecoderFallbackException)
        {
            return LifecycleStoreReadResult.Invalid(
                workspace,
                selectedSection,
                file,
                $"The lifecycle document is invalid: {exception.Message}");
        }
    }

    private static LifecycleStoreReadResult DecodeSelectedSection(
        CliWorkspace workspace,
        LifecycleSection selectedSection,
        FileStateSnapshot file,
        LifecycleEnvelopeV1 envelope)
        => selectedSection switch
        {
            LifecycleSection.Framework => DecodeFramework(workspace, file, envelope),
            LifecycleSection.Extensions => DecodeExtensions(workspace, file, envelope),
            _ => throw new ArgumentOutOfRangeException(
                nameof(selectedSection),
                selectedSection,
                "The lifecycle section is not defined."),
        };

    private static LifecycleStoreReadResult DecodeAvailable(
        LifecycleDocumentSnapshot snapshot,
        LifecycleSection selectedSection)
    {
        if (snapshot.File is not { } file)
        {
            throw new InvalidOperationException("An available lifecycle snapshot requires file bytes.");
        }

        return Decode(snapshot.Workspace, selectedSection, file);
    }

    private static LifecycleStoreReadResult Missing(
        LifecycleDocumentSnapshot snapshot,
        LifecycleSection selectedSection)
    {
        if (snapshot.File is not { } file)
        {
            throw new InvalidOperationException("A missing lifecycle snapshot requires its file fact.");
        }

        return LifecycleStoreReadResult.DocumentMissing(snapshot.Workspace, selectedSection, file);
    }

    private static LifecycleStoreReadResult Blocked(
        LifecycleDocumentSnapshot snapshot,
        LifecycleSection selectedSection)
    {
        var cause = snapshot.Cause
            ?? throw new InvalidOperationException("A blocked lifecycle snapshot requires its cause.");
        return snapshot.File is { Kind: FileExpectationKind.Directory } file
            ? LifecycleStoreReadResult.Invalid(snapshot.Workspace, selectedSection, file, cause)
            : LifecycleStoreReadResult.Blocked(snapshot.Workspace, selectedSection, snapshot.File, cause);
    }

    private static LifecycleStoreReadResult Unavailable(
        LifecycleDocumentSnapshot snapshot,
        LifecycleSection selectedSection)
    {
        var failure = snapshot.Failure
            ?? throw new InvalidOperationException("An unavailable lifecycle snapshot requires its failure.");
        return LifecycleStoreReadResult.Unavailable(snapshot.Workspace, selectedSection, failure);
    }
}
