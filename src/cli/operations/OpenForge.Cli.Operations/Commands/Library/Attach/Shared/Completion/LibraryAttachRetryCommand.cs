using OpenForge.Cli.Core.Commands.Library.Attach.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions.Shared.Commands;

namespace OpenForge.Cli.Core.Commands.Library.Attach.Shared.Completion;

internal static class LibraryAttachRetryCommand
{
    internal static string From(LibraryAttachRequest request, IEnumerable<string> missingPermissions)
    {
        var arguments = new List<string> { "library", "attach", request.LibraryId.Value, request.SourceRoot.Value };
        CliRetryCommand.Add(arguments, "--to", request.DestinationRoot.Value);
        if (request.Workspace.SelectedBy == CliWorkspaceSelectionMethod.ExplicitWorkspace)
            CliRetryCommand.Add(arguments, "--workspace", request.Workspace.LexicalRoot);
        if (request.GitIgnore is { } ignore) CliRetryCommand.Add(arguments, "--git-ignore", ignore ? "true" : "false");
        foreach (var path in request.Allow.Concat(missingPermissions).Distinct(StringComparer.Ordinal))
            CliRetryCommand.Add(arguments, "--allow-path", path);
        if (request.Automatic) arguments.Add("--automatic");
        if (request.Mode == LibraryMode.DryRun) arguments.Add("--dry-run");
        CliRetryCommand.AddPresentation(arguments, request.RetryOptions);
        return CliRetryCommand.Build(arguments);
    }
}
