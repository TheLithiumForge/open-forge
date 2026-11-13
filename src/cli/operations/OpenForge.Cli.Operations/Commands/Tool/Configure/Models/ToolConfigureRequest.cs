using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Writing;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Tool.Configure.Models;

internal sealed record ToolConfigureRequest
{
    public required CliWorkspace Workspace { get; init; }
    public required ToolWriteOptions Options { get; init; }
    public required string Id { get; init; }
    public string[]? Hosts { get; init; }
    public string? CodeRoot { get; init; }
    public ToolComponentChoice[] Components { get; init; } = [];
}
