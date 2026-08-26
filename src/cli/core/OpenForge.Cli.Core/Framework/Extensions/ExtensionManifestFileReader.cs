using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Extensions.Models;

namespace OpenForge.Cli.Core.Framework.Extensions;

internal static class ExtensionManifestFileReader
{
    internal static async ValueTask<ExtensionManifestFileReadOutcome> ReadAsync(
        string physicalManifest,
        CancellationToken cancellationToken)
    {
        try
        {
            var bytes = await File.ReadAllBytesAsync(physicalManifest, cancellationToken).ConfigureAwait(false);
            return new ExtensionManifestFileReadSuccess(ExtensionManifestReader.Read(bytes, 0));
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
        catch (Exception exception) when (exception is JsonException or DecoderFallbackException)
        {
            return new ExtensionManifestFileReadFailure(
                new ExtensionSourceReadResult(
                    state: ExtensionSourceReadState.Invalid,
                    kind: null,
                    identity: physicalManifest,
                    packages: [],
                    cause: $"The Extension manifest is invalid: {exception.Message}"));
        }
        catch (UnauthorizedAccessException exception)
        {
            return Unavailable(physicalManifest, exception.Message);
        }
        catch (IOException exception)
        {
            return Unavailable(physicalManifest, exception.Message);
        }
    }

    private static ExtensionManifestFileReadFailure Unavailable(string physicalManifest, string cause)
        => new(
            new ExtensionSourceReadResult(
                state: ExtensionSourceReadState.Unavailable,
                kind: null,
                identity: physicalManifest,
                packages: [],
                cause: cause));
}
