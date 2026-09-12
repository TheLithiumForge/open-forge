using System.Text;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Reading;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Planning;

internal sealed class RouteUpdateLayerObserver(SourceDocumentSnapshotReader snapshotReader)
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);
    private readonly SourceDocumentSnapshotReader _snapshotReader = snapshotReader;

    internal async ValueTask<RouteUpdateObservedLayerBuild> ObserveAsync(
        RouteUpdateLayerObservationInput input,
        CancellationToken cancellationToken)
    {
        var reader = new SourceDocumentReader(input.Workspace);
        var read = await reader.ReadAsync(input.Layer, cancellationToken)
            .ConfigureAwait(false);
        if (!TryReadText(read, out var text, out var code, out var cause))
        {
            return RouteUpdateObservedLayerBuild.Stop(code, cause, IsIncomplete(code));
        }

        try
        {
            var snapshot = await _snapshotReader
                .ReadAsync(input.Workspace, read, cancellationToken)
                .ConfigureAwait(false);
            var snapshotText = StrictUtf8.GetString(snapshot.Bytes.AsSpan());
            if (!string.Equals(text, snapshotText, StringComparison.Ordinal))
            {
                return RouteUpdateObservedLayerBuild.Stop(
                    RouteUpdateFindingCode.TargetChanged,
                    "The selected source changed while its exact observation was formed.",
                    isIncomplete: false);
            }

            return RouteUpdateObservedLayerBuild.Complete(text, snapshot);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return RouteUpdateObservedLayerBuild.Stop(
                RouteUpdateFindingCode.Interrupted,
                "Route Update source observation was interrupted.",
                isIncomplete: true);
        }
        catch (DecoderFallbackException)
        {
            return RouteUpdateObservedLayerBuild.Stop(
                RouteUpdateFindingCode.TargetUnsafe,
                "The selected source changed to invalid UTF-8 during observation.",
                isIncomplete: false);
        }
        catch (UnauthorizedAccessException)
        {
            return RouteUpdateObservedLayerBuild.Stop(
                RouteUpdateFindingCode.InspectionIncomplete,
                "The selected source became unreadable during observation.",
                isIncomplete: true);
        }
        catch (IOException)
        {
            return RouteUpdateObservedLayerBuild.Stop(
                RouteUpdateFindingCode.InspectionIncomplete,
                "The selected source became unavailable during observation.",
                isIncomplete: true);
        }
    }

    private static bool TryReadText(
        SourceDocumentReadResult read,
        out string text,
        out RouteUpdateFindingCode code,
        out string cause)
    {
        text = string.Empty;
        code = RouteUpdateFindingCode.InspectionIncomplete;
        cause = read.Read?.Failure?.DirectCause
            ?? "The selected source content is unavailable.";
        if (read.Verification.State == SourceLayerVerificationState.Verified
            && read.Read?.State == FileReadState.Complete
            && read.Read.Value is { } value)
        {
            text = value;
            return true;
        }

        if (read.Read?.State == FileReadState.Cancelled)
        {
            code = RouteUpdateFindingCode.Interrupted;
            return false;
        }

        code = read.Verification.State switch
        {
            SourceLayerVerificationState.Unsafe
                or SourceLayerVerificationState.Changed => RouteUpdateFindingCode.TargetUnsafe,
            SourceLayerVerificationState.Cancelled => RouteUpdateFindingCode.Interrupted,
            SourceLayerVerificationState.Verified
                or SourceLayerVerificationState.Missing
                or SourceLayerVerificationState.Unavailable =>
                RouteUpdateFindingCode.InspectionIncomplete,
            _ => throw new ArgumentOutOfRangeException(
                nameof(read),
                read.Verification.State,
                "The source-layer verification state is not defined."),
        };
        return false;
    }

    private static bool IsIncomplete(RouteUpdateFindingCode code)
        => code is RouteUpdateFindingCode.InspectionIncomplete
            or RouteUpdateFindingCode.WorkspaceUnavailable
            or RouteUpdateFindingCode.Interrupted;
}
