using OpenForge.Cli.Core.Commands.Tool.Run.Models;
using OpenForge.Cli.Core.Commands.Tool.Install.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Tool.Run;

internal sealed class ToolRunSession(ToolResolveRun resolve, ToolRunInstallService? install = null)
{
    internal async ValueTask<ToolRunResolution> ResolveAsync(ToolRunRequest request, CancellationToken token)
    {
        var result = await resolve(request, token).ConfigureAwait(false);
        if (!request.Interactive || install is null || result is not ToolRunRefused
            { Failure: { Code: ToolRunFailureCode.DeclaredToolMissing, OfferInstall: true, Workspace: { } workspace } }) return result;
        ToolInstallResult? installed;
        try
        {
            installed = await install(new()
            {
                Workspace = workspace,
                // Bare Install restores settings pins against their locks instead of selecting a catalogue default.
                Id = null,
                Options = new() { AllowInteraction = true },
            }, token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return new ToolRunRefused(new(ToolRunFailureCode.Cancelled, request.Id, "Tool installation was cancelled before dispatch.", false) { Workspace = workspace });
        }
        if (installed?.Status == CliSemanticStatus.Interrupted)
            return new ToolRunRefused(new(ToolRunFailureCode.Cancelled, request.Id, "Tool installation was cancelled before dispatch.", false) { Workspace = workspace });
        if (installed is null) return result;
        // Use the already selected workspace even if CWD or host configuration changes during consent.
        return await resolve(request with { WorkspacePath = workspace.LexicalRoot, HostProjectDirectory = null }, token).ConfigureAwait(false);
    }
}
