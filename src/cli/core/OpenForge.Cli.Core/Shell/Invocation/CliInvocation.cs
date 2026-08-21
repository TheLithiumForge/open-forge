using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Shell.Invocation;

internal sealed record CliInvocation(
    CliProcessIdentity Process,
    CliPresentation Presentation,
    CliTerminalMode TerminalMode,
    CliWorkspaceRequest WorkspaceRequest,
    CliWorkspace? Workspace);

internal sealed record CliProcessEnvironment
{
    internal CliProcessEnvironment(string currentDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(currentDirectory);
        CurrentDirectory = currentDirectory;
    }

    internal string CurrentDirectory { get; }
}
