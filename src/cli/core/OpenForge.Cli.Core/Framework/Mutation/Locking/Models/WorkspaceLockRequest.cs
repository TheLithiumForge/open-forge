using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Framework.Mutation.Locking.Models;

internal sealed record WorkspaceLockRequest
{
    private const int MaximumCommandLength = 128;
    internal const string DirectoryName = ".agents";
    internal const string FileName = "open-forge.lock";
    internal const string RelativePath = ".agents/open-forge.lock";

    internal WorkspaceLockRequest(
        CliWorkspace? workspace,
        string command,
        Guid operationId)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentException.ThrowIfNullOrWhiteSpace(command);
        if (command.Length > MaximumCommandLength)
        {
            throw new ArgumentException(
                $"A lock command identity cannot exceed {MaximumCommandLength} characters.",
                nameof(command));
        }

        if (operationId == Guid.Empty)
        {
            throw new ArgumentException("A lock operation ID cannot be empty.", nameof(operationId));
        }

        Workspace = workspace;
        Command = command;
        OperationId = operationId;
    }

    internal CliWorkspace Workspace { get; }

    internal string Command { get; }

    internal Guid OperationId { get; }

    internal string LogicalPath => Path.Combine(Workspace.LexicalRoot, DirectoryName, FileName);
}
