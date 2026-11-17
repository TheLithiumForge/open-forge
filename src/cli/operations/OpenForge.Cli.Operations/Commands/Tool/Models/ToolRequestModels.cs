using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Tool.Models;

internal enum ToolCommandKind
{
    List,
    Install,
    Remove,
}

internal sealed record ToolListRequest(CliWorkspace Workspace);

internal sealed record ToolWriteRequest
{
    public required ToolCommandKind Kind { get; init; }
    public required CliWorkspace Workspace { get; init; }

    /// <summary>The named tool, or null for bare Install.</summary>
    public string? Id { get; init; }
    public bool Yes { get; init; }
    public bool Preview { get; init; }
    public required CliFormat Format { get; init; }
}
