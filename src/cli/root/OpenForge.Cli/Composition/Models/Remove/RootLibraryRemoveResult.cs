using OpenForge.Cli.Core.Commands.Library.Detach.Models.Result;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Composition.Models.Remove;

internal sealed record RootLibraryRemoveResult(LibraryDetachResult Operation) : ICliCommandResult
{
    public string Command => "remove";
    public CliSemanticStatus Status => Operation.Status;
    public CliWorkspace? Workspace => Operation.Workspace;
    public CliNextAction? Next => Operation.Next;
}
