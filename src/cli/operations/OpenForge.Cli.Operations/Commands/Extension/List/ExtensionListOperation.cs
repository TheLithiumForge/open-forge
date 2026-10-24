using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Commands.Extension.List.Models;
using OpenForge.Cli.Core.Commands.Extension.List.Shared.Result;
using OpenForge.Cli.Core.Framework.Extensions;
using OpenForge.Cli.Core.Framework.Extensions.Operational;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;

namespace OpenForge.Cli.Core.Commands.Extension.List;

internal sealed class ExtensionListOperation(
    ExtensionSourceReader sourceReader,
    PhysicalPathResolver physicalPathResolver,
    ExtensionLifecycleDoctorReader doctorReader)
{
    private readonly ExtensionSourceReader _sourceReader = sourceReader;
    private readonly PhysicalPathResolver _physicalPathResolver = physicalPathResolver;
    private readonly ExtensionLifecycleDoctorReader _doctorReader = doctorReader;

    internal async ValueTask<ExtensionListResult> ExecuteAsync(
        ExtensionListRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var source = await _sourceReader
                .ReadAsync(request.Workspace, request.ExplicitSource, cancellationToken)
                .ConfigureAwait(false);
            var observation = request.Selection.Installed
                ? await new WorkspaceLayoutReader(_physicalPathResolver).ReadAsync(request.Workspace, cancellationToken).ConfigureAwait(false)
                : null;
            var lifecycle = observation?.Ownership;
            var doctor = observation is { Ownership.IsTrustworthy: true } observed && observed.Ownership.Document.Extensions.Length > 0
                ? await _doctorReader.ReadAsync(
                    observed,
                    includeEmbedded: false,
                    cancellationToken).ConfigureAwait(false)
                : null;
            return ExtensionListResultBuilder.Build(request, source, lifecycle, doctor);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return ExtensionListResultBuilder.Event(
                request,
                CliSemanticStatus.Interrupted,
                ExtensionListFindingCode.Interrupted,
                "Extension List was interrupted.");
        }
        catch (Exception exception) when (exception is IOException or InvalidOperationException or UnauthorizedAccessException)
        {
            return ExtensionListResultBuilder.Event(
                request,
                CliSemanticStatus.Failed,
                ExtensionListFindingCode.OperationFailed,
                exception.Message);
        }
    }
}
