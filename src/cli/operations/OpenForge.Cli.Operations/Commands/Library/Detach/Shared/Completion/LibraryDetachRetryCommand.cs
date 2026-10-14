using OpenForge.Cli.Core.Commands.Library.Detach.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions.Shared.Commands;

namespace OpenForge.Cli.Core.Commands.Library.Detach.Shared.Completion;

internal static class LibraryDetachRetryCommand
{
    internal static string From(LibraryDetachRequest request, IEnumerable<string> missingPermissions)
    {
        var arguments = new List<string> { "library", "detach", request.LibraryId.Value };
        if (request.Workspace.SelectedBy == CliWorkspaceSelectionMethod.ExplicitWorkspace)
            CliRetryCommand.Add(arguments, "--workspace", request.Workspace.LexicalRoot);
        foreach (var path in request.Allow.Concat(missingPermissions).Distinct(StringComparer.Ordinal))
            CliRetryCommand.Add(arguments, "--allow-path", path);
        if (request.Automatic) arguments.Add("--automatic");
        if (request.Mode == LibraryMode.DryRun) arguments.Add("--dry-run");
        CliRetryCommand.AddPresentation(arguments, request.RetryOptions);
        return CliRetryCommand.Build(arguments);
    }
}
