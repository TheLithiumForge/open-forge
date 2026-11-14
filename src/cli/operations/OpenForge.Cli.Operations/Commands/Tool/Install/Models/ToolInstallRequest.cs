using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Writing;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Tool.Install.Models;

internal sealed record ToolInstallRequest
{
    public required CliWorkspace Workspace { get; init; }
    public required ToolWriteOptions Options { get; init; }
    public string? Id { get; init; }
    public string? Source { get; init; }
    public string? Version { get; init; }
    public string[]? Hosts { get; init; }
    public string? CodeRoot { get; init; }
    public bool AllowDownload { get; init; }
    public ToolComponentChoice[] Components { get; init; } = [];
}
