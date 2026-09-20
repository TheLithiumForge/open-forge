using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Parsing.Models.Input;

namespace OpenForge.Cli.Core.Shell.Invocation;

internal static class CliInvocationResolver
{
    internal static CliInvocationResolution Resolve(
        CliGlobalInput input,
        CliProcessIdentity process,
        CliProcessEnvironment environment,
        CliWorkspaceRequirement workspaceRequirement,
        CliWorkspaceSelector workspaceSelector)
    {
        CliPresentationDefinitions.Validate(input.Presentation);
        if (!Enum.IsDefined(workspaceRequirement))
        {
            throw new ArgumentOutOfRangeException(
                nameof(workspaceRequirement),
                workspaceRequirement,
                "The workspace requirement is not defined.");
        }

        var request = new CliWorkspaceRequest(input.WorkspaceValue, environment.CurrentDirectory);
        if (workspaceRequirement == CliWorkspaceRequirement.Absent)
        {
            return Selected(new CliInvocation(
                process,
                input.Presentation,
                CliTerminalMode.None,
                request,
                null)
            {
                SuppliedDetail = ReadSuppliedDetail(input),
            });
        }

        var selection = workspaceSelector.Select(request);
        if (selection.State == CliWorkspaceSelectionState.Selected)
        {
            return Selected(new CliInvocation(
                process,
                input.Presentation,
                CliTerminalMode.None,
                request,
                selection.Workspace)
            {
                SuppliedDetail = ReadSuppliedDetail(input),
            });
        }

        var diagnostic = selection.Failure?.DirectCause
            ?? global::OpenForge.Cli.OutputText.Shared.ShellText.WorkspaceUnavailable(selection.State.ToString().ToLowerInvariant());
        return new CliInvocationResolution(
            null,
            new CliInvalidInput(
                "cli.workspace.invalid",
                CliInvalidInputSource.Workspace,
                [diagnostic]))
        {
            WorkspaceSelectionState = selection.State,
        };
    }

    private static CliInvocationResolution Selected(CliInvocation invocation)
    {
        return new CliInvocationResolution(invocation, null);
    }

    private static CliDetail? ReadSuppliedDetail(CliGlobalInput input)
        => input.DetailOccurrences == 0 ? null : input.Detail;
}
