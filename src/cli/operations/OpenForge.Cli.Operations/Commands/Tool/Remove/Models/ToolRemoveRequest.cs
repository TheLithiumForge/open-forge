using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Writing;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Tool.Remove.Models;

internal sealed record ToolRemoveRequest
{
    public required CliWorkspace Workspace { get; init; }
    public required ToolWriteOptions Options { get; init; }
    public required string Id { get; init; }
}
