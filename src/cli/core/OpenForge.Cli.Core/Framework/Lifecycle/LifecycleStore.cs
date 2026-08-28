using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Filesystem;
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
    private readonly PhysicalPathResolver _physicalPathResolver;

    internal LifecycleStore(PhysicalPathResolver physicalPathResolver)
    {
        _physicalPathResolver = physicalPathResolver;
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

        if (cancellationToken.IsCancellationRequested)
        {
            return LifecycleStoreReadResult.Cancelled(workspace, selectedSection);
        }

        var logicalPath = Path.Combine(
            workspace.LexicalRoot,
            LifecycleSchema.DirectoryName,
            LifecycleSchema.FileName);
        var resolution = Resolve(workspace, logicalPath);
        if (resolution.State == PhysicalPathState.Missing)
        {
            return LifecycleStoreReadResult.DocumentMissing(
                workspace,
                selectedSection,
                FileStateSnapshot.Missing(logicalPath));
        }

        if (resolution.State != PhysicalPathState.Contained)
        {
            return FromResolution(workspace, selectedSection, resolution);
        }

        var physicalPath = resolution.GetContainedPhysicalPath();
        try
        {
            var attributes = File.GetAttributes(physicalPath);
            if ((attributes & FileAttributes.Directory) != 0)
            {
                return LifecycleStoreReadResult.Invalid(
                    workspace,
                    selectedSection,
                    FileStateSnapshot.Directory(logicalPath, physicalPath),
                    "The lifecycle document path is a directory.");
            }

            if ((attributes & (FileAttributes.Device | FileAttributes.ReparsePoint)) != 0)
            {
                return LifecycleStoreReadResult.Blocked(
                    workspace,
                    selectedSection,
                    file: null,
                    "The lifecycle document path is not an ordinary file.");
            }

            var bytes = await File.ReadAllBytesAsync(physicalPath, cancellationToken).ConfigureAwait(false);
            if (cancellationToken.IsCancellationRequested)
            {
                return LifecycleStoreReadResult.Cancelled(workspace, selectedSection);
            }

            var confirmed = Resolve(workspace, logicalPath);
            if (confirmed.State != PhysicalPathState.Contained)
            {
                return FromResolution(workspace, selectedSection, confirmed);
            }

            var confirmedPhysicalPath = confirmed.GetContainedPhysicalPath();
            if (!string.Equals(physicalPath, confirmedPhysicalPath, PathComparison()))
            {
                return LifecycleStoreReadResult.Blocked(
                    workspace,
                    selectedSection,
                    file: null,
                    "The lifecycle document changed its resolved physical path during inspection.");
            }

            var file = FileStateSnapshot.File(logicalPath, confirmedPhysicalPath, bytes);
            return Decode(workspace, selectedSection, file);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return LifecycleStoreReadResult.Cancelled(workspace, selectedSection);
        }
        catch (Exception exception) when (exception is FileNotFoundException or DirectoryNotFoundException)
        {
            return LifecycleStoreReadResult.DocumentMissing(
                workspace,
                selectedSection,
                FileStateSnapshot.Missing(logicalPath));
        }
        catch (UnauthorizedAccessException exception)
        {
            return Unavailable(
                workspace,
                selectedSection,
                FilesystemFailureKind.AccessDenied,
                exception);
        }
        catch (IOException exception)
        {
            return Unavailable(
                workspace,
                selectedSection,
                FilesystemFailureKind.InputOutput,
                exception);
        }
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

    private PhysicalPathResolution Resolve(CliWorkspace workspace, string path)
        => _physicalPathResolver.ResolveCandidate(
            workspace.LexicalRoot,
            workspace.PhysicalRoot,
            path);

    private static StringComparison PathComparison()
        => OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
}
