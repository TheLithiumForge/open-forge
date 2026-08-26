using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Parsing.Models;

namespace OpenForge.Cli.Core.Shell.Invocation;

internal sealed record CliInvocationResolution(
    CliInvocation? Invocation,
    CliInvalidInput? InvalidInput);

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
                SuppliedView = ReadSuppliedView(input),
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
                SuppliedView = ReadSuppliedView(input),
            });
        }

        var diagnostic = selection.Failure?.DirectCause
            ?? $"The selected workspace is {selection.State.ToString().ToLowerInvariant()}.";
        return new CliInvocationResolution(
            null,
            new CliInvalidInput(
                "cli.workspace.invalid",
                CliInvalidInputSource.Workspace,
                [diagnostic]));
    }

    private static CliInvocationResolution Selected(CliInvocation invocation)
    {
        return new CliInvocationResolution(invocation, null);
    }

    private static CliView? ReadSuppliedView(CliGlobalInput input)
        => input.ViewOccurrences == 0 ? null : input.View;
}
