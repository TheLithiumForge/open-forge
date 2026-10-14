using OpenForge.Cli.Core.Commands.Extension.Install.Models.Request;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions.Shared.Commands;

namespace OpenForge.Cli.Core.Commands.Extension.Install.Shared.Result;

internal static class ExtensionInstallRetryCommand
{
    internal static string From(ExtensionInstallRequest request, bool forcePreview = false, bool confirmationRequired = false, string? permission = null)
    {
        var arguments = new List<string> { "extension", "install" };
        if (request.All) arguments.Add("--all");
        else arguments.AddRange(request.RequestedIds);
        CliRetryCommand.Add(arguments, "--source", request.SourcePath);
        if (request.Workspace.SelectedBy == CliWorkspaceSelectionMethod.ExplicitWorkspace)
            CliRetryCommand.Add(arguments, "--workspace", request.Workspace.LexicalRoot);
        foreach (var path in request.AllowPath) CliRetryCommand.Add(arguments, "--allow-path", path);
        if (permission is not null && !request.AllowPath.Contains(permission, StringComparer.Ordinal))
            CliRetryCommand.Add(arguments, "--allow-path", permission);
        if (request.Automatic || confirmationRequired) arguments.Add("--automatic");
        if (request.Force || forcePreview) arguments.Add("--force");
        if (request.Mode == ExtensionInstallMode.DryRun || forcePreview) arguments.Add("--dry-run");
        CliRetryCommand.AddPresentation(arguments, request.RetryOptions);
        return CliRetryCommand.Build(arguments);
    }
}
