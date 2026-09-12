using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

internal interface ICliCommandResult
{
    string Command { get; }

    CliSemanticStatus Status { get; }

    CliWorkspace? Workspace { get; }

    CliNextAction? Next { get; }
}
