using System.ComponentModel;
using OpenForge.Cli.Composition.Shared.Tools;
using OpenForge.Cli.Core.Commands.Tool.Run;
using OpenForge.Cli.Core.Commands.Tool.Run.Models;
using OpenForge.Cli.Core.Commands.Tool.Run.Shared.Reporting;
using OpenForge.Cli.Core.Commands.Tool.Shared.Reporting;
using OpenForge.Cli.OutputText.Tool.Run;

namespace OpenForge.Cli.Hosting.Shared.Tools;

// Registration point: root composition calls this raw-stream path before its report pipeline.
internal static class ToolRunHost
{
    internal static async ValueTask<int> RunAsync(ToolRunRequest request, ToolRunSession session, TextWriter errors, CancellationToken token)
    {
        var resolution = await session.ResolveAsync(request, token).ConfigureAwait(false);
        if (resolution is ToolRunRefused refused) return await RefuseAsync(refused.Failure, errors).ConfigureAwait(false);
        var ready = (ToolRunReady)resolution;
        try { return await ToolDispatchProcessHost.RunAsync(ready.Dispatch, token).ConfigureAwait(false); }
        catch (OperationCanceledException)
        {
            return await RefuseAsync(new(ToolRunFailureCode.Cancelled, request.Id, "Tool Run was cancelled before launch.", false)
            { Workspace = ready.Workspace }, errors).ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is Win32Exception or IOException or UnauthorizedAccessException or InvalidOperationException or ArgumentException)
        {
            return await RefuseAsync(new(ToolRunFailureCode.StartFailed, request.Id, exception.Message, false)
            { Workspace = ready.Workspace }, errors).ConfigureAwait(false);
        }
    }

    private static async ValueTask<int> RefuseAsync(ToolRunFailure failure, TextWriter errors)
    {
        var next = failure.Code == ToolRunFailureCode.DeclaredToolMissing ? ToolContractCodes.RestoreCommand : null;
        await errors.WriteLineAsync(ToolRunText.Refusal(ToolRunFailureCodes.Suffix(failure.Code), failure.Id, failure.Cause, next)).ConfigureAwait(false);
        return failure.ExitCode;
    }
}
