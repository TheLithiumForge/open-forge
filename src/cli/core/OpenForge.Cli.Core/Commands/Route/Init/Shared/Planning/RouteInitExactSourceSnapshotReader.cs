using System.Text;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;

namespace OpenForge.Cli.Core.Commands.Route.Init.Shared.Planning;

internal sealed class RouteInitExactSourceSnapshotReader
{
    private static readonly UTF8Encoding StrictUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    internal async ValueTask<FileStateSnapshot> ReadAsync(
        RouteInitRequest request,
        SourceDocumentReadResult read,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(read);
        if (read.Verification.State != SourceLayerVerificationState.Verified
            || read.Verification.CurrentPhysicalPath is not { } physicalPath)
        {
            throw new RouteInitPlanningException(
                RouteInitFindingCode.InspectionIncomplete,
                "The exact current source snapshot is unavailable.",
                incomplete: true);
        }

        try
        {
            var bytes = await File.ReadAllBytesAsync(physicalPath, cancellationToken)
                .ConfigureAwait(false);
            _ = StrictUtf8.GetString(bytes);
            return FileStateSnapshot.File(
                SourceLogicalPath.ToLexicalPath(
                    request.Workspace.LexicalRoot,
                    read.Layer.CanonicalPath),
                physicalPath,
                bytes);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw new RouteInitPlanningException(
                RouteInitFindingCode.Interrupted,
                "Route Init source snapshotting was cancelled.",
                incomplete: true);
        }
        catch (Exception exception) when (exception is UnauthorizedAccessException
            or IOException
            or DecoderFallbackException)
        {
            throw new RouteInitPlanningException(
                RouteInitFindingCode.InspectionIncomplete,
                exception.Message,
                incomplete: true);
        }
    }
}

internal sealed class RouteInitPlanningException(
    RouteInitFindingCode code,
    string message,
    bool incomplete) : Exception(message)
{
    internal RouteInitFindingCode Code { get; } = code;

    internal bool Incomplete { get; } = incomplete;
}
