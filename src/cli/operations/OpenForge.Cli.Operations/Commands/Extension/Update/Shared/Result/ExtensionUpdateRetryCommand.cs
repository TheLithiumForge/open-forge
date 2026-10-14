using OpenForge.Cli.Core.Commands.Extension.Update.Models.Request;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions.Shared.Commands;

namespace OpenForge.Cli.Core.Commands.Extension.Update.Shared.Result;

internal static class ExtensionUpdateRetryCommand
{
    internal static string From(ExtensionUpdateRequest request, bool prunePreview = false, bool confirmationRequired = false)
    {
        var arguments = new List<string> { "extension", "update" };
        if (request.All) arguments.Add("--all");
        else arguments.AddRange(request.RequestedIds);
        CliRetryCommand.Add(arguments, "--source", request.SourcePath);
        if (request.Workspace.SelectedBy == CliWorkspaceSelectionMethod.ExplicitWorkspace)
            CliRetryCommand.Add(arguments, "--workspace", request.Workspace.LexicalRoot);
        foreach (var path in request.AllowPath) CliRetryCommand.Add(arguments, "--allow-path", path);
        if (request.Force) arguments.Add("--force");
        if (request.Prune || prunePreview) arguments.Add("--prune");
        if (request.Automatic || confirmationRequired) arguments.Add("--automatic");
        if (request.Mode == ExtensionUpdateMode.DryRun || prunePreview) arguments.Add("--dry-run");
        CliRetryCommand.AddPresentation(arguments, request.RetryOptions);
        return CliRetryCommand.Build(arguments);
    }
}
