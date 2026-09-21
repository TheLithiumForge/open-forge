using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Extensions.Models;

namespace OpenForge.Cli.Core.Framework.Extensions.Shared.Manifest;

internal static class ExtensionManifestFileReader
{
    private const int WindowsSharingViolationError = 32;
    private const int WindowsLockViolationError = 33;

    internal static async ValueTask<ExtensionManifestFileReadOutcome> ReadAsync(
        string physicalManifest,
        string manifestPath,
        IEnumerable<ExtensionPackageFileFact> payload,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(manifestPath);
        ArgumentNullException.ThrowIfNull(payload);
        try
        {
            var bytes = await File.ReadAllBytesAsync(physicalManifest, cancellationToken).ConfigureAwait(false);
            return new ExtensionManifestFileReadSuccess(
                ExtensionManifestReader.Read(bytes, manifestPath, payload));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return new ExtensionManifestFileReadFailure(
                new ExtensionSourceReadResult(
                    state: ExtensionSourceReadState.Cancelled,
                    kind: null,
                    identity: physicalManifest,
                    packages: [],
                    cause: "Extension source inspection was interrupted."));
        }
        catch (ExtensionManifestDependencyConflictException exception)
        {
            return new ExtensionManifestFileReadFailure(
                new ExtensionSourceReadResult(
                    state: ExtensionSourceReadState.Invalid,
                    kind: null,
                    identity: physicalManifest,
                    packages: [],
                    cause: $"The Extension manifest has conflicting dependencies: {exception.Message}",
                    failureKind: ExtensionSourceFailureKind.DependencyConflict));
        }
        catch (JsonException exception)
        {
            return new ExtensionManifestFileReadFailure(
                new ExtensionSourceReadResult(
                    state: ExtensionSourceReadState.Invalid,
                    kind: null,
                    identity: physicalManifest,
                    packages: [],
                    cause: $"The Extension manifest is invalid: {exception.Message}",
                    failureKind: ExtensionSourceFailureKind.PackageInvalid)
                {
                    FailureDetail = new(
                        physicalManifest,
                        ExtensionSourceFailureDetailKind.InvalidManifest),
                });
        }
        catch (DecoderFallbackException exception)
        {
            return new ExtensionManifestFileReadFailure(
                new ExtensionSourceReadResult(
                    state: ExtensionSourceReadState.Invalid,
                    kind: null,
                    identity: physicalManifest,
                    packages: [],
                    cause: $"The Extension manifest is invalid: {exception.Message}",
                    failureKind: ExtensionSourceFailureKind.PackageInvalid)
                {
                    FailureDetail = new(
                        physicalManifest,
                        ExtensionSourceFailureDetailKind.InvalidEncoding),
                });
        }
        catch (UnauthorizedAccessException exception)
        {
            return Unavailable(
                physicalManifest,
                exception.Message,
                ExtensionSourceFailureDetailKind.AccessDenied);
        }
        catch (IOException exception)
        {
            return Unavailable(
                physicalManifest,
                exception.Message,
                IsWindowsSharingDenial(exception)
                    ? ExtensionSourceFailureDetailKind.FileInUse
                    : ExtensionSourceFailureDetailKind.InputOutput);
        }
    }

    private static ExtensionManifestFileReadFailure Unavailable(
        string physicalManifest,
        string cause,
        ExtensionSourceFailureDetailKind detailKind)
        => new(
            new ExtensionSourceReadResult(
                state: ExtensionSourceReadState.Unavailable,
                kind: null,
                identity: physicalManifest,
                packages: [],
                cause: cause)
            {
                FailureDetail = new(physicalManifest, detailKind),
            });

    private static bool IsWindowsSharingDenial(IOException exception)
    {
        if (!OperatingSystem.IsWindows())
        {
            return false;
        }

        var win32Error = exception.HResult & 0xFFFF;
        return win32Error is WindowsSharingViolationError or WindowsLockViolationError;
    }
}
