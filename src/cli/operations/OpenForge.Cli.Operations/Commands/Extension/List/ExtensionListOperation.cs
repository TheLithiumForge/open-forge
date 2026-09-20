using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Commands.Extension.List.Models;
using OpenForge.Cli.Core.Commands.Extension.List.Shared.Result;
using OpenForge.Cli.Core.Framework.Extensions;
using OpenForge.Cli.Core.Framework.Extensions.Operational;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;
using OpenForge.Cli.Core.Shell.Definitions;

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
            var lifecycle = request.Selection.Installed
                ? await WorkspaceOwnershipReader.ReadAsync(_physicalPathResolver, request.Workspace, cancellationToken).ConfigureAwait(false)
                : null;
            var doctor = lifecycle is { IsTrustworthy: true } && lifecycle.Document.Extensions.Length > 0
                ? await _doctorReader.ReadAsync(
                    request.Workspace,
                    lifecycle,
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
