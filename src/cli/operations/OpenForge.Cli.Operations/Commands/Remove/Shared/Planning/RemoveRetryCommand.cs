using OpenForge.Cli.Core.Commands.Remove.Models.Request;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions.Shared.Commands;

namespace OpenForge.Cli.Core.Commands.Remove.Shared.Planning;

internal static class RemoveRetryCommand
{
    internal static string From(RemovePathRequest request, string? target = null, string kind = "path", bool confirmationRequired = false, bool diagnostics = false)
    {
        var arguments = new List<string> { "remove", target ?? request.Path, "--kind", kind };
        if (request.Workspace.SelectedBy == CliWorkspaceSelectionMethod.ExplicitWorkspace)
            CliRetryCommand.Add(arguments, "--workspace", request.Workspace.LexicalRoot);
        foreach (var path in request.AllowPaths) CliRetryCommand.Add(arguments, "--allow-path", path);
        if (request.Automatic || confirmationRequired) arguments.Add("--automatic");
        if (request.IsDryRun) arguments.Add("--dry-run");
        CliRetryCommand.AddPresentation(arguments, request.RetryOptions, diagnostics);
        return CliRetryCommand.Build(arguments);
    }
}
