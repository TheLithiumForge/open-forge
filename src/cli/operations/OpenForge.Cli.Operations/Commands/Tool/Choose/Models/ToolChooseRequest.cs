using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Writing;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Tool.Choose.Models;

internal sealed record ToolChooseRequest
{
    public required CliWorkspace Workspace { get; init; }
    public required ToolWriteOptions Options { get; init; }
    public required string Role { get; init; }
    public required string Provider { get; init; }
}
