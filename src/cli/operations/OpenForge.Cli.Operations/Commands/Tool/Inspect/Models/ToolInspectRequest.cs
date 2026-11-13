using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Tool.Inspect.Models;

internal sealed record ToolInspectRequest(CliWorkspace Workspace, string Id, string? Source, string? Version);
